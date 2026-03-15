using Microsoft.EntityFrameworkCore;
using PeopleDirectoryApplication.Application.Contracts.Repositories;
using PeopleDirectoryApplication.Data;
using PeopleDirectoryApplication.Models;

namespace PeopleDirectoryApplication.Infrastructure.Repositories;

public class AuditTrailRepository : IAuditTrailRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AuditTrailRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AuditTrailEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _dbContext.AuditTrailEntries.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditTrailEntry>> GetByEntityAsync(
        string entityName,
        string entityId,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuditTrailEntries
            .AsNoTracking()
            .Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderByDescending(a => a.ChangedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
