using Presentation.KioskViewer.Services;

namespace Presentation.KioskViewer.UI;

public class MainMenu
{
    private readonly CustomerServiceWrapper _customerService;
    private readonly ProductServiceWrapper _productService;
    private readonly EmployeeServiceWrapper _employeeService;
    private readonly string _dataProvider;

    public MainMenu(CustomerServiceWrapper customerService, ProductServiceWrapper productService, EmployeeServiceWrapper employeeService, string dataProvider)
    {
        _customerService = customerService;
        _productService = productService;
        _employeeService = employeeService;
        _dataProvider = dataProvider;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("???????????????????????????????????????");
            Console.WriteLine("         KIOSK MANAGEMENT SYSTEM       ");
            Console.WriteLine($"      Data Provider: {_dataProvider}");
            Console.WriteLine("???????????????????????????????????????");
            Console.WriteLine();
            Console.WriteLine("1. Customer Management");
            Console.WriteLine("2. Product Management");
            Console.WriteLine("3. Employee Management");
            Console.WriteLine("0. Exit");
            Console.WriteLine();
            Console.Write("Select an option: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await ShowCustomerMenuAsync();
                    break;
                case "2":
                    await ShowProductMenuAsync();
                    break;
                case "3":
                    await ShowEmployeeMenuAsync();
                    break;
                case "0":
                    Console.WriteLine("\nThank you for using Kiosk Management System!");
                    return;
                default:
                    Console.WriteLine("\nInvalid option. Press any key to continue...");
                    Console.ReadKey();
                    break;
            }
        }
    }

    private async Task ShowCustomerMenuAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("???????????????????????????????????????");
            Console.WriteLine("         CUSTOMER MANAGEMENT           ");
            Console.WriteLine("???????????????????????????????????????");
            Console.WriteLine();
            Console.WriteLine("1. List All Customers");
            Console.WriteLine("2. Add New Customer");
            Console.WriteLine("3. Update Customer");
            Console.WriteLine("4. Delete Customer");
            Console.WriteLine("0. Back to Main Menu");
            Console.WriteLine();
            Console.Write("Select an option: ");

            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await ListCustomersAsync();
                        break;
                    case "2":
                        await AddCustomerAsync();
                        break;
                    case "3":
                        await UpdateCustomerAsync();
                        break;
                    case "4":
                        await DeleteCustomerAsync();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("\nInvalid option.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }

            if (choice != "0")
            {
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }
    }

    private async Task ListCustomersAsync()
    {
        Console.WriteLine("\n--- Customer List ---");
        var customers = await _customerService.GetAllCustomersAsync();
        
        if (!customers.Any())
        {
            Console.WriteLine("No customers found.");
            return;
        }

        Console.WriteLine($"\n{"ID",-38} {"Name",-30} {"Email",-30} {"Phone",-15}");
        Console.WriteLine(new string('-', 115));
        
        foreach (var customer in customers)
        {
            Console.WriteLine($"{customer.Id,-38} {customer.FirstName + " " + customer.LastName,-30} {customer.Email,-30} {customer.Phone ?? "N/A",-15}");
        }
    }

    private async Task AddCustomerAsync()
    {
        Console.WriteLine("\n--- Add New Customer ---");
        
        Console.Write("First Name: ");
        var firstName = Console.ReadLine() ?? "";
        
        Console.Write("Last Name: ");
        var lastName = Console.ReadLine() ?? "";
        
        Console.Write("Email: ");
        var email = Console.ReadLine() ?? "";
        
        Console.Write("Phone (optional): ");
        var phone = Console.ReadLine();

        var customer = await _customerService.CreateCustomerAsync(firstName, lastName, email, string.IsNullOrWhiteSpace(phone) ? null : phone);
        Console.WriteLine($"\nCustomer created successfully with ID: {customer.Id}");
    }

    private async Task UpdateCustomerAsync()
    {
        Console.Write("\nEnter Customer ID to update: ");
        if (!Guid.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Invalid ID format.");
            return;
        }

        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
        {
            Console.WriteLine("Customer not found.");
            return;
        }

        Console.WriteLine($"\nCurrent: {customer.FirstName} {customer.LastName} ({customer.Email})");
        
        Console.Write("New First Name (leave empty to keep current): ");
        var firstName = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(firstName)) customer.FirstName = firstName;
        
        Console.Write("New Last Name (leave empty to keep current): ");
        var lastName = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(lastName)) customer.LastName = lastName;
        
        Console.Write("New Email (leave empty to keep current): ");
        var email = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(email)) customer.Email = email;
        
        Console.Write("New Phone (leave empty to keep current): ");
        var phone = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(phone)) customer.Phone = phone;

        await _customerService.UpdateCustomerAsync(customer);
        Console.WriteLine("\nCustomer updated successfully.");
    }

    private async Task DeleteCustomerAsync()
    {
        Console.Write("\nEnter Customer ID to delete: ");
        if (!Guid.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Invalid ID format.");
            return;
        }

        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
        {
            Console.WriteLine("Customer not found.");
            return;
        }

        Console.Write($"Are you sure you want to delete {customer.FirstName} {customer.LastName}? (y/n): ");
        if (Console.ReadLine()?.ToLower() == "y")
        {
            await _customerService.DeleteCustomerAsync(id);
            Console.WriteLine("\nCustomer deleted successfully.");
        }
    }

    private async Task ShowProductMenuAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("???????????????????????????????????????");
            Console.WriteLine("         PRODUCT MANAGEMENT            ");
            Console.WriteLine("???????????????????????????????????????");
            Console.WriteLine();
            Console.WriteLine("1. List All Products");
            Console.WriteLine("2. Add New Product");
            Console.WriteLine("3. Update Product");
            Console.WriteLine("4. Delete Product");
            Console.WriteLine("0. Back to Main Menu");
            Console.WriteLine();
            Console.Write("Select an option: ");

            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await ListProductsAsync();
                        break;
                    case "2":
                        await AddProductAsync();
                        break;
                    case "3":
                        await UpdateProductAsync();
                        break;
                    case "4":
                        await DeleteProductAsync();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("\nInvalid option.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }

            if (choice != "0")
            {
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }
    }

    private async Task ListProductsAsync()
    {
        Console.WriteLine("\n--- Product List ---");
        var products = await _productService.GetAllProductsAsync();
        
        if (!products.Any())
        {
            Console.WriteLine("No products found.");
            return;
        }

        Console.WriteLine($"\n{"ID",-38} {"Name",-30} {"Price",-12} {"Stock",-8} {"SKU",-15}");
        Console.WriteLine(new string('-', 105));
        
        foreach (var product in products)
        {
            Console.WriteLine($"{product.Id,-38} {product.Name,-30} ${product.Price,-11:F2} {product.StockQuantity,-8} {product.SKU ?? "N/A",-15}");
        }
    }

    private async Task AddProductAsync()
    {
        Console.WriteLine("\n--- Add New Product ---");
        
        Console.Write("Name: ");
        var name = Console.ReadLine() ?? "";
        
        Console.Write("Description (optional): ");
        var description = Console.ReadLine();
        
        Console.Write("Price: ");
        if (!decimal.TryParse(Console.ReadLine(), out var price))
        {
            Console.WriteLine("Invalid price format.");
            return;
        }
        
        Console.Write("Stock Quantity: ");
        if (!int.TryParse(Console.ReadLine(), out var stock))
        {
            Console.WriteLine("Invalid stock format.");
            return;
        }
        
        Console.Write("SKU (optional): ");
        var sku = Console.ReadLine();

        var product = await _productService.CreateProductAsync(name, string.IsNullOrWhiteSpace(description) ? null : description, price, stock, string.IsNullOrWhiteSpace(sku) ? null : sku);
        Console.WriteLine($"\nProduct created successfully with ID: {product.Id}");
    }

    private async Task UpdateProductAsync()
    {
        Console.Write("\nEnter Product ID to update: ");
        if (!Guid.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Invalid ID format.");
            return;
        }

        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            Console.WriteLine("Product not found.");
            return;
        }

        Console.WriteLine($"\nCurrent: {product.Name} - ${product.Price} (Stock: {product.StockQuantity})");
        
        Console.Write("New Name (leave empty to keep current): ");
        var name = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(name)) product.Name = name;
        
        Console.Write("New Price (leave empty to keep current): ");
        var priceStr = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(priceStr) && decimal.TryParse(priceStr, out var price)) 
            product.Price = price;
        
        Console.Write("New Stock Quantity (leave empty to keep current): ");
        var stockStr = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(stockStr) && int.TryParse(stockStr, out var stock)) 
            product.StockQuantity = stock;

        await _productService.UpdateProductAsync(product);
        Console.WriteLine("\nProduct updated successfully.");
    }

    private async Task DeleteProductAsync()
    {
        Console.Write("\nEnter Product ID to delete: ");
        if (!Guid.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Invalid ID format.");
            return;
        }

        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            Console.WriteLine("Product not found.");
            return;
        }

        Console.Write($"Are you sure you want to delete {product.Name}? (y/n): ");
        if (Console.ReadLine()?.ToLower() == "y")
        {
            await _productService.DeleteProductAsync(id);
            Console.WriteLine("\nProduct deleted successfully.");
        }
    }

    private async Task ShowEmployeeMenuAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("???????????????????????????????????????");
            Console.WriteLine("         EMPLOYEE MANAGEMENT           ");
            Console.WriteLine("???????????????????????????????????????");
            Console.WriteLine();
            Console.WriteLine("1. List All Employees");
            Console.WriteLine("2. Add New Employee");
            Console.WriteLine("3. Update Employee");
            Console.WriteLine("4. Delete Employee");
            Console.WriteLine("0. Back to Main Menu");
            Console.WriteLine();
            Console.Write("Select an option: ");

            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await ListEmployeesAsync();
                        break;
                    case "2":
                        await AddEmployeeAsync();
                        break;
                    case "3":
                        await UpdateEmployeeAsync();
                        break;
                    case "4":
                        await DeleteEmployeeAsync();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("\nInvalid option.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }

            if (choice != "0")
            {
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }
    }

    private async Task ListEmployeesAsync()
    {
        Console.WriteLine("\n--- Employee List ---");
        var employees = await _employeeService.GetAllEmployeesAsync();
        
        if (!employees.Any())
        {
            Console.WriteLine("No employees found.");
            return;
        }

        Console.WriteLine($"\n{"ID",-38} {"Name",-30} {"Email",-30} {"Hourly Rate",-12} {"Active",-8}");
        Console.WriteLine(new string('-', 120));
        
        foreach (var employee in employees)
        {
            Console.WriteLine($"{employee.Id,-38} {employee.FirstName + " " + employee.LastName,-30} {employee.Email,-30} ${employee.HourlyRate,-11:F2} {(employee.IsActive ? "Yes" : "No"),-8}");
        }
    }

    private async Task AddEmployeeAsync()
    {
        Console.WriteLine("\n--- Add New Employee ---");
        
        Console.Write("First Name: ");
        var firstName = Console.ReadLine() ?? "";
        
        Console.Write("Last Name: ");
        var lastName = Console.ReadLine() ?? "";
        
        Console.Write("Email: ");
        var email = Console.ReadLine() ?? "";
        
        Console.Write("Phone (optional): ");
        var phone = Console.ReadLine();
        
        Console.Write("Hire Date (YYYY-MM-DD, leave empty for today): ");
        var hireDateStr = Console.ReadLine();
        DateTime hireDate = DateTime.Today;
        if (!string.IsNullOrWhiteSpace(hireDateStr) && DateTime.TryParse(hireDateStr, out var parsedDate))
        {
            hireDate = parsedDate;
        }
        
        Console.Write("Hourly Rate: ");
        if (!decimal.TryParse(Console.ReadLine(), out var rate))
        {
            Console.WriteLine("Invalid hourly rate format.");
            return;
        }

        var employee = await _employeeService.CreateEmployeeAsync(
            firstName, 
            lastName, 
            email, 
            string.IsNullOrWhiteSpace(phone) ? null : phone,
            hireDate,
            rate);
        Console.WriteLine($"\nEmployee created successfully with ID: {employee.Id}");
    }

    private async Task UpdateEmployeeAsync()
    {
        Console.Write("\nEnter Employee ID to update: ");
        if (!Guid.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Invalid ID format.");
            return;
        }

        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
        {
            Console.WriteLine("Employee not found.");
            return;
        }

        Console.WriteLine($"\nCurrent: {employee.FirstName} {employee.LastName} - ${employee.HourlyRate}/hr");
        
        Console.Write("New First Name (leave empty to keep current): ");
        var firstName = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(firstName)) employee.FirstName = firstName;
        
        Console.Write("New Last Name (leave empty to keep current): ");
        var lastName = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(lastName)) employee.LastName = lastName;
        
        Console.Write("New Email (leave empty to keep current): ");
        var email = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(email)) employee.Email = email;
        
        Console.Write("New Hourly Rate (leave empty to keep current): ");
        var rateStr = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(rateStr) && decimal.TryParse(rateStr, out var rate)) 
            employee.HourlyRate = rate;

        await _employeeService.UpdateEmployeeAsync(employee);
        Console.WriteLine("\nEmployee updated successfully.");
    }

    private async Task DeleteEmployeeAsync()
    {
        Console.Write("\nEnter Employee ID to delete: ");
        if (!Guid.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Invalid ID format.");
            return;
        }

        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
        {
            Console.WriteLine("Employee not found.");
            return;
        }

        Console.Write($"Are you sure you want to delete {employee.FirstName} {employee.LastName}? (y/n): ");
        if (Console.ReadLine()?.ToLower() == "y")
        {
            await _employeeService.DeleteEmployeeAsync(id);
            Console.WriteLine("\nEmployee deleted successfully.");
        }
    }
}
