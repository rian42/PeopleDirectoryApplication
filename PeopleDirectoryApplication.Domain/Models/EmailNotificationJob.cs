using System.ComponentModel.DataAnnotations;

namespace PeopleDirectoryApplication.Models;

public class EmailNotificationJob
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string ToAddress { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string FromAddress { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public EmailNotificationJobStatus Status { get; set; } = EmailNotificationJobStatus.Pending;

    public int AttemptCount { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime NextAttemptAtUtc { get; set; }

    public DateTime? LastAttemptAtUtc { get; set; }

    public DateTime? ProcessedAtUtc { get; set; }

    public DateTime? DeadLetteredAtUtc { get; set; }

    [MaxLength(2000)]
    public string? LastError { get; set; }
}
