using System.Web.Http;
using Newtonsoft.Json.Serialization;
using Owin;
using Xinchuan.U8Bridge.Security;
using Xinchuan.U8Bridge.Services;

namespace Xinchuan.U8Bridge
{
    public sealed class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            config.MessageHandlers.Add(new ApiKeyHandler(ServiceRegistry.Options));
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling =
                Newtonsoft.Json.NullValueHandling.Include;
            app.UseWebApi(config);
        }
    }
}
