namespace PeopleDirectoryApplication.Application.Models;

public sealed record PropertyChange(string PropertyName, string? OldValue, string? NewValue);
