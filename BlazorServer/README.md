# Blazor Server Kiosk Management System

## Overview

A modern **Blazor Server** application for managing customers, products, and employees with full CRUD operations. The application supports both **Entity Framework Core** and **Dapper** as data providers with seamless switching.

## Features

### Data Provider Selection
- **EF Core** or **Dapper** - Choose your preferred ORM
- Configured via a single constant in `Program.cs`
- Automatic database initialization for both providers
- Same database schema, fully compatible

### CRUD Operations
- **Customers** - Manage customer information
- **Products** - Track inventory and product details
- **Employees** - Manage employee records

### Modern UI/UX
- Clean, professional design
- Responsive layout (mobile-friendly)
- Interactive modals for create/edit/delete
- Loading states and error handling
- Empty state designs
- Bootstrap Icons integration

## Quick Start

### 1. Choose Your Data Provider

Open `BlazorServer\Program.cs` and set the data provider:

```csharp
// Line 16
const DataProvider DATA_PROVIDER = DataProvider.EFCore; // or DataProvider.Dapper
```

### 2. Configure Connection String

The application uses the connection string from `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=KioskDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 3. Run the Application

```bash
cd BlazorServer
dotnet run
```

The application will:
- Automatically create the database if it doesn't exist
- Apply EF Core migrations (if using EF Core)
- Run Dapper initialization script (if using Dapper)
- Start the web server

### 4. Navigate to the Application

Open your browser and go to: `https://localhost:5001` (or the URL shown in console)

## Project Structure

```
BlazorServer/
??? Components/
?   ??? Layout/
?   ?   ??? MainLayout.razor         # Main layout template
?   ?   ??? NavMenu.razor            # Navigation menu
?   ??? Pages/
?   ?   ??? Home.razor               # Dashboard homepage
?   ?   ??? Customers.razor          # Customer CRUD
?   ?   ??? Products.razor           # Product CRUD
?   ?   ??? Employees.razor          # Employee CRUD
?   ?   ??? Shared.css               # Shared styles
?   ??? App.razor                    # App shell
?   ??? Routes.razor                 # Routing configuration
??? Services/
?   ??? CustomerService.cs           # Customer business logic
?   ??? ProductService.cs            # Product business logic
?   ??? EmployeeService.cs           # Employee business logic
??? DataProvider.cs                  # Enum for provider selection
??? Program.cs                       # Application configuration & DI
??? appsettings.json                 # Configuration settings
```

## Data Provider Configuration

### Using Entity Framework Core

```csharp
const DataProvider DATA_PROVIDER = DataProvider.EFCore;
```

**Benefits:**
- Change tracking
- LINQ support
- Migration management
- Navigation properties

### Using Dapper

```csharp
const DataProvider DATA_PROVIDER = DataProvider.Dapper;
```

**Benefits:**
- High performance
- Direct SQL control
- Minimal overhead
- Lightweight

## Database Initialization

### Automatic (Recommended)

The application automatically initializes the database on startup:

**EF Core:**
- Checks if database exists
- Applies pending migrations
- Creates all tables

**Dapper:**
- Checks if database exists
- Runs `Infrastructure.Dapper\Scripts\InitializeDatabase.sql`
- Creates all tables and seed data

### Manual

If automatic initialization fails, run manually:

**EF Core:**
```bash
cd Infrastructure.EFCore
dotnet ef database update
```

**Dapper:**
```bash
sqlcmd -S "(localdb)\mssqllocaldb" -i "Infrastructure.Dapper\Scripts\InitializeDatabase.sql"
```

## Pages and Features

### Home Page (`/`)

Dashboard with:
- Data provider badge
- Navigation cards to Customers, Products, Employees
- Modern gradient hero section

### Customers Page (`/customers`)

**Features:**
- List all customers with avatar initials
- Add new customer (modal)
- Edit customer (modal)
- Delete customer (confirmation modal)
- Empty state for no customers
- Loading spinner
- Error handling

**Fields:**
- First Name (required)
- Last Name (required)
- Email (required)
- Phone (optional)

### Products Page (`/products`)

**Features:**
- List all products with descriptions
- Stock quantity badges (color-coded)
- Add new product (modal)
- Edit product (modal)
- Delete product (confirmation modal)
- Price display formatting
- SKU tracking

**Fields:**
- Name (required)
- Description (optional)
- Price (required)
- Stock Quantity (required)
- SKU (optional)

### Employees Page (`/employees`)

**Features:**
- List all employees with avatar initials
- Active/Inactive status badges
- Add new employee (modal)
- Edit employee (modal)
- Delete employee (confirmation modal)
- Hire date tracking
- Hourly rate display

**Fields:**
- First Name (required)
- Last Name (required)
- Email (required)
- Phone (optional)
- Hire Date (required)
- Hourly Rate (required)
- Active Status (checkbox)

