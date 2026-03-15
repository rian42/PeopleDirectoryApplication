using System.Text.Json;
using Microsoft.Extensions.Logging;
using PeopleDirectoryApplication.Application.Contracts.Repositories;
using PeopleDirectoryApplication.Application.Contracts.Services;
using PeopleDirectoryApplication.Application.Exceptions;
using PeopleDirectoryApplication.Application.Models;
using PeopleDirectoryApplication.Models;

namespace PeopleDirectoryApplication.Application.Services;

public class PersonService : IPersonService
{
    private readonly IPersonRepository _personRepository;
    private readonly IAuditTrailRepository _auditTrailRepository;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ILogger<PersonService> _logger;

    public PersonService(
        IPersonRepository personRepository,
        IAuditTrailRepository auditTrailRepository,
        IEmailNotificationService emailNotificationService,
        ICurrentUserAccessor currentUserAccessor,
        ILogger<PersonService> logger)
    {
        _personRepository = personRepository;
        _auditTrailRepository = auditTrailRepository;
        _emailNotificationService = emailNotificationService;
        _currentUserAccessor = currentUserAccessor;
        _logger = logger;
    }

    public Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken cancellationToken = default)
        => _personRepository.GetAllAsync(cancellationToken);

    public Task<Person?> GetByIdAsync(int personId, CancellationToken cancellationToken = default)
    {
        if (personId <= 0)
        {
            _logger.LogWarning("Invalid person id requested: {PersonId}", personId);
            return Task.FromResult<Person?>(null);
        }

        return _personRepository.GetByIdAsync(personId, cancellationToken);
    }

    public async Task<IReadOnlyList<AuditTrailEntry>> GetAuditTrailAsync(
        int personId,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        if (personId <= 0)
        {
            return Array.Empty<AuditTrailEntry>();
        }

        if (take <= 0)
        {
            take = 50;
        }

        return await _auditTrailRepository.GetByEntityAsync(nameof(Person), personId.ToString(), take, cancellationToken);
    }

    public Task<IReadOnlyList<Person>> SearchAsync(
        string searchTerm,
        string? country,
        string? city,
        CancellationToken cancellationToken = default)
    {
        var normalizedSearchTerm = searchTerm?.Trim() ?? string.Empty;
        var normalizedCountry = string.IsNullOrWhiteSpace(country) ? null : country.Trim();
        var normalizedCity = string.IsNullOrWhiteSpace(city) ? null : city.Trim();

        if (string.IsNullOrWhiteSpace(normalizedSearchTerm))
        {
            return Task.FromResult<IReadOnlyList<Person>>(Array.Empty<Person>());
        }

        return _personRepository.SearchAsync(normalizedSearchTerm, normalizedCountry, normalizedCity, cancellationToken);
    }

    public Task<IReadOnlyList<Person>> AutocompleteAsync(
        string query,
        int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult<IReadOnlyList<Person>>(Array.Empty<Person>());
        }

        if (maxResults <= 0)
        {
            maxResults = 10;
        }

        return _personRepository.AutocompleteAsync(query.Trim(), maxResults, cancellationToken);
    }

    public async Task<Person> CreateAsync(PersonUpsertRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var personToCreate = BuildPersonFromRequest(request);
        var createdPerson = await _personRepository.AddAsync(personToCreate, cancellationToken);

        _logger.LogInformation("Person created with id {PersonId} by service layer", createdPerson.Id);

        var createdChanges = BuildCreateChanges(createdPerson);
        await TryWriteAuditAsync("Created", createdPerson, createdChanges, cancellationToken);

        try
        {
            await _emailNotificationService.SendPersonCreatedNotificationAsync(createdPerson, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue creation email notification for person id {PersonId}", createdPerson.Id);
        }

        return createdPerson;
    }

    public async Task<Person?> UpdateAsync(int personId, PersonUpsertRequest request, CancellationToken cancellationToken = default)
    {
        if (personId <= 0)
        {
            _logger.LogWarning("Invalid person id for update: {PersonId}", personId);
            return null;
        }

        ArgumentNullException.ThrowIfNull(request);

        var existingPerson = await _personRepository.GetByIdAsync(personId, cancellationToken);
        if (existingPerson is null)
        {
            _logger.LogWarning("Person not found for update: {PersonId}", personId);
            return null;
        }

        if (request.RowVersion is null || request.RowVersion.Length == 0)
        {
            throw new ConcurrencyConflictException("The person record version is missing.");
        }

        var candidatePerson = BuildPersonFromRequest(request);
        candidatePerson.Id = personId;
        var changes = BuildChanges(existingPerson, candidatePerson);

        if (changes.Count == 0)
        {
            _logger.LogInformation("No changes detected for person id {PersonId}", personId);
            return existingPerson;
        }

        var updatedPerson = await _personRepository.UpdateAsync(candidatePerson, cancellationToken);
        if (updatedPerson is null)
        {
            _logger.LogWarning("Person update failed for id {PersonId}", personId);
            return null;
        }

        _logger.LogInformation("Person updated with id {PersonId}. Change count: {ChangeCount}", personId, changes.Count);
        await TryWriteAuditAsync("Updated", updatedPerson, changes, cancellationToken);

        try
        {
            await _emailNotificationService.SendPersonUpdatedNotificationAsync(existingPerson, updatedPerson, changes, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue update email notification for person id {PersonId}", personId);
        }

        return updatedPerson;
    }

    public async Task<bool> DeleteAsync(int personId, CancellationToken cancellationToken = default)
    {
        if (personId <= 0)
        {
            _logger.LogWarning("Invalid person id for delete: {PersonId}", personId);
            return false;
        }

        var existingPerson = await _personRepository.GetByIdAsync(personId, cancellationToken);
        if (existingPerson is null)
        {
            return false;
        }

        var deleted = await _personRepository.DeleteAsync(personId, cancellationToken);
        if (!deleted)
        {
            return false;
        }

        await TryWriteAuditAsync("Deleted", existingPerson, BuildDeleteChanges(existingPerson), cancellationToken);
        return true;
    }

    public Task<IReadOnlyList<string>> GetCountriesAsync(CancellationToken cancellationToken = default)
        => _personRepository.GetCountriesAsync(cancellationToken);

    public Task<IReadOnlyList<string>> GetCitiesByCountryAsync(string country, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        return _personRepository.GetCitiesByCountryAsync(country.Trim(), cancellationToken);
    }

    private async Task TryWriteAuditAsync(
        string action,
        Person person,
        IReadOnlyCollection<PropertyChange> changes,
        CancellationToken cancellationToken)
    {
        try
        {
            var auditEntry = new AuditTrailEntry
            {
                EntityName = nameof(Person),
                EntityId = person.Id.ToString(),
                Action = action,
                ChangedBy = _currentUserAccessor.GetCurrentUserIdentifier(),
                ChangedAtUtc = DateTime.UtcNow,
                ChangesJson = JsonSerializer.Serialize(changes),
            };

            await _auditTrailRepository.AddAsync(auditEntry, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit trail for person id {PersonId} action {AuditAction}", person.Id, action);
        }
    }

    private static Person BuildPersonFromRequest(PersonUpsertRequest request)
    {
        return new Person
        {
            Name = request.Name.Trim(),
            Surname = request.Surname.Trim(),
            Country = request.Country.Trim(),
            City = request.City.Trim(),
            EmailAddress = string.IsNullOrWhiteSpace(request.EmailAddress) ? null : request.EmailAddress.Trim(),
            MobileNumber = string.IsNullOrWhiteSpace(request.MobileNumber) ? null : request.MobileNumber.Trim(),
            ProfilePicture = string.IsNullOrWhiteSpace(request.ProfilePicture) ? null : request.ProfilePicture.Trim(),
            Gender = request.Gender,
            RowVersion = request.RowVersion ?? Array.Empty<byte>(),
        };
    }

    private static List<PropertyChange> BuildCreateChanges(Person person)
    {
        return
        [
            new PropertyChange(nameof(Person.Name), null, person.Name),
            new PropertyChange(nameof(Person.Surname), null, person.Surname),
            new PropertyChange(nameof(Person.Country), null, person.Country),
            new PropertyChange(nameof(Person.City), null, person.City),
            new PropertyChange(nameof(Person.EmailAddress), null, person.EmailAddress),
            new PropertyChange(nameof(Person.MobileNumber), null, person.MobileNumber),
            new PropertyChange(nameof(Person.ProfilePicture), null, person.ProfilePicture),
            new PropertyChange(nameof(Person.Gender), null, person.Gender.ToString()),
        ];
    }

    private static List<PropertyChange> BuildDeleteChanges(Person person)
    {
        return
        [
            new PropertyChange(nameof(Person.Name), person.Name, null),
            new PropertyChange(nameof(Person.Surname), person.Surname, null),
            new PropertyChange(nameof(Person.Country), person.Country, null),
            new PropertyChange(nameof(Person.City), person.City, null),
            new PropertyChange(nameof(Person.EmailAddress), person.EmailAddress, null),
            new PropertyChange(nameof(Person.MobileNumber), person.MobileNumber, null),
            new PropertyChange(nameof(Person.ProfilePicture), person.ProfilePicture, null),
            new PropertyChange(nameof(Person.Gender), person.Gender.ToString(), null),
        ];
    }

    private static List<PropertyChange> BuildChanges(Person previous, Person current)
    {
        var changes = new List<PropertyChange>();

        AddIfDifferent(changes, nameof(Person.Name), previous.Name, current.Name);
        AddIfDifferent(changes, nameof(Person.Surname), previous.Surname, current.Surname);
        AddIfDifferent(changes, nameof(Person.Country), previous.Country, current.Country);
        AddIfDifferent(changes, nameof(Person.City), previous.City, current.City);
        AddIfDifferent(changes, nameof(Person.EmailAddress), previous.EmailAddress, current.EmailAddress);
        AddIfDifferent(changes, nameof(Person.MobileNumber), previous.MobileNumber, current.MobileNumber);
        AddIfDifferent(changes, nameof(Person.ProfilePicture), previous.ProfilePicture, current.ProfilePicture);
        AddIfDifferent(changes, nameof(Person.Gender), previous.Gender.ToString(), current.Gender.ToString());

        return changes;
    }

    private static void AddIfDifferent(List<PropertyChange> changes, string propertyName, string? oldValue, string? newValue)
    {
        if (!string.Equals(oldValue, newValue, StringComparison.Ordinal))
        {
            changes.Add(new PropertyChange(propertyName, oldValue, newValue));
        }
    }
}
