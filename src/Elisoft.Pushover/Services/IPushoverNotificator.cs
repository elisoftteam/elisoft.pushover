namespace Elisoft.Pushover.Services
{
    public interface IPushoverNotificator
    {
        Task SendMessageAsync(string apiToken, string userKey, string message, string? title = null, int? priority = null);
    }
}
