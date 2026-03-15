using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PeopleDirectoryApplication.Application.Contracts.Repositories;
using PeopleDirectoryApplication.Application.Exceptions;
using PeopleDirectoryApplication.Data;
using PeopleDirectoryApplication.Models;

namespace PeopleDirectoryApplication.Infrastructure.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PersonRepository> _logger;

    public PersonRepository(ApplicationDbContext dbContext, ILogger<PersonRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Persons
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Surname)
            .ToListAsync(cancellationToken);

    public async Task<Person?> GetByIdAsync(int personId, CancellationToken cancellationToken = default)
    {
        if (personId <= 0)
        {
            return null;
        }

        return await _dbContext.Persons
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == personId, cancellationToken);
    }

    public async Task<IReadOnlyList<Person>> SearchAsync(
        string searchTerm,
        string? country,
        string? city,
        CancellationToken cancellationToken = default)
    {
        var normalizedSearch = searchTerm.Trim().ToLowerInvariant();

        IQueryable<Person> query = _dbContext.Persons.AsNoTracking();
        query = query.Where(p =>
            p.Name.ToLower().Contains(normalizedSearch) ||
            p.Surname.ToLower().Contains(normalizedSearch));

        if (!string.IsNullOrWhiteSpace(country))
        {
            var normalizedCountry = country.Trim().ToLowerInvariant();
            query = query.Where(p => p.Country.ToLower() == normalizedCountry);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            var normalizedCity = city.Trim().ToLowerInvariant();
            query = query.Where(p => p.City.ToLower() == normalizedCity);
        }

        return await query
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Surname)
            .Take(100)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Person>> AutocompleteAsync(string query, int maxResults, CancellationToken cancellationToken = default)
    {
        var normalizedQuery = query.Trim().ToLowerInvariant();

        return await _dbContext.Persons
            .AsNoTracking()
            .Where(p => p.Name.ToLower().Contains(normalizedQuery) || p.Surname.ToLower().Contains(normalizedQuery))
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Surname)
            .Take(maxResults)
            .ToListAsync(cancellationToken);
    }

    public async Task<Person> AddAsync(Person person, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(person);

        _dbContext.Persons.Add(person);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return person;
    }

    public async Task<Person?> UpdateAsync(Person person, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(person);

        if (person.RowVersion is null || person.RowVersion.Length == 0)
        {
            throw new ConcurrencyConflictException("Missing row version value for update.");
        }

        var existingPerson = await _dbContext.Persons.FirstOrDefaultAsync(p => p.Id == person.Id, cancellationToken);
        if (existingPerson is null)
        {
            return null;
        }

        if (!person.RowVersion.SequenceEqual(existingPerson.RowVersion))
        {
            throw new ConcurrencyConflictException("The record has already been modified by another user.");
        }

        existingPerson.Name = person.Name;
        existingPerson.Surname = person.Surname;
        existingPerson.Country = person.Country;
        existingPerson.City = person.City;
        existingPerson.EmailAddress = person.EmailAddress;
        existingPerson.MobileNumber = person.MobileNumber;
        existingPerson.ProfilePicture = person.ProfilePicture;
        existingPerson.Gender = person.Gender;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict while updating person {PersonId}", person.Id);
            throw new ConcurrencyConflictException("The record has already been modified by another user.");
        }

        _logger.LogInformation("Updated person record {PersonId}", existingPerson.Id);

        return await _dbContext.Persons.AsNoTracking().FirstOrDefaultAsync(p => p.Id == existingPerson.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int personId, CancellationToken cancellationToken = default)
    {
        var person = await _dbContext.Persons.FirstOrDefaultAsync(p => p.Id == personId, cancellationToken);
        if (person is null)
        {
            return false;
        }

        _dbContext.Persons.Remove(person);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<string>> GetCountriesAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Persons
            .AsNoTracking()
            .Where(p => !string.IsNullOrWhiteSpace(p.Country))
            .Select(p => p.Country)
            .Distinct()
            .OrderBy(country => country)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<string>> GetCitiesByCountryAsync(string country, CancellationToken cancellationToken = default)
    {
        var normalizedCountry = country.Trim().ToLowerInvariant();
        return await _dbContext.Persons
            .AsNoTracking()
            .Where(p => p.Country.ToLower() == normalizedCountry && !string.IsNullOrWhiteSpace(p.City))
            .Select(p => p.City)
            .Distinct()
            .OrderBy(city => city)
            .ToListAsync(cancellationToken);
    }
}
