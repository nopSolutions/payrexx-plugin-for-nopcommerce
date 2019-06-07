using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.Payrexx.Models;
using Nop.Plugin.Payments.Payrexx.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Payments.Payrexx.Controllers
{
    public class PayrexxController : BaseAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly PayrexxManager _payrexxManager;
        private readonly PayrexxSettings _payrexxSettings;

        #endregion

        #region Ctor

        public PayrexxController(ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            PayrexxManager payrexxManager,
            PayrexxSettings payrexxSettings)
        {
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _payrexxManager = payrexxManager;
            _payrexxSettings = payrexxSettings;
        }

        #endregion

        #region Methods

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //prepare model
            var model = new ConfigurationModel
            {
                InstanceName = _payrexxSettings.InstanceName,
                SecretKey = _payrexxSettings.SecretKey
            };

            return View("~/Plugins/Payments.Payrexx/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            _payrexxSettings.InstanceName = model.InstanceName;
            _payrexxSettings.SecretKey = model.SecretKey;
            _settingService.SaveSetting(_payrexxSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            //validate credentials
            var (credentialsValid, error) = _payrexxManager.CheckSignature();
            if (credentialsValid)
                SuccessNotification("Credentials entered are valid");
            else if (string.IsNullOrEmpty(error))
                WarningNotification("Credentials entered are invalid");
            else
                ErrorNotification(error);

            return Configure();
        }

        #endregion
    }
}