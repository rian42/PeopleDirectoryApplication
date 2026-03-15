using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PeopleDirectoryApplication.Application.Contracts.Services;
using PeopleDirectoryApplication.Application.Models;
using PeopleDirectoryApplication.Data;
using PeopleDirectoryApplication.Models;

namespace PeopleDirectoryApplication.Infrastructure.Services;

public class QueuedEmailNotificationService : IEmailNotificationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly EmailNotificationOptions _options;
    private readonly ILogger<QueuedEmailNotificationService> _logger;

    public QueuedEmailNotificationService(
        ApplicationDbContext dbContext,
        IOptions<EmailNotificationOptions> options,
        ILogger<QueuedEmailNotificationService> logger)
    {
        _dbContext = dbContext;
        _options = options.Value;
        _logger = logger;
    }

    public Task SendPersonCreatedNotificationAsync(Person person, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(person);

        var subject = $"[PeopleDirectory] Person Created: {person.Name} {person.Surname}";
        var body =
$"""
New person record created.

Id: {person.Id}
Name: {person.Name} {person.Surname}
Country: {person.Country}
City: {person.City}
Gender: {person.Gender}
Email: {person.EmailAddress}
Mobile: {person.MobileNumber}
ProfilePicture: {person.ProfilePicture}
""";

        return QueueEmailAsync(subject, body, cancellationToken);
    }

    public Task SendPersonUpdatedNotificationAsync(
        Person previous,
        Person current,
        IReadOnlyCollection<PropertyChange> changes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(changes);

        var subject = $"[PeopleDirectory] Person Updated: {current.Name} {current.Surname} (Id: {current.Id})";
        var changeLines = changes.Select(change => $"{change.PropertyName}: '{change.OldValue ?? "(empty)"}' -> '{change.NewValue ?? "(empty)"}'");

        var body =
$"""
Person record updated.

Person Id: {current.Id}
Name: {current.Name} {current.Surname}

Changes:
{string.Join(Environment.NewLine, changeLines)}
""";

        return QueueEmailAsync(subject, body, cancellationToken);
    }

    private async Task QueueEmailAsync(string subject, string body, CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Email notifications disabled. Not queueing subject {EmailSubject}", subject);
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.ToAddress) || string.IsNullOrWhiteSpace(_options.FromAddress))
        {
            _logger.LogWarning("Email queue skipped because addresses are missing. Subject: {EmailSubject}", subject);
            return;
        }

        var nowUtc = DateTime.UtcNow;
        var job = new EmailNotificationJob
        {
            ToAddress = _options.ToAddress,
            FromAddress = _options.FromAddress,
            Subject = subject,
            Body = body,
            Status = EmailNotificationJobStatus.Pending,
            AttemptCount = 0,
            CreatedAtUtc = nowUtc,
            NextAttemptAtUtc = nowUtc,
        };

        _dbContext.EmailNotificationJobs.Add(job);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Queued email job {EmailJobId} with subject {EmailSubject}", job.Id, subject);
    }
}
