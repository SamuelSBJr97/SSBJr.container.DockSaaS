# DockSaaS Setup Guide

## Running with .NET Aspire (Recommended)

.NET Aspire manages PostgreSQL and Redis containers automatically:

1. **Start the Aspire Host:**
   ```bash
   dotnet run --project SSBJr.container.DockSaaS.AppHost
   ```

2. **Access the Aspire Dashboard:**
   - Open your browser to `https://localhost:17090` (or the URL shown in the console)
   - Monitor all services, databases, and logs from the dashboard

3. **Services will be available at:**
   - API Service: `https://localhost:7000`
   - Blazor Web: `https://localhost:7001`
   - PostgreSQL: Managed by Aspire (connection string provided automatically)
   - Redis: Managed by Aspire
   - pgAdmin: Available through Aspire dashboard

## Fixed Issues

### ? JavaScript Interop Error Resolution
Fixed the JavaScript interoperability error during prerendering:

**Error**: `JavaScript interop calls cannot be issued at this time. This is because the component is being statically rendered.`

**Solution**: 
1. **Updated AuthService**: Added checks for JavaScript runtime availability before accessing localStorage
2. **Updated CustomAuthenticationStateProvider**: Added prerendering detection to avoid localStorage during server-side rendering
3. **Updated Login Page**: Moved authentication checks to `OnAfterRenderAsync` lifecycle method
4. **Enhanced LocalStorage Configuration**: Added proper JSON serialization options

### ? 404 Error Resolution
The 404 error when accessing the web application has been fixed by:

1. **Updated Routing**: Added proper NotFound handling in `Routes.razor`
2. **Authentication Flow**: Modified the home page to show welcome content for non-authenticated users
3. **Navigation**: Updated the layout to work for both authenticated and non-authenticated users
4. **Welcome Page**: Added `/welcome` route that doesn't require authentication

### ? Database Connection
Enhanced database connection handling with:
- Retry logic (3 attempts with 5-second delays)
- Better error messages with Aspire-specific guidance
- Fallback configuration for standalone development

## Available Pages

- **`/`** - Home/Dashboard (shows welcome for non-authenticated, dashboard for authenticated)
- **`/welcome`** - Welcome page (no authentication required)
- **`/login`** - Login/Register page
- **`/health`** - Application health status

## Technical Improvements

### Prerendering Support
The application now properly handles Blazor Server prerendering:
- **Authentication state** is properly managed during prerendering
- **LocalStorage access** is deferred until after client-side rendering
- **JavaScript interop** calls are safely handled during server-side rendering

### Authentication Flow
- ? Proper authentication state management
- ? Secure token storage in browser localStorage  
- ? Automatic token expiration handling
- ? Seamless login/logout experience

## Running Standalone (Without Aspire)

If you prefer to run services individually:

1. **Install PostgreSQL:**
   - Download from https://www.postgresql.org/download/
   - Create database: `docksaasdb`
   - User: `postgres`, Password: `postgres`

2. **Install Redis (optional):**
   - Download from https://redis.io/download
   - Run on default port 6379

3. **Run API Service:**
   ```bash
   dotnet run --project SSBJr.container.DockSaaS.ApiService
   ```

4. **Run Blazor Web:**
   ```bash
   dotnet run --project SSBJr.container.DockSaaS.Web
   ```

## Troubleshooting

### JavaScript Interop Error Fixed
? **Issue**: `JavaScript interop calls cannot be issued at this time`  
? **Solution**: Added prerendering detection and deferred localStorage access

**What was causing the error:**
1. AuthService trying to access localStorage during server-side prerendering
2. CustomAuthenticationStateProvider calling localStorage before JavaScript was available
3. Components checking authentication state during initial render

**How it's fixed:**
1. Added `IsJavaScriptRuntimeAvailable()` method to detect prerendering
2. Deferred localStorage operations until client-side rendering
3. Used `OnAfterRenderAsync` for authentication checks
4. Added proper exception handling for JavaScript interop errors

### 404 Error Fixed
? **Issue**: Web application showing 404 error  
? **Solution**: Updated routing and authentication flow

### Database Connection Issues

If you see `Failed to connect to 127.0.0.1:5432`:

1. **Using Aspire:** Make sure the AppHost is running first
2. **Standalone:** Verify PostgreSQL is installed and running
3. **Check logs:** The application will retry 3 times with 5-second delays
4. **Skip database:** Set `DatabaseInitialization:SkipOnConnectionFailure` to `true` in appsettings.json

### Common Commands

```bash
# Check PostgreSQL status (Windows)
sc query postgresql-x64-15

# Check PostgreSQL status (Linux/Mac)
sudo systemctl status postgresql

# Connect to database manually
psql -h localhost -U postgres -d docksaasdb

# Reset database
dotnet ef database drop --project SSBJr.container.DockSaaS.ApiService

# Build all projects
dotnet build

# Run tests
dotnet test

# Clean and rebuild
dotnet clean && dotnet build
```

## Quick Start (Recommended)

1. **Start Aspire:**
   ```bash
   dotnet run --project SSBJr.container.DockSaaS.AppHost
   ```

2. **Open Browser:**
   - Go to `https://localhost:7001` for the web application
   - Go to `https://localhost:17090` for the Aspire dashboard

3. **First Time Setup:**
   - Visit `/welcome` to learn about the application
   - Click "Sign Up" to create your first account
   - The database will be automatically created and seeded

## Environment Variables

The application supports these environment variables:

- `ASPNETCORE_ENVIRONMENT`: Set to `Development`, `Staging`, or `Production`
- `ConnectionStrings__docksaasdb`: Override database connection string
- `DatabaseInitialization__SkipOnConnectionFailure`: Skip DB init on connection failure
- `ApiBaseUrl`: Base URL for API service (used by Blazor web app)

## Development Notes

- **Authentication**: Uses JWT tokens stored in browser localStorage
- **Database**: PostgreSQL with Entity Framework Core migrations
- **UI Framework**: MudBlazor for modern Material Design components
- **Architecture**: Clean separation between API service and Blazor web app
- **Orchestration**: .NET Aspire for local development and testing
- **Prerendering**: Fully supported with proper JavaScript interop handling

## Status Summary

- ? **JavaScript Interop**: Fixed prerendering issues
- ? **404 Error**: Resolved routing problems
- ? **Database Connection**: Enhanced with retry logic
- ? **Authentication Flow**: Working seamlessly
- ? **Navigation**: Improved for all user states
- ? **Build**: Compiling successfully
- ? **Prerendering**: Properly supported