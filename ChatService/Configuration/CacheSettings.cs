namespace ChatService.Configuration;

public class CacheSettings
{
    public int SizeLimit { get; set; }
    public string ExpirationScanFrequency { get; set; } = "00:05:00";
    public string MessageCacheDuration { get; set; } = "00:05:00";
    public bool CompressionEnabled { get; set; }

    public TimeSpan GetExpirationScanFrequency() => TimeSpan.Parse(ExpirationScanFrequency);
    public TimeSpan GetMessageCacheDuration() => TimeSpan.Parse(MessageCacheDuration);
}