using PeopleDirectoryApplication.Models;

namespace PeopleDirectoryApplication.Application.Contracts.Repositories;

public interface IPersonRepository
{
    Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Person?> GetByIdAsync(int personId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Person>> SearchAsync(string searchTerm, string? country, string? city, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Person>> AutocompleteAsync(string query, int maxResults, CancellationToken cancellationToken = default);
    Task<Person> AddAsync(Person person, CancellationToken cancellationToken = default);
    Task<Person?> UpdateAsync(Person person, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int personId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetCountriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetCitiesByCountryAsync(string country, CancellationToken cancellationToken = default);
}
