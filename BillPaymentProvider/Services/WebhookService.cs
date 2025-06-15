using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BillPaymentProvider.Services
{
    /// <summary>
    /// Service pour notifier un système tiers via webhook après un paiement
    /// </summary>
    public class WebhookService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _webhookUrl;
        private readonly bool _enabled;
        private readonly int _timeoutSeconds;

        public WebhookService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _webhookUrl = configuration["Webhook:Url"];
            _enabled = bool.TryParse(configuration["Webhook:Enabled"], out var enabled) ? enabled : false;
            _timeoutSeconds = int.TryParse(configuration["Webhook:TimeoutSeconds"], out var t) ? t : 5;
            _httpClient.Timeout = System.TimeSpan.FromSeconds(_timeoutSeconds);
        }

        public async Task NotifyAsync(object payload)
        {
            if (!_enabled || string.IsNullOrWhiteSpace(_webhookUrl)) return;
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                await _httpClient.PostAsync(_webhookUrl, content);
            }
            catch
            {
                // Ignorer les erreurs pour ne pas bloquer le paiement
            }
        }
    }
}
