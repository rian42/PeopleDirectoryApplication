using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using PeopleDirectoryApplication.Application.Exceptions;
using PeopleDirectoryApplication.Application.Models;
using PeopleDirectoryApplication.Models;

namespace PeopleDirectoryApplication.Clients;

public sealed class PeopleApiClient : IPeopleApiClient
{
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<PeopleApiClient> _logger;

    public PeopleApiClient(HttpClient httpClient, NavigationManager navigationManager, ILogger<PeopleApiClient> logger)
    {
        _httpClient = httpClient;
        _navigationManager = navigationManager;
        _logger = logger;
    }

    public Task<IReadOnlyList<Person>> SearchAsync(
        string searchTerm,
        string? country,
        string? city,
        CancellationToken cancellationToken = default)
    {
        var queryValues = new List<string>
        {
            $"searchTerm={Uri.EscapeDataString(searchTerm)}",
        };

        if (!string.IsNullOrWhiteSpace(country))
        {
            queryValues.Add($"country={Uri.EscapeDataString(country)}");
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            queryValues.Add($"city={Uri.EscapeDataString(city)}");
        }

        var path = $"/api/people/search?{string.Join("&", queryValues)}";
        return GetRequiredListAsync<Person>(path, cancellationToken);
    }

    public Task<IReadOnlyList<AutocompleteSuggestion>> AutocompleteAsync(
        string query,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (take <= 0)
        {
            take = 10;
        }

        var path = $"/api/people/autocomplete?query={Uri.EscapeDataString(query)}";
        return GetAutocompleteSuggestionsAsync(path, take, cancellationToken);
    }

    public async Task<Person?> GetByIdAsync(int personId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(ToAbsoluteUri($"/api/people/{personId}"), cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return await DeserializeBodyAsync<Person>(response, cancellationToken);
    }

    public Task<IReadOnlyList<string>> GetCountriesAsync(CancellationToken cancellationToken = default)
        => GetRequiredListAsync<string>("/api/people/countries", cancellationToken);

    public Task<IReadOnlyList<string>> GetCitiesByCountryAsync(string country, CancellationToken cancellationToken = default)
        => GetRequiredListAsync<string>(
            $"/api/people/countries/{Uri.EscapeDataString(country)}/cities",
            cancellationToken);

    public Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken cancellationToken = default)
        => GetRequiredListAsync<Person>("/api/people", cancellationToken);

    public async Task<Person> CreateAsync(PersonUpsertRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(ToAbsoluteUri("/api/people"), request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await DeserializeBodyAsync<Person>(response, cancellationToken);
    }

    public async Task<Person?> UpdateAsync(int personId, PersonUpsertRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync(ToAbsoluteUri($"/api/people/{personId}"), request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var message = await TryReadApiMessageAsync(response, cancellationToken);
            throw new ConcurrencyConflictException(message ?? "This record was modified by another user.");
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return await DeserializeBodyAsync<Person>(response, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int personId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync(ToAbsoluteUri($"/api/people/{personId}"), cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return true;
    }

    private async Task<IReadOnlyList<TItem>> GetRequiredListAsync<TItem>(string path, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(ToAbsoluteUri(path), cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await DeserializeBodyAsync<List<TItem>>(response, cancellationToken);
    }

    private async Task<IReadOnlyList<AutocompleteSuggestion>> GetAutocompleteSuggestionsAsync(string path, int take, CancellationToken cancellationToken)
    {
        var results = await GetRequiredListAsync<AutocompleteSuggestion>(path, cancellationToken);
        return results.Take(take).ToList();
    }

    private Uri ToAbsoluteUri(string relativePath)
        => _navigationManager.ToAbsoluteUri(relativePath);

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = await TryReadApiMessageAsync(response, cancellationToken);
        _logger.LogWarning(
            "People API request failed with status {StatusCode}. Message: {Message}",
            (int)response.StatusCode,
            message);

        throw new HttpRequestException(
            message ?? $"People API request failed with status code {(int)response.StatusCode}.",
            null,
            response.StatusCode);
    }

    private static async Task<T> DeserializeBodyAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
        if (result is null)
        {
            throw new HttpRequestException("People API returned an empty response body.");
        }

        return result;
    }

    private static async Task<string?> TryReadApiMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<ApiMessage>(cancellationToken: cancellationToken);
            if (!string.IsNullOrWhiteSpace(payload?.Message))
            {
                return payload.Message;
            }
        }
        catch
        {
            // ignored, fallback to raw response
        }

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(raw))
        {
            return raw;
        }

        return null;
    }

    private sealed class ApiMessage
    {
        public string? Message { get; set; }
    }
}
