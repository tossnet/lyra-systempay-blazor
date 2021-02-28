using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Blazor_SystemPay.Configurations;

namespace Blazor_SystemPay.Services
{
    public class SystemPayService : ISystemPayService
    {
        
        private HttpClient _client;
        private IOptions<SystemPay> _systemPayConfig;

        public SystemPayService(
                            HttpClient client,
                            IOptions<SystemPay> systemPayConfig)
        {
            _client = client;
            _systemPayConfig = systemPayConfig;
        }

        public async Task<string> GetFormToken(string JSON_Order)
        {
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", _systemPayConfig.Value.Authorization);

            var data = new StringContent(JSON_Order, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(_systemPayConfig.Value.API, data);

            var res = await response.Content.ReadAsStringAsync();

            var json = System.Text.Json.JsonDocument.Parse(res);
            return json.RootElement.GetProperty("answer").GetProperty("formToken").ToString();
        }
    }
}