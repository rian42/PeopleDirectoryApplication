using Microsoft.Extensions.DependencyInjection;
using PeopleDirectoryApplication.Application.Contracts.Services;
using PeopleDirectoryApplication.Application.Services;

namespace PeopleDirectoryApplication.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPersonService, PersonService>();
        return services;
    }
}
