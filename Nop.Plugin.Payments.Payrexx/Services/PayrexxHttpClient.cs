using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Nop.Plugin.Payments.Payrexx.Services
{
    /// <summary>
    /// Represents plugin HTTP client
    /// </summary>
    public class PayrexxHttpClient
    {
        #region Fields

        private readonly HttpClient _httpClient;
        private readonly PayrexxSettings _payrexxSettings;

        #endregion

        #region Ctor

        public PayrexxHttpClient(HttpClient httpClient,
            PayrexxSettings payrexxSettings)
        {
            //configure client
            httpClient.BaseAddress = new Uri(PayrexxDefaults.ApiServiceUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(payrexxSettings.RequestTimeout ?? 10);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, PayrexxDefaults.UserAgent);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, MimeTypes.ApplicationJson);

            _httpClient = httpClient;
            _payrexxSettings = payrexxSettings;
        }

        #endregion

        #region Utilities

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
        /// Request API service
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResponseData">Response data type</typeparam>
        /// <param name="request">Request</param>
        /// <returns>The asynchronous task whose result contains response details</returns>
        public async Task<Response<TResponseData>> RequestAsync<TRequest, TResponseData>(TRequest request)
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
                //rethrow actual exception
                throw exception.InnerException;
            }
        }

        #endregion
    }
}