namespace PeopleDirectoryApplication.Application.Contracts.Services;

public interface ICurrentUserAccessor
{
    string GetCurrentUserIdentifier();
}
