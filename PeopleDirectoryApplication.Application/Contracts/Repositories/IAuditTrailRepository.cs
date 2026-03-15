using PeopleDirectoryApplication.Models;

namespace PeopleDirectoryApplication.Application.Contracts.Repositories;

public interface IAuditTrailRepository
{
    Task AddAsync(AuditTrailEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditTrailEntry>> GetByEntityAsync(string entityName, string entityId, int take, CancellationToken cancellationToken = default);
}
