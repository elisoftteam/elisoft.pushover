using Microsoft.Extensions.Logging;

namespace Elisoft.Pushover.Services
{
    public class PushoverNotificator : IPushoverNotificator
    {
        private const string PushoverEndpoint = "https://api.pushover.net/1/messages.json";
        private readonly HttpClient _httpClient;
        private readonly ILogger<PushoverNotificator> _logger;

        public PushoverNotificator(HttpClient httpClient, ILogger<PushoverNotificator> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendMessageAsync(string apiToken, string userKey, string message, string? title = null, int? priority = null)
        {
            if (string.IsNullOrWhiteSpace(apiToken))
            {
                throw new ArgumentException("API Token cannot be null or empty.", nameof(apiToken));
            }

            if (string.IsNullOrWhiteSpace(userKey))
            {
                throw new ArgumentException("User Key cannot be null or empty.", nameof(userKey));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));
            }
            
            var payload = new Dictionary<string, string>
            {
                ["token"] = apiToken,
                ["user"] = userKey,
                ["message"] = message
            };

            if (!string.IsNullOrWhiteSpace(title))
            {
                payload["title"] = title;
            }

            if (priority.HasValue)
            {
                payload["priority"] = priority.Value.ToString();
            }

            try
            {
                using var content = new FormUrlEncodedContent(payload);
                var response = await _httpClient.PostAsync(PushoverEndpoint, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Pushover notification.");
                throw;
            }
        }
    }
}
