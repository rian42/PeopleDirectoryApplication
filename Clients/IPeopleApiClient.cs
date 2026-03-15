using PeopleDirectoryApplication.Application.Models;
using PeopleDirectoryApplication.Models;

namespace PeopleDirectoryApplication.Clients;

public interface IPeopleApiClient
{
    Task<IReadOnlyList<Person>> SearchAsync(
        string searchTerm,
        string? country,
        string? city,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AutocompleteSuggestion>> AutocompleteAsync(
        string query,
        int take,
        CancellationToken cancellationToken = default);

    Task<Person?> GetByIdAsync(int personId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetCountriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetCitiesByCountryAsync(string country, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Person> CreateAsync(PersonUpsertRequest request, CancellationToken cancellationToken = default);
    Task<Person?> UpdateAsync(int personId, PersonUpsertRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int personId, CancellationToken cancellationToken = default);
}
