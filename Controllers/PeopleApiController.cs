using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleDirectoryApplication.Application.Contracts.Services;
using PeopleDirectoryApplication.Application.Exceptions;
using PeopleDirectoryApplication.Application.Models;
using PeopleDirectoryApplication.Security;

namespace PeopleDirectoryApplication.Controllers;

[ApiController]
[Route("api/people")]
[Produces("application/json")]
public class PeopleApiController : ControllerBase
{
    private readonly IPersonService _personService;
    private readonly ILogger<PeopleApiController> _logger;

    public PeopleApiController(IPersonService personService, ILogger<PeopleApiController> logger)
    {
        _personService = personService;
        _logger = logger;
    }

    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string searchTerm,
        [FromQuery] string? country,
        [FromQuery] string? city,
        CancellationToken cancellationToken)
    {
        var people = await _personService.SearchAsync(searchTerm, country, city, cancellationToken);
        return Ok(people);
    }

    [HttpGet("autocomplete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Autocomplete([FromQuery] string query, CancellationToken cancellationToken)
    {
        var people = await _personService.AutocompleteAsync(query, 10, cancellationToken);
        var response = people.Select(person => new
        {
            person.Id,
            DisplayName = $"{person.Name} {person.Surname}".Trim(),
        });

        return Ok(response);
    }

    [HttpGet("{personId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int personId, CancellationToken cancellationToken)
    {
        var person = await _personService.GetByIdAsync(personId, cancellationToken);
        if (person is null)
        {
            return NotFound();
        }

        return Ok(person);
    }

    [HttpGet("countries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCountries(CancellationToken cancellationToken)
    {
        var countries = await _personService.GetCountriesAsync(cancellationToken);
        return Ok(countries);
    }

    [HttpGet("countries/{country}/cities")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCitiesByCountry(string country, CancellationToken cancellationToken)
    {
        var cities = await _personService.GetCitiesByCountryAsync(country, cancellationToken);
        return Ok(cities);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var people = await _personService.GetAllAsync(cancellationToken);
        return Ok(people);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpGet("{personId:int}/audit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAudit(int personId, [FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        var entries = await _personService.GetAuditTrailAsync(personId, take, cancellationToken);
        return Ok(entries);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] PersonUpsertRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var created = await _personService.CreateAsync(request, cancellationToken);
        _logger.LogInformation("API created person {PersonId}", created.Id);
        return CreatedAtAction(nameof(GetById), new { personId = created.Id }, created);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPut("{personId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int personId, [FromBody] PersonUpsertRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var updated = await _personService.UpdateAsync(personId, request, cancellationToken);
            if (updated is null)
            {
                return NotFound();
            }

            _logger.LogInformation("API updated person {PersonId}", personId);
            return Ok(updated);
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict for person {PersonId}", personId);
            return Conflict(new { Message = ex.Message });
        }
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpDelete("{personId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int personId, CancellationToken cancellationToken)
    {
        var deleted = await _personService.DeleteAsync(personId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        _logger.LogInformation("API deleted person {PersonId}", personId);
        return NoContent();
    }
}
