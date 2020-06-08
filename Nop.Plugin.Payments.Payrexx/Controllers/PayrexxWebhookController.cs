using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.Payrexx.Services;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.Payrexx.Controllers
{
    public class PayrexxWebhookController : BasePaymentController
    {
        #region Fields

        private readonly PayrexxManager _payrexxManager;

        #endregion

        #region Ctor

        public PayrexxWebhookController(PayrexxManager payrexxManager)
        {
            _payrexxManager = payrexxManager;
        }

        #endregion

        #region Methods

        [HttpPost]
        public IActionResult WebhookHandler()
        {
            _payrexxManager.HandleWebhookTransaction(HttpContext);
            return Ok();
        }

        #endregion
    }
}