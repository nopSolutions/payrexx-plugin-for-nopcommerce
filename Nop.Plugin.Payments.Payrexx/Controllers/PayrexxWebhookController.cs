using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.Payrexx.Services;

namespace Nop.Plugin.Payments.Payrexx.Controllers
{
    public class PayrexxWebhookController : Controller
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
        public async Task<IActionResult> WebhookHandler()
        {
            await _payrexxManager.HandleWebhookTransactionAsync(HttpContext);
            return Ok();
        }

        #endregion
    }
}