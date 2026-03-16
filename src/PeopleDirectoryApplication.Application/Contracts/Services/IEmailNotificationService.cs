using PeopleDirectoryApplication.Application.Models;
using PeopleDirectoryApplication.Models;

namespace PeopleDirectoryApplication.Application.Contracts.Services;

public interface IEmailNotificationService
{
    Task SendPersonCreatedNotificationAsync(Person person, CancellationToken cancellationToken = default);
    Task SendPersonUpdatedNotificationAsync(Person previous, Person current, IReadOnlyCollection<PropertyChange> changes, CancellationToken cancellationToken = default);
}
