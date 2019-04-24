using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Plugin.Payments.Payrexx.Domain;
using Nop.Services.Logging;

namespace Nop.Plugin.Payments.Payrexx.Services
{
    /// <summary>
    /// Represents the service manager
    /// </summary>
    public class PayrexxManager
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly PayrexxSettings _payrexxSettings;

        private readonly HttpClient _httpClient;

        #endregion

        #region Ctor

        public PayrexxManager(ILogger logger,
            IWorkContext workContext,
            PayrexxSettings payrexxSettings)
        {
            _logger = logger;
            _workContext = workContext;
            _payrexxSettings = payrexxSettings;

            //create HTTP client
            _httpClient = new HttpClient { BaseAddress = new Uri(PayrexxDefaults.ApiServiceUrl) };
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, MimeTypes.ApplicationJson);
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, PayrexxDefaults.UserAgent);
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Handle function and get result
        /// </summary>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="function">Function</param>
        /// <returns>Result; error message if exists</returns>
        private (TResult Result, string ErrorMessage) HandleFunction<TResult>(Func<TResult> function)
        {
            try
            {
                //ensure that plugin is configured
                if (!IsConfigured())
                    throw new NopException("Plugin not configured");

                //invoke function
                return (function(), null);
            }
            catch (Exception exception)
            {
                //log errors
                var errorMessage = $"{PayrexxDefaults.SystemName} error: {Environment.NewLine}{exception.Message}";
                _logger.Error(errorMessage, exception, _workContext.CurrentCustomer);

                return (default(TResult), errorMessage);
            }
        }

        /// <summary>
        /// Check whether the plugin is configured
        /// </summary>
        /// <returns>Result</returns>
        private bool IsConfigured()
        {
            //instance name and secret key are required to request services
            return !string.IsNullOrEmpty(_payrexxSettings.InstanceName) && !string.IsNullOrEmpty(_payrexxSettings.SecretKey);
        }

        /// <summary>
        /// Handle request and get response
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResponseData">Response data type</typeparam>
        /// <param name="request">Request</param>
        /// <returns>Response; error message if exists</returns>
        private (TResponseData ResponseData, string ErrorMessage) HandleRequest<TRequest, TResponseData>(TRequest request)
            where TRequest : Request where TResponseData : ResponseData
        {
            return HandleFunction(() =>
            {
                //execute request
                var response = RequestAsync<TRequest, TResponseData>(request)?.Result
                    ?? throw new NopException("No response from service");

                //check whether request was successfull
                if (response.Status != ResponseStatus.Success)
                    throw new NopException($"Request status - {response.Status}. {Environment.NewLine}{response.ErrorMessage}");

                return response.Data;
            });
        }

        /// <summary>
        /// Request API service
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResponseData">Response data type</typeparam>
        /// <param name="request">Request</param>
        /// <returns>The asynchronous task whose result contains response details</returns>
        private async Task<Response<TResponseData>> RequestAsync<TRequest, TResponseData>(TRequest request)
            where TRequest : Request where TResponseData : ResponseData
        {
            try
            {
                //prepare request parameters
                var requestString = JsonConvert.SerializeObject(request);
                var parameters = JsonConvert.DeserializeObject<IDictionary<string, object>>(requestString)
                    .Where(parameter => parameter.Value != null)
                    .ToDictionary(parameter => parameter.Key,
                        parameter => parameter.Value is JArray array ? string.Join(',', array.Values()) : parameter.Value.ToString());
                var additionalFields = (request as CreateGatewayRequest)?.AdditionalFields ?? new List<(string Name, string Value)>();
                foreach (var (name, value) in additionalFields)
                {
                    parameters.Add($"fields[{name}]{(value != null ? "[value]" : string.Empty)}", value);
                }
                parameters.Add(PayrexxDefaults.RequestSignatureParameter,
                    CreateSignature(await new FormUrlEncodedContent(parameters).ReadAsStringAsync()));
                var requestContent = new StringContent((await new FormUrlEncodedContent(parameters).ReadAsStringAsync()).Replace("+", "%20"),
                    Encoding.GetEncoding("iso-8859-1"), MimeTypes.ApplicationXWwwFormUrlencoded);

                //execute request and get response
                var query = request.Method == HttpMethods.Get ? $"&{await requestContent.ReadAsStringAsync()}" : string.Empty;
                var path = $"{request.Path}?{PayrexxDefaults.RequestInstanceParameter}={_payrexxSettings.InstanceName}{query}";
                var requestMessage = new HttpRequestMessage(new HttpMethod(request.Method), path) { Content = requestContent };
                var httpResponse = await _httpClient.SendAsync(requestMessage);

                //return result
                var responseString = await httpResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Response<TResponseData>>(responseString);
            }
            catch (AggregateException exception)
            {
                //rethrow actual exceptions
                foreach (var innerException in exception.InnerExceptions)
                {
                    throw innerException;
                }

                return default(Response<TResponseData>);
            }
        }

        /// <summary>
        /// Create signature based on the passed message and API secret key
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>Signature value</returns>
        private string CreateSignature(string message)
        {
            var keyByte = new UTF8Encoding().GetBytes(_payrexxSettings.SecretKey);
            var messageBytes = new UTF8Encoding().GetBytes(message);
            var hash = new HMACSHA256(keyByte).ComputeHash(messageBytes);
            var signature = Convert.ToBase64String(hash);

            return signature;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Check whether signature based on plugin credentials is valid
        /// </summary>
        /// <returns>Result; error message if exists</returns>
        public (bool Result, string ErrorMessage) CheckSignature()
        {
            var (response, errorMessage) = HandleRequest<SignatureRequest, ResponseData>(new SignatureRequest());
            return (response != null, errorMessage);
        }

        /// <summary>
        /// Get gateway details
        /// </summary>
        /// <param name="gatewayId">Gateway identifier</param>
        /// <returns>Gateway; error message if exists</returns>
        public (Gateway Gateway, string ErrorMessage) GetGateway(string gatewayId)
        {
            return HandleRequest<GetGatewayRequest, Gateway>(new GetGatewayRequest { Id = gatewayId });
        }

        /// <summary>
        /// Create gateway
        /// </summary>
        /// <param name="request">Request details to create gateway</param>
        /// <returns>Gateway; error message if exists</returns>
        public (Gateway Gateway, string ErrorMessage) CreateGateway(CreateGatewayRequest request)
        {
            return HandleRequest<CreateGatewayRequest, Gateway>(request);
        }

        /// <summary>
        /// Capture transaction
        /// </summary>
        /// <param name="transactionId">Transaction identifer</param>
        /// <param name="amount">Amount to capture</param>
        /// <returns>Transaction; error message if exists</returns>
        public (Transaction Transaction, string ErrorMessage) CaptureTransaction(string transactionId, int amount)
        {
            var request = new CaptureTransactionRequest { Id = transactionId, TotalAmount = amount };
            return HandleRequest<CaptureTransactionRequest, Transaction>(request);
        }

        /// <summary>
        /// Get transaction details from the webhook request
        /// </summary>
        /// <param name="httpRequest">Request</param>
        /// <returns>Transaction; raw request data</returns>
        public (Transaction Transaction, string RawRequestString) GetTransactionFromWebhookRequest(HttpRequest httpRequest)
        {
            //get transaction from request
            var (result, errorMessage) = HandleFunction(() =>
            {
                try
                {
                    using (var streamReader = new StreamReader(httpRequest.Body))
                    {
                        var rawRequestString = streamReader.ReadToEnd();
                        var transaction = JsonConvert.DeserializeObject<Webhook>(rawRequestString)?.Transaction;
                        return (transaction, rawRequestString);
                    }
                }
                catch (WebException exception)
                {
                    var response = (HttpWebResponse)exception.Response;
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        var error = streamReader.ReadToEnd();
                        throw new NopException($"Webhook error: {Environment.NewLine}{error}", exception);
                    }
                }
            });

            return result;
        }

        #endregion
    }
}