## UI Components

### Modals
- Create/Edit forms with validation
- Delete confirmation dialogs
- Backdrop overlay
- Smooth animations

### Tables
- Gradient header
- Hover effects
- Action buttons (Edit/Delete)
- Avatar initials for names
- Responsive design

### Empty States
- Friendly messaging
- Icons
- Call-to-action buttons

### Loading States
- Spinner during data fetch
- Prevents layout shift

## Styling

### Design System
- **Primary Color:** Purple gradient (#667eea to #764ba2)
- **Success Color:** Green (#48bb78)
- **Danger Color:** Red (#f56565)
- **Text Colors:** Gray scale (#2d3748 to #718096)

### Typography
- **Headings:** Bold, clean sans-serif
- **Body:** Readable, comfortable spacing

### Components
- **Border Radius:** 8-16px for modern look
- **Shadows:** Subtle elevation
- **Transitions:** Smooth 0.2-0.3s ease

## Error Handling

The application handles errors gracefully:

### Data Loading Errors
```razor
@if (errorMessage != null)
{
    <div class="alert alert-danger">
        Error: @errorMessage
    </div>
}
```

### Save/Delete Errors
- Displays error message
- Keeps modal open for user to retry
- Logs to console for debugging

## Performance

### Blazor Server Mode
- Real-time updates via SignalR
- Server-side rendering
- Low bandwidth usage

### Database Performance
- Async/await throughout
- Efficient queries (especially with Dapper)
- No N+1 query issues

## Switching Between Providers

You can switch between EF Core and Dapper **at any time**:

1. Stop the application
2. Edit `Program.cs` line 16
3. Save the file
4. Run the application again

**Both providers use the same database and schema!**

## Validation

All forms include:
- Client-side validation
- Required field markers
- Validation messages
- Bootstrap styling for errors

## Responsive Design

The application is fully responsive:

- **Desktop:** Full-width tables, side-by-side layouts
- **Tablet:** Adapted grid layouts
- **Mobile:** Stacked layouts, scrollable tables

## Browser Support

- Chrome (latest)
- Edge (latest)
- Firefox (latest)
- Safari (latest)

## Dependencies

### NuGet Packages
- `Microsoft.EntityFrameworkCore.SqlServer` - EF Core provider
- `Microsoft.Data.SqlClient` - SQL Server client
- `Dapper` (via Infrastructure.Dapper) - Dapper ORM

### Project References
- `Application.Services` - Service interfaces
- `Domain.Entities` - Entity models
- `Infrastructure.EFCore` - EF Core implementation
- `Infrastructure.Dapper` - Dapper implementation

### External
- Bootstrap 5.3+ - UI framework
- Bootstrap Icons - Icon library

## Customization

### Add New Entity

1. Create service in `Services/` folder
2. Create Razor component in `Components/Pages/`
3. Add navigation link to `NavMenu.razor`
4. Add home page card in `Home.razor`

### Change Color Scheme

Edit `Shared.css`:
```css
/* Change gradient colors */
background: linear-gradient(135deg, #YOUR_COLOR1 0%, #YOUR_COLOR2 100%);
```

### Modify Layout

Edit `MainLayout.razor` for overall structure

## Troubleshooting

### Database Not Found

**Solution:** The app should auto-create it. If not:
```bash
sqlcmd -S "(localdb)\mssqllocaldb" -i "Infrastructure.Dapper\Scripts\InitializeDatabase.sql"
```

### Validation Errors Not Showing

**Solution:** Ensure `<DataAnnotationsValidator />` is in your `<EditForm>`

### Icons Not Displaying

**Solution:** Check `App.razor` includes Bootstrap Icons CDN:
```html
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.0/font/bootstrap-icons.css" />
```

### CSS Not Applying

**Solution:** Verify `Shared.css` is referenced in `App.razor`:
```html
<link rel="stylesheet" href="Components/Pages/Shared.css" />
```

## Security Considerations

For production deployment:

1. **Use HTTPS** - Always use SSL/TLS
2. **Validate Input** - Add server-side validation
3. **Sanitize Output** - Protect against XSS
4. **Use Authentication** - Add authentication/authorization
5. **Connection String** - Store in User Secrets or Azure Key Vault
6. **SQL Injection** - Using parameterized queries (both EF Core and Dapper)

## Future Enhancements

Potential features to add:

- Authentication and authorization
- Search and filtering
- Sorting and pagination
- Export to Excel/PDF
- Advanced reporting
- Dark mode toggle
- Real-time notifications
- Audit logging
- Soft deletes

## Support

For issues or questions:
1. Check the troubleshooting section
2. Review the console logs
3. Check the browser console for client errors
4. Review `TROUBLESHOOTING_DB.md` for database issues

## License

Same as the overall project license.

---

**Built with Blazor Server, Bootstrap, and modern web standards.**
