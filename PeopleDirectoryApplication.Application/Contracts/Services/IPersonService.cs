using PeopleDirectoryApplication.Application.Models;
using PeopleDirectoryApplication.Models;

namespace PeopleDirectoryApplication.Application.Contracts.Services;

public interface IPersonService
{
    Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Person?> GetByIdAsync(int personId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditTrailEntry>> GetAuditTrailAsync(int personId, int take = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Person>> SearchAsync(string searchTerm, string? country, string? city, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Person>> AutocompleteAsync(string query, int maxResults = 10, CancellationToken cancellationToken = default);
    Task<Person> CreateAsync(PersonUpsertRequest request, CancellationToken cancellationToken = default);
    Task<Person?> UpdateAsync(int personId, PersonUpsertRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int personId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetCountriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetCitiesByCountryAsync(string country, CancellationToken cancellationToken = default);
}
