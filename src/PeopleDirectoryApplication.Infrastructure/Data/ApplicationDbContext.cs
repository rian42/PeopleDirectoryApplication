using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PeopleDirectoryApplication.Models;

namespace PeopleDirectoryApplication.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Person> Persons => Set<Person>();
    public DbSet<AuditTrailEntry> AuditTrailEntries => Set<AuditTrailEntry>();
    public DbSet<EmailNotificationJob> EmailNotificationJobs => Set<EmailNotificationJob>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Person>()
            .Property(p => p.RowVersion)
            .IsRowVersion();

        builder.Entity<AuditTrailEntry>()
            .HasIndex(a => new { a.EntityName, a.EntityId, a.ChangedAtUtc });

        builder.Entity<EmailNotificationJob>()
            .HasIndex(j => new { j.Status, j.NextAttemptAtUtc });
    }
}
