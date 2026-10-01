# RestaurantHub

Online restaurant ordering system (customer + admin portals) built with
ASP.NET Core MVC, EF Core Code First, SQL Server and ASP.NET Core Identity.

> 🚧 **Work in progress.** Implemented: authentication/roles and the admin back office.
> Planned: customer ordering flow, real-time notifications (SignalR),
> SSRS sales report, custom middleware.

## Implemented
- Authentication and role-based authorization (Admin / Customer) with Identity
- Admin CRUD for categories and menu items
- Secure image upload (extension whitelist, size limit, generated file names)
- Code First migrations, restrict-delete rules, unique category names

## Tech stack
C# · ASP.NET Core MVC · EF Core · SQL Server · Identity · Bootstrap

## Run locally
1. Install the .NET SDK and SQL Server (LocalDB works).
2. Clone the repo and check the connection string in `appsettings.json`.
3. Run `Update-Database` in Package Manager Console.
4. Run the app. Demo admin: `admin@restauranthub.com` / `Admin123`.
