using System.IO;
using System.Web.Http;
using Xinchuan.U8Bridge.Models;

namespace Xinchuan.U8Bridge.Controllers
{
    public sealed class HealthController : ApiController
    {
        [HttpGet]
        [Route("health")]
        public IHttpActionResult Health()
        {
            return Ok(new { success = true, message = "U8 Bridge is alive" });
        }

        [HttpGet]
        [Route("openapi.yaml")]
        public IHttpActionResult OpenApi()
        {
            string path = FindOpenApiPath();
            if (!File.Exists(path))
            {
                var fail = BridgeResponse.Fail(null, BridgeErrorCodes.RequestInvalid, "OpenAPI 文件不存在");
                return Content(fail.ToHttpStatus(), fail);
            }

            return ResponseMessage(new System.Net.Http.HttpResponseMessage
            {
                Content = new System.Net.Http.StringContent(File.ReadAllText(path), System.Text.Encoding.UTF8, "text/yaml")
            });
        }

        private static string FindOpenApiPath()
        {
            var dir = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null)
            {
                string path = Path.Combine(dir.FullName, "docs", "u8-bridge-openapi.yaml");
                if (File.Exists(path))
                {
                    return path;
                }

                dir = dir.Parent;
            }

            return "docs/u8-bridge-openapi.yaml";
        }
    }
}
