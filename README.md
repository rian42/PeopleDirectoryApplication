# PeopleDirectoryApplication

- `PeopleDirectoryApplication.Web` (Web): Blazor UI, auth endpoints, REST API endpoints.
- `PeopleDirectoryApplication.Domain`: Core entities and enums.
- `PeopleDirectoryApplication.Application`: Contracts + business services.
- `PeopleDirectoryApplication.Infrastructure`: EF Core + repository + email outbox processing.

## Highlights

- Blazor-based frontend for:
  - Public search + predictive matches via REST (`/api/people/autocomplete`)
  - Profile view
  - Admin CRUD screen
- `AdminOnly` role-based policy for admin APIs and admin UI route
- Structured logging in API/controller/service/repository/background-worker layers
- Audit trail persistence (`who`, `what`, `when`) for create/update/delete operations
- Optimistic concurrency on person updates via `RowVersion`
- Resilient email delivery:
  - queue-backed outbox table
  - retry with exponential backoff
  - dead-letter state after max retries
- Country and city dropdown behavior for admin create/edit
- OpenAPI/Swagger at `/swagger` for `/api/people`

## Run

1. Update `ConnectionStrings:DefaultConnection` in `appsettings.json`.
2. Configure admin seed values in `AdminUser`.
3. (Optional) Configure `EmailNotifications` for SMTP and set `Enabled=true`.
4. Apply DB migrations / update database:
   - `dotnet tool restore`
   - `dotnet tool run dotnet-ef database update --project .\\PeopleDirectoryApplication.Web.csproj --startup-project .\\PeopleDirectoryApplication.Web.csproj`
5. Seed data and admin user:
   - `dotnet run --project .\\PeopleDirectoryApplication.Web.csproj seeddata`
6. Run app:
   - `dotnet run --project .\\PeopleDirectoryApplication.Web.csproj`

## Notes

- REST endpoints are under `/api/people`.
- Admin management route is `/admin/people`.
- Login route is `/Account/Login`.
- Admin policy requires role: `admin`.
