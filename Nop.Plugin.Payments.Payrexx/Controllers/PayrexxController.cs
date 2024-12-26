using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.Payrexx.Models;
using Nop.Plugin.Payments.Payrexx.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.Payrexx.Controllers;

[Area(AreaNames.ADMIN)]
[HttpsRequirement]
[AutoValidateAntiforgeryToken]
[ValidateIpAddress]
[AuthorizeAdmin]
public class PayrexxController : BasePaymentController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly ISettingService _settingService;
    private readonly IWebHelper _webHelper;
    private readonly PayrexxManager _payrexxManager;
    private readonly PayrexxSettings _payrexxSettings;

    #endregion

    #region Ctor

    public PayrexxController(ILocalizationService localizationService,
        INotificationService notificationService,
        ISettingService settingService,
        IWebHelper webHelper,
        PayrexxManager payrexxManager,
        PayrexxSettings payrexxSettings)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _webHelper = webHelper;
        _payrexxManager = payrexxManager;
        _payrexxSettings = payrexxSettings;
    }

    #endregion

    #region Methods

    [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
    public async Task<IActionResult> Configure()
    {
        var model = new ConfigurationModel
        {
            InstanceName = _payrexxSettings.InstanceName,
            SecretKey = _payrexxSettings.SecretKey,
            WebhookUrl = Url.RouteUrl(PayrexxDefaults.WebhookRouteName, null, _webHelper.GetCurrentRequestProtocol())
        };

        return View("~/Plugins/Payments.Payrexx/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        _payrexxSettings.InstanceName = model.InstanceName;
        _payrexxSettings.SecretKey = model.SecretKey;
        await _settingService.SaveSettingAsync(_payrexxSettings);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        //validate credentials
        var (credentialsValid, error) = await _payrexxManager.CheckSignatureAsync();
        if (credentialsValid)
            _notificationService.SuccessNotification("Credentials entered are valid");
        else if (string.IsNullOrEmpty(error))
            _notificationService.WarningNotification("Credentials entered are invalid");
        else
            _notificationService.ErrorNotification(error);

        return await Configure();
    }

    #endregion
}