using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.Payrexx.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(PayrexxDefaults.ConfigurationRouteName,
                "Plugins/Payrexx/Configure",
                new { controller = "Payrexx", action = "Configure", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(PayrexxDefaults.WebhookRouteName,
                "Plugins/Payrexx/Webhook",
                new { controller = "PayrexxWebhook", action = "WebhookHandler" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;
    }
}