using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PeopleDirectoryApplication.Application.Contracts.Repositories;
using PeopleDirectoryApplication.Application.Contracts.Services;
using PeopleDirectoryApplication.Application.Models;
using PeopleDirectoryApplication.Data;
using PeopleDirectoryApplication.Infrastructure.Repositories;
using PeopleDirectoryApplication.Infrastructure.Services;

namespace PeopleDirectoryApplication.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("PeopleDirectoryApplication.Web");
            });
        });

        services.Configure<EmailNotificationOptions>(configuration.GetSection(EmailNotificationOptions.SectionName));
        services.AddHttpContextAccessor();

        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IAuditTrailRepository, AuditTrailRepository>();
        services.AddScoped<ICurrentUserAccessor, HttpCurrentUserAccessor>();
        services.AddScoped<IEmailNotificationService, QueuedEmailNotificationService>();
        services.AddHostedService<EmailOutboxProcessor>();

        return services;
    }
}
