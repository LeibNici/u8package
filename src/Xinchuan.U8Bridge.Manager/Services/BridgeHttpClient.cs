using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Xinchuan.U8Bridge.Manager.Services
{
    public sealed class BridgeHttpClient
    {
        public async Task<string> TestHealthAsync(string baseUrl)
        {
            using (var client = new HttpClient())
            {
                string url = ToClientBaseUrl(baseUrl) + "health";
                return await client.GetStringAsync(url).ConfigureAwait(false);
            }
        }

        public async Task<string> TestLoginAsync(BridgeConfig config)
        {
            string requestId = "manager-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            var body = new { requestId, profileName = config.DefaultProfileName };
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Request-ID", requestId);
                client.DefaultRequestHeaders.Add("X-API-KEY", config.ApiKey);
                string url = ToClientBaseUrl(config.BaseUrl) + "api/u8/login-test";
                string json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content).ConfigureAwait(false);
                string text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return ((int)response.StatusCode) + " " + response.ReasonPhrase + Environment.NewLine + text;
            }
        }

        private static string ToClientBaseUrl(string baseUrl)
        {
            string value = string.IsNullOrWhiteSpace(baseUrl) ? "http://+:8081/" : baseUrl;
            value = value.Replace("http://+:", "http://127.0.0.1:");
            value = value.Replace("http://*:", "http://127.0.0.1:");
            return value.EndsWith("/", StringComparison.Ordinal) ? value : value + "/";
        }
    }
}
