# PeopleDirectoryApplication

## Repository Layout

- `src/PeopleDirectoryApplication.Web`: Blazor UI host, auth MVC endpoints/views, startup wiring.
- `src/PeopleDirectoryApplication.Api`: REST API controllers and API authorization policy constants.
- `src/PeopleDirectoryApplication.Domain`: Core entities and enums.
- `src/PeopleDirectoryApplication.Application`: Contracts and business services.
- `src/PeopleDirectoryApplication.Infrastructure`: EF Core, repositories, and email outbox processing.
- `tests/PeopleDirectoryApplication.Application.Tests`: Application-layer tests.

## Highlights

- Blazor-based frontend for:
  - public search + predictive matches via REST (`/api/people/autocomplete`)
  - profile view
  - admin CRUD screen
- `AdminOnly` role-based policy for admin APIs and admin UI route
- Structured logging across API/controller/service/repository/background-worker layers
- Audit trail persistence (`who`, `what`, `when`) for create/update/delete operations
- Optimistic concurrency on person updates via `RowVersion`
- Resilient email delivery:
  - queue-backed outbox table
  - retry with exponential backoff
  - dead-letter state after max retries
- OpenAPI/Swagger at `/swagger` for `/api/people`

## Run

1. Update `ConnectionStrings:DefaultConnection` in `src/PeopleDirectoryApplication.Web/appsettings.json`.
2. Configure admin seed values in `AdminUser` in `src/PeopleDirectoryApplication.Web/appsettings.json`.
3. (Optional) Configure `EmailNotifications` and set `Enabled=true`.
4. Apply DB migrations / update database:
   - `dotnet tool restore`
   - `dotnet tool run dotnet-ef database update --project .\src\PeopleDirectoryApplication.Web\PeopleDirectoryApplication.Web.csproj --startup-project .\src\PeopleDirectoryApplication.Web\PeopleDirectoryApplication.Web.csproj`
5. Seed data and admin user:
   - `dotnet run --project .\src\PeopleDirectoryApplication.Web\PeopleDirectoryApplication.Web.csproj seeddata`
6. Run app:
   - `dotnet run --project .\src\PeopleDirectoryApplication.Web\PeopleDirectoryApplication.Web.csproj`

## Notes

- REST endpoints are under `/api/people`.
- Admin management route is `/admin/people`.
- Login route is `/Account/Login`.
- Admin policy requires role: `admin`.
