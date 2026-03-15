namespace PeopleDirectoryApplication.Application.Models;

public class EmailNotificationOptions
{
    public const string SectionName = "EmailNotifications";

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromAddress { get; set; } = "noreply@peopledirectory.local";
    public string ToAddress { get; set; } = "mark@bluegrassdigital.com";
    public bool Enabled { get; set; } = false;
    public int MaxRetryAttempts { get; set; } = 5;
    public int RetryBaseDelaySeconds { get; set; } = 30;
    public int ProcessingBatchSize { get; set; } = 20;
    public int PollingIntervalSeconds { get; set; } = 10;
}
