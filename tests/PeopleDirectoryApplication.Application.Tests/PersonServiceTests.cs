using Microsoft.Extensions.Logging;
using Moq;
using PeopleDirectoryApplication.Application.Contracts.Repositories;
using PeopleDirectoryApplication.Application.Contracts.Services;
using PeopleDirectoryApplication.Application.Exceptions;
using PeopleDirectoryApplication.Application.Models;
using PeopleDirectoryApplication.Application.Services;
using PeopleDirectoryApplication.Data.Enum;
using PeopleDirectoryApplication.Models;

namespace PeopleDirectoryApplication.Application.Tests;

public class PersonServiceTests
{
    private readonly Mock<IPersonRepository> _personRepositoryMock = new();
    private readonly Mock<IAuditTrailRepository> _auditTrailRepositoryMock = new();
    private readonly Mock<IEmailNotificationService> _emailNotificationServiceMock = new();
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessorMock = new();
    private readonly Mock<ILogger<PersonService>> _loggerMock = new();

    private PersonService CreateService()
    {
        _currentUserAccessorMock.Setup(current => current.GetCurrentUserIdentifier()).Returns("admin@local");
        return new PersonService(
            _personRepositoryMock.Object,
            _auditTrailRepositoryMock.Object,
            _emailNotificationServiceMock.Object,
            _currentUserAccessorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdIsInvalid_ReturnsNull()
    {
        var service = CreateService();

        var result = await service.GetByIdAsync(0);

        Assert.Null(result);
        _personRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_WhenSearchTermIsBlank_ReturnsEmptyList()
    {
        var service = CreateService();

        var result = await service.SearchAsync("   ", null, null);

        Assert.Empty(result);
        _personRepositoryMock.Verify(repo => repo.SearchAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AutocompleteAsync_WhenMaxResultsIsInvalid_UsesDefaultLimitAndTrimmedQuery()
    {
        var expected = new List<Person> { new() { Id = 1, Name = "Mark", Surname = "Blue" } };
        _personRepositoryMock
            .Setup(repo => repo.AutocompleteAsync("Mark", 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var service = CreateService();
        var result = await service.AutocompleteAsync(" Mark ", 0);

        Assert.Single(result);
        _personRepositoryMock.Verify(repo => repo.AutocompleteAsync("Mark", 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_MapsAndTrimsFields_PersistsAndQueuesEmail_AndWritesAudit()
    {
        _personRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person person, CancellationToken _) =>
            {
                person.Id = 99;
                return person;
            });

        var service = CreateService();
        var request = BuildRequest();

        var created = await service.CreateAsync(request);

        Assert.Equal(99, created.Id);
        Assert.Equal("Mark", created.Name);
        Assert.Equal("Jones", created.Surname);
        Assert.Equal("South Africa", created.Country);
        Assert.Equal("Cape Town", created.City);
        Assert.Equal("mark@example.com", created.EmailAddress);
        Assert.Equal("0811111111", created.MobileNumber);
        Assert.Equal("https://example.com/pic.png", created.ProfilePicture);
        Assert.Equal(Gender.Male, created.Gender);

        _personRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Person>(p =>
            p.Name == "Mark" &&
            p.Surname == "Jones" &&
            p.Country == "South Africa" &&
            p.City == "Cape Town" &&
            p.EmailAddress == "mark@example.com" &&
            p.MobileNumber == "0811111111" &&
            p.ProfilePicture == "https://example.com/pic.png"), It.IsAny<CancellationToken>()), Times.Once);

        _emailNotificationServiceMock.Verify(email => email.SendPersonCreatedNotificationAsync(It.Is<Person>(p => p.Id == 99), It.IsAny<CancellationToken>()), Times.Once);
        _auditTrailRepositoryMock.Verify(audit => audit.AddAsync(It.Is<AuditTrailEntry>(entry =>
            entry.EntityName == nameof(Person) &&
            entry.EntityId == "99" &&
            entry.Action == "Created" &&
            entry.ChangedBy == "admin@local"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenEmailQueueFails_StillReturnsCreatedPerson()
    {
        var created = new Person { Id = 7, Name = "Mark", Surname = "Jones", Country = "South Africa", City = "Cape Town", Gender = Gender.Male };
        _personRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);
        _emailNotificationServiceMock
            .Setup(email => email.SendPersonCreatedNotificationAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Queue unavailable"));

        var service = CreateService();
        var result = await service.CreateAsync(BuildRequest());

        Assert.Equal(7, result.Id);
        _emailNotificationServiceMock.Verify(email => email.SendPersonCreatedNotificationAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenIdIsInvalid_ReturnsNull()
    {
        var service = CreateService();

        var result = await service.UpdateAsync(0, BuildRequest(Array.Empty<byte>()));

        Assert.Null(result);
        _personRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _personRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenRowVersionMissing_ThrowsConcurrencyConflict()
    {
        _personRepositoryMock
            .Setup(repo => repo.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Person
            {
                Id = 10,
                Name = "Mark",
                Surname = "Jones",
                Country = "South Africa",
                City = "Cape Town",
                Gender = Gender.Male,
                RowVersion = new byte[] { 1, 2, 3 },
            });

        var service = CreateService();
        var request = BuildRequest();
        request.RowVersion = null;

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => service.UpdateAsync(10, request));
    }

    [Fact]
    public async Task UpdateAsync_WhenRecordDoesNotExist_ReturnsNull()
    {
        _personRepositoryMock
            .Setup(repo => repo.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        var service = CreateService();
        var result = await service.UpdateAsync(10, BuildRequest(Array.Empty<byte>()));

        Assert.Null(result);
        _personRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailNotificationServiceMock.Verify(email => email.SendPersonUpdatedNotificationAsync(It.IsAny<Person>(), It.IsAny<Person>(), It.IsAny<IReadOnlyCollection<PropertyChange>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenNoChangesDetected_DoesNotPersistOrQueueEmail()
    {
        var rowVersion = new byte[] { 1, 2, 3 };
        var existing = new Person
        {
            Id = 5,
            Name = "Mark",
            Surname = "Jones",
            Country = "South Africa",
            City = "Cape Town",
            EmailAddress = "mark@example.com",
            MobileNumber = "0811111111",
            ProfilePicture = "https://example.com/pic.png",
            Gender = Gender.Male,
            RowVersion = rowVersion,
        };

        _personRepositoryMock
            .Setup(repo => repo.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var service = CreateService();
        var result = await service.UpdateAsync(5, BuildRequest(rowVersion));

        Assert.NotNull(result);
        Assert.Equal(existing.Id, result!.Id);
        _personRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailNotificationServiceMock.Verify(email => email.SendPersonUpdatedNotificationAsync(It.IsAny<Person>(), It.IsAny<Person>(), It.IsAny<IReadOnlyCollection<PropertyChange>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenChangesExist_PersistsAndQueuesChangeEmail_AndWritesAudit()
    {
        var rowVersion = new byte[] { 8, 8, 8 };
        var existing = new Person
        {
            Id = 5,
            Name = "Mark",
            Surname = "Jones",
            Country = "South Africa",
            City = "Cape Town",
            EmailAddress = "mark@example.com",
            MobileNumber = "0811111111",
            ProfilePicture = "https://example.com/pic.png",
            Gender = Gender.Male,
            RowVersion = rowVersion,
        };

        var updated = new Person
        {
            Id = 5,
            Name = "Marcus",
            Surname = "Jones",
            Country = "South Africa",
            City = "Johannesburg",
            EmailAddress = "mark@example.com",
            MobileNumber = "0811111111",
            ProfilePicture = "https://example.com/pic.png",
            Gender = Gender.Male,
            RowVersion = new byte[] { 9, 9, 9 },
        };

        _personRepositoryMock
            .Setup(repo => repo.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _personRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var request = BuildRequest(rowVersion);
        request.Name = " Marcus ";
        request.City = " Johannesburg ";

        var service = CreateService();
        var result = await service.UpdateAsync(5, request);

        Assert.NotNull(result);
        Assert.Equal("Marcus", result!.Name);
        Assert.Equal("Johannesburg", result.City);

        _personRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Person>(p =>
            p.Id == 5 &&
            p.Name == "Marcus" &&
            p.City == "Johannesburg" &&
            p.RowVersion.SequenceEqual(rowVersion)), It.IsAny<CancellationToken>()), Times.Once);

        _emailNotificationServiceMock.Verify(email => email.SendPersonUpdatedNotificationAsync(
            existing,
            updated,
            It.Is<IReadOnlyCollection<PropertyChange>>(changes =>
                changes.Any(c => c.PropertyName == nameof(Person.Name) && c.OldValue == "Mark" && c.NewValue == "Marcus") &&
                changes.Any(c => c.PropertyName == nameof(Person.City) && c.OldValue == "Cape Town" && c.NewValue == "Johannesburg")),
            It.IsAny<CancellationToken>()), Times.Once);

        _auditTrailRepositoryMock.Verify(audit => audit.AddAsync(It.Is<AuditTrailEntry>(entry =>
            entry.EntityId == "5" &&
            entry.Action == "Updated" &&
            entry.ChangedBy == "admin@local"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenRepositoryUpdateFails_ReturnsNullAndSkipsQueuedEmail()
    {
        var rowVersion = new byte[] { 1, 1, 1 };
        var existing = new Person
        {
            Id = 5,
            Name = "Mark",
            Surname = "Jones",
            Country = "South Africa",
            City = "Cape Town",
            Gender = Gender.Male,
            RowVersion = rowVersion,
        };

        _personRepositoryMock
            .Setup(repo => repo.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _personRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        var request = BuildRequest(rowVersion);
        request.City = "Johannesburg";

        var service = CreateService();
        var result = await service.UpdateAsync(5, request);

        Assert.Null(result);
        _emailNotificationServiceMock.Verify(email => email.SendPersonUpdatedNotificationAsync(It.IsAny<Person>(), It.IsAny<Person>(), It.IsAny<IReadOnlyCollection<PropertyChange>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenIdIsInvalid_ReturnsFalse()
    {
        var service = CreateService();

        var deleted = await service.DeleteAsync(-1);

        Assert.False(deleted);
        _personRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenDeleted_WritesAudit()
    {
        var existing = new Person
        {
            Id = 12,
            Name = "Mia",
            Surname = "May",
            Country = "South Africa",
            City = "Durban",
            Gender = Gender.Female,
        };

        _personRepositoryMock.Setup(repo => repo.GetByIdAsync(12, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _personRepositoryMock.Setup(repo => repo.DeleteAsync(12, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var service = CreateService();
        var deleted = await service.DeleteAsync(12);

        Assert.True(deleted);
        _auditTrailRepositoryMock.Verify(audit => audit.AddAsync(It.Is<AuditTrailEntry>(entry =>
            entry.EntityId == "12" &&
            entry.Action == "Deleted"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCitiesByCountryAsync_WhenCountryIsBlank_ReturnsEmpty()
    {
        var service = CreateService();
        var result = await service.GetCitiesByCountryAsync(" ");

        Assert.Empty(result);
        _personRepositoryMock.Verify(repo => repo.GetCitiesByCountryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static PersonUpsertRequest BuildRequest(byte[]? rowVersion = null)
    {
        return new PersonUpsertRequest
        {
            Name = " Mark ",
            Surname = " Jones ",
            Country = " South Africa ",
            City = " Cape Town ",
            EmailAddress = " mark@example.com ",
            MobileNumber = " 0811111111 ",
            ProfilePicture = " https://example.com/pic.png ",
            Gender = Gender.Male,
            RowVersion = rowVersion ?? new byte[] { 1, 2, 3 },
        };
    }
}
