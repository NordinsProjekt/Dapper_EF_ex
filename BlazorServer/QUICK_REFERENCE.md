# Blazor Server - Quick Reference

## Switch Data Provider

**File:** `BlazorServer\Program.cs` (Line 16)

```csharp
// Use EF Core
const DataProvider DATA_PROVIDER = DataProvider.EFCore;

// Use Dapper
const DataProvider DATA_PROVIDER = DataProvider.Dapper;
```

## Run Application

```bash
cd BlazorServer
dotnet run
```

Then open: `https://localhost:5001`

## Pages

| URL | Page | Description |
|-----|------|-------------|
| `/` | Home | Dashboard with navigation cards |
| `/customers` | Customers | Customer CRUD operations |
| `/products` | Products | Product CRUD operations |
| `/employees` | Employees | Employee CRUD operations |

## Common Tasks

### Add New Customer
1. Navigate to `/customers`
2. Click "Add Customer" button
3. Fill in form (First Name, Last Name, Email required)
4. Click "Save"

### Edit Customer
1. Navigate to `/customers`
2. Click pencil icon next to customer
3. Update fields
4. Click "Save"

### Delete Customer
1. Navigate to `/customers`
2. Click trash icon next to customer
3. Confirm deletion

### Check Data Provider
Look at the homepage badge - shows "EFCore" or "Dapper"

## Project Structure

```
BlazorServer/
??? Components/Pages/     # Razor pages (Customers, Products, Employees)
??? Services/             # Business logic services
??? Program.cs            # DI configuration & data provider setup
??? appsettings.json      # Connection string
```

## Files to Edit

### Change Data Provider
`Program.cs` - Line 16

### Modify Customer Page
`Components/Pages/Customers.razor`

### Modify Product Page
`Components/Pages/Products.razor`

### Modify Employee Page
`Components/Pages/Employees.razor`

### Change Styles
`Components/Pages/Shared.css`

### Update Navigation
`Components/Layout/NavMenu.razor`

### Change Homepage
`Components/Pages/Home.razor`

## Database Commands

### Reset Database
```bash
sqlcmd -S "(localdb)\mssqllocaldb" -i "Infrastructure.Dapper\Scripts\CompleteReset.sql"
```

### Initialize Database (Dapper)
```bash
sqlcmd -S "(localdb)\mssqllocaldb" -i "Infrastructure.Dapper\Scripts\InitializeDatabase.sql"
```

### Apply Migrations (EF Core)
```bash
cd Infrastructure.EFCore
dotnet ef database update
```

## Console Output

When starting the app, you'll see:

```
========================================
  BLAZOR SERVER - KIOSK MANAGEMENT
========================================
Data Provider: EFCore
Environment: Development
========================================

Initializing database...
Database is up to date.

Application started successfully!
Navigate to the application in your browser.
```

## Key Features

### CRUD Operations
- Create: Modal form with validation
- Read: Data table with sorting
- Update: Modal form pre-populated
- Delete: Confirmation dialog

### UI Components
- Loading spinners
- Error messages
- Empty states
- Success feedback
- Responsive design

### Data Providers
- **EF Core**: Full ORM, LINQ, migrations
- **Dapper**: Lightweight, fast, SQL control

## Keyboard Shortcuts (Browser)

- `F5` - Refresh page
- `Ctrl+Shift+I` - Open developer tools
- `Esc` - Close modal (on some browsers)

## Troubleshooting Quick Fixes

### Port Already in Use
Edit `Properties/launchSettings.json` to change port

### Database Not Found
App should auto-create. If not, run initialization script manually.

### Validation Not Working
Check `<DataAnnotationsValidator />` in EditForm

### Icons Not Showing
Verify Bootstrap Icons CDN in `App.razor`

### CSS Not Applied
Clear browser cache (Ctrl+F5)

## Service Methods

### CustomerService
```csharp
await GetAllCustomersAsync()
await GetCustomerByIdAsync(Guid id)
await CreateCustomerAsync(firstName, lastName, email, phone)
await UpdateCustomerAsync(customer)
await DeleteCustomerAsync(Guid id)
```

### ProductService
```csharp
await GetAllProductsAsync()
await GetProductByIdAsync(Guid id)
await CreateProductAsync(name, description, price, stock, sku)
await UpdateProductAsync(product)
await DeleteProductAsync(Guid id)
```

### EmployeeService
```csharp
await GetAllEmployeesAsync()
await GetEmployeeByIdAsync(Guid id)
await CreateEmployeeAsync(firstName, lastName, email, phone, hireDate, rate, isActive)
await UpdateEmployeeAsync(employee)
await DeleteEmployeeAsync(Guid id)
```

## Color Scheme

- **Primary:** Purple gradient (#667eea to #764ba2)
- **Success:** Green (#48bb78)
- **Danger:** Red (#f56565)
- **Info:** Blue (#4299e1)
- **Warning:** Yellow (#ed8936)

## Bootstrap Classes Used

- `btn btn-primary` - Primary button
- `btn btn-sm` - Small button
- `table` - Data table
- `modal` - Modal dialog
- `alert alert-danger` - Error message
- `badge bg-success` - Status badge
- `form-control` - Input field
- `spinner-border` - Loading spinner

## Custom CSS Classes

- `.page-container` - Page wrapper
- `.page-header` - Page title area
- `.data-table` - Table container
- `.empty-state` - No data message
- `.loading-spinner` - Loading indicator
- `.avatar` - User initials circle
- `.price` - Price formatting

## Icons Used (Bootstrap Icons)

- `bi-people-fill` - Customers
- `bi-box-seam-fill` - Products
- `bi-person-badge-fill` - Employees
- `bi-plus-circle` - Add button
- `bi-pencil` - Edit button
- `bi-trash` - Delete button
- `bi-house-door-fill` - Home
- `bi-exclamation-triangle-fill` - Error

## Form Validation

All required fields show validation:
- First Name, Last Name, Email (Customers)
- Name, Price, Stock Quantity (Products)
- First Name, Last Name, Email, Hire Date, Hourly Rate (Employees)

## Entity Properties

### Customer
- Id (Guid)
- FirstName (string)
- LastName (string)
- Email (string)
- Phone (string?)
- CreatedAt (DateTime)
- UpdatedAt (DateTime?)

### Product
- Id (Guid)
- Name (string)
- Description (string?)
- Price (decimal)
- StockQuantity (int)
- SKU (string?)
- CreatedAt (DateTime)
- UpdatedAt (DateTime?)

### Employee
- Id (Guid)
- FirstName (string)
- LastName (string)
- Email (string)
- Phone (string?)
- HireDate (DateTime)
- HourlyRate (decimal)
- IsActive (bool)

---

**For detailed documentation, see `BlazorServer\README.md`**
