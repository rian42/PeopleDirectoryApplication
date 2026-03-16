using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PeopleDirectoryApplication.Application.Models;
using PeopleDirectoryApplication.Data;
using PeopleDirectoryApplication.Models;

namespace PeopleDirectoryApplication.Infrastructure.Services;

public class EmailOutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IOptionsMonitor<EmailNotificationOptions> _optionsMonitor;
    private readonly ILogger<EmailOutboxProcessor> _logger;

    public EmailOutboxProcessor(
        IServiceScopeFactory serviceScopeFactory,
        IOptionsMonitor<EmailNotificationOptions> optionsMonitor,
        ILogger<EmailOutboxProcessor> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var options = _optionsMonitor.CurrentValue;
            var pollDelay = TimeSpan.FromSeconds(Math.Max(1, options.PollingIntervalSeconds));

            if (!options.Enabled)
            {
                await Task.Delay(pollDelay, stoppingToken);
                continue;
            }

            try
            {
                await ProcessPendingJobsAsync(options, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while processing email outbox jobs");
            }

            await Task.Delay(pollDelay, stoppingToken);
        }
    }

    private async Task ProcessPendingJobsAsync(EmailNotificationOptions options, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var nowUtc = DateTime.UtcNow;
        var batchSize = Math.Max(1, options.ProcessingBatchSize);

        var pendingJobs = await dbContext.EmailNotificationJobs
            .Where(job => job.Status == EmailNotificationJobStatus.Pending && job.NextAttemptAtUtc <= nowUtc)
            .OrderBy(job => job.CreatedAtUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        foreach (var job in pendingJobs)
        {
            job.Status = EmailNotificationJobStatus.Processing;
            job.LastAttemptAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);

            try
            {
                await SendMessageAsync(job, options, cancellationToken);
                job.Status = EmailNotificationJobStatus.Sent;
                job.ProcessedAtUtc = DateTime.UtcNow;
                job.LastError = null;

                await dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Email outbox job {EmailJobId} sent successfully", job.Id);
            }
            catch (Exception ex)
            {
                job.AttemptCount += 1;
                job.LastError = ex.Message;

                if (job.AttemptCount >= Math.Max(1, options.MaxRetryAttempts))
                {
                    job.Status = EmailNotificationJobStatus.DeadLetter;
                    job.DeadLetteredAtUtc = DateTime.UtcNow;
                    _logger.LogError(ex, "Email outbox job {EmailJobId} moved to dead letter after {AttemptCount} attempts", job.Id, job.AttemptCount);
                }
                else
                {
                    job.Status = EmailNotificationJobStatus.Pending;
                    var retryDelaySeconds = Math.Max(1, options.RetryBaseDelaySeconds) * (int)Math.Pow(2, job.AttemptCount - 1);
                    job.NextAttemptAtUtc = DateTime.UtcNow.AddSeconds(retryDelaySeconds);
                    _logger.LogWarning(ex, "Email outbox job {EmailJobId} failed attempt {AttemptCount}. Retrying at {NextAttemptUtc}", job.Id, job.AttemptCount, job.NextAttemptAtUtc);
                }

                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private static async Task SendMessageAsync(
        EmailNotificationJob job,
        EmailNotificationOptions options,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.SmtpHost))
        {
            throw new InvalidOperationException("SMTP host is required for email delivery.");
        }

        using var smtpClient = new SmtpClient(options.SmtpHost, options.SmtpPort)
        {
            EnableSsl = options.EnableSsl,
            UseDefaultCredentials = false,
        };

        if (!string.IsNullOrWhiteSpace(options.Username))
        {
            smtpClient.Credentials = new NetworkCredential(options.Username, options.Password);
        }

        using var mailMessage = new MailMessage(job.FromAddress, job.ToAddress, job.Subject, job.Body);
        cancellationToken.ThrowIfCancellationRequested();
        await smtpClient.SendMailAsync(mailMessage, cancellationToken);
    }
}
