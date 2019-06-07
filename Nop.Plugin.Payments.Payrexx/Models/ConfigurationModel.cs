using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.Payrexx.Models
{
    /// <summary>
    /// Represents configuration model
    /// </summary>
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Payments.Payrexx.Fields.InstanceName")]
        public string InstanceName { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Payrexx.Fields.SecretKey")]
        public string SecretKey { get; set; }
    }
}