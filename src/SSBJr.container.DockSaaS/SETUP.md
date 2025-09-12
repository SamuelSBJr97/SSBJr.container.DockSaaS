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
   - API Service: `https://localhost:7000` (HTTPS) / `http://localhost:5200` (HTTP)
   - Blazor Web: `https://localhost:7001` (HTTPS) / `http://localhost:5201` (HTTP)
   - PostgreSQL: Managed by Aspire (connection string provided automatically)
   - Redis: Managed by Aspire
   - pgAdmin: Available through Aspire dashboard

## ?? Default Admin User

A default administrator user is automatically created during database initialization:

**Default Admin Credentials:**
- **Email:** `admin@docksaas.com`
- **Password:** `Admin123!`
- **Tenant:** `DockSaaS`
- **Role:** `Admin`

?? **IMPORTANT:** Change this password immediately in production environments!

### How to Login
1. Go to `https://localhost:7001`
2. Click "Sign In"
3. Enter the default credentials above
4. You'll have full admin access to the system

## Fixed Issues

### ? Port Conflict Resolution
**Issue Fixed**: Port 5000/5001 conflicts with other services

**Solution**: 
- Changed API HTTP port from 5000 to 5200
- Changed Web HTTP port from 5001 to 5201
- Updated CORS configuration accordingly
- HTTPS ports remain unchanged (7000/7001)

### ? Default Admin User Creation
**New Feature**: Automatic creation of default administrator account

**What was added:**
1. **Default Tenant**: `DockSaaS` tenant created automatically
2. **Admin User**: `admin@docksaas.com` with Admin role
3. **Secure Password**: `Admin123!` (follows security requirements)
4. **Logging**: Clear logs about admin user creation
5. **Production Warning**: Reminder to change password

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

## Port Configuration

### Aspire Managed Ports
- **API Service**: 
  - HTTPS: `https://localhost:7000`
  - HTTP: `http://localhost:5200`
- **Blazor Web**: 
  - HTTPS: `https://localhost:7001`
  - HTTP: `http://localhost:5201`
- **Aspire Dashboard**: `https://localhost:17090`
- **PostgreSQL**: Managed by Aspire (dynamic port)
- **Redis**: Managed by Aspire (dynamic port)

### Port Conflict Solutions
If you encounter port conflicts:

1. **Check what's using the port:**
```bash
# Windows
netstat -ano | findstr :5200
netstat -ano | findstr :7000

# Linux/Mac
lsof -i :5200
lsof -i :7000
```

2. **Stop conflicting services:**
```bash
# Windows - kill process by PID
taskkill /PID <PID> /F

# Linux/Mac - kill process by PID
kill -9 <PID>
```

3. **Alternative: Change ports in AppHost.cs if needed**

## User Management

### Default Setup
1. **System Admin**: `admin@docksaas.com` (created automatically)
2. **First User Registration**: Creates new tenant with admin role
3. **Subsequent Users**: Added as regular users to existing tenant

### Creating Additional Users
- **Admin Access**: Use the default admin account
- **User Management**: Navigate to `/users` (Admin/Manager only)
- **Self Registration**: Users can register for new tenants

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
- ? Default admin user for immediate access

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

### Port Conflict Issues

**If you see "bind: address already in use" errors:**
1. Check if other applications are using ports 5200, 7000, 7001
2. Common conflicting services:
   - IIS Express
   - Other .NET applications
   - Docker containers
   - Visual Studio debug sessions

**Solutions:**
1. Stop conflicting services
2. Change ports in `AppHost.cs`
3. Use different port ranges entirely

### Default Admin User Issues

**If admin user is not created:**
1. Check database initialization logs
2. Verify database connectivity
3. Ensure roles are created first
4. Check for any constraint violations

**If you can't login with default credentials:**
1. Verify the user exists in database
2. Check if password policy is met
3. Ensure tenant `DockSaaS` exists
4. Try creating a new user instead

### JavaScript Interop Error Fixed
? **Issue**: `JavaScript interop calls cannot be issued at this time`  
? **Solution**: Added prerendering detection and deferred localStorage access

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
# Check port usage (Windows)
netstat -ano | findstr :5200
netstat -ano | findstr :7000
netstat -ano | findstr :7001

# Check port usage (Linux/Mac)
lsof -i :5200
lsof -i :7000
lsof -i :7001

# Check PostgreSQL status (Windows)
sc query postgresql-x64-15

# Check PostgreSQL status (Linux/Mac)
sudo systemctl status postgresql

# Connect to database manually
psql -h localhost -U postgres -d docksaasdb

# Reset database (will recreate admin user)
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

3. **Login with Default Admin:**
   - Email: `admin@docksaas.com`
   - Password: `Admin123!`
   - Full admin access immediately available

4. **Optional - Create Your Own Account:**
   - Click "Sign Up" to create your own tenant and account
   - First user in new tenant automatically becomes admin

## Security Notes

### Default Credentials
- **Development**: Default admin credentials are fine for local development
- **Production**: **MUST** change default password immediately
- **Best Practice**: Create your own admin account and disable/delete default one

### Password Requirements
- Minimum 8 characters
- At least 1 uppercase letter
- At least 1 lowercase letter  
- At least 1 digit
- Special characters allowed but not required

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
- **Default Admin**: Automatically created for immediate access
- **Port Management**: Configured to avoid common conflicts

## Status Summary

- ? **Port Configuration**: Fixed conflicts (5200/5201 for HTTP, 7000/7001 for HTTPS)
- ? **Default Admin User**: Created automatically (`admin@docksaas.com`)
- ? **JavaScript Interop**: Fixed prerendering issues
- ? **404 Error**: Resolved routing problems
- ? **Database Connection**: Enhanced with retry logic
- ? **Authentication Flow**: Working seamlessly
- ? **Navigation**: Improved for all user states
- ? **Build**: Compiling successfully
- ? **Prerendering**: Properly supported
- ? **User Management**: Multi-tenant with role-based access