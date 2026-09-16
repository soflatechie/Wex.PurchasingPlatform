# C# Coding Standards

**Version:** 1.0
**Language:** C#
**Applies to:** All C# application and library development

---

## 1. Purpose

The purpose of these standards is to establish a consistent, readable, maintainable, and professional coding style for C# development.

These standards are based primarily on established Microsoft C# and .NET conventions, with several team-specific rules intended to improve code readability and maintainability.

The goal is not to prescribe every possible C# language feature. Developers should use good judgment and favor code that is clear, predictable, maintainable, and easy for another developer to understand.

---

# 2. General Principles

## 2.1 Write Self-Documenting Code

Code should explain **what it is doing through its structure, naming, and organization**.

Developers should not rely on comments to explain code that could instead be made clear through better naming or simpler implementation.

### Avoid

```csharp
// Check if the customer is active
if (customer.Status == 1)
    ActivateCustomer();
```

### Prefer

```csharp
if (customer.IsActive)
    ActivateCustomer();
```

Similarly, avoid:

```csharp
// Get the customer's orders
var orders = GetData(customer.Id);
```

Prefer:

```csharp
var customerOrders = GetCustomerOrders(customer.Id);
```

The second example communicates intent through naming rather than comments.

---

# 3. Comments

Comments should be used sparingly.

**Do not add comments simply to explain what ordinary code is doing.**

Comments are appropriate when they explain information that cannot reasonably be expressed through the code itself.

### Appropriate uses of comments

Comments may be used for:

* Complex algorithms
* Non-obvious business rules
* Workarounds for bugs or limitations
* External system behavior
* Performance considerations
* Important warnings or constraints
* Reasons why an unusual implementation is necessary

### Example

```csharp
// The API occasionally returns duplicate transactions.
// DistinctBy is intentionally applied before processing to prevent
// duplicate payments from being submitted.
var transactions = response.Transactions
    .DistinctBy(transaction => transaction.TransactionId);
```

### Avoid

```csharp
// Loop through customers
foreach (var customer in customers)
{
    // Send an email
    SendEmail(customer);
}
```

The code is already self-documenting.

---

# 4. Naming Conventions

Use meaningful names that clearly communicate intent.

Avoid abbreviations unless they are universally understood within the codebase.

## 4.1 Classes

Use **PascalCase**.

```csharp
public class CustomerService
{
}
```

```csharp
public class InvoiceRepository
{
}
```

Avoid:

```csharp
public class CustSvc
{
}
```

---

## 4.2 Methods

Use **PascalCase**.

Method names should describe an action.

```csharp
public Customer GetCustomer(int customerId)
{
}
```

```csharp
public void SendInvoice(int invoiceId)
{
}
```

Prefer verbs such as:

* Get
* Create
* Update
* Delete
* Calculate
* Validate
* Process
* Send
* Load
* Save
* Find

---

## 4.3 Properties

Use **PascalCase**.

```csharp
public string CustomerName { get; set; }
public DateTime CreatedDate { get; set; }
public bool IsActive { get; set; }
```

Boolean properties should generally be phrased as a question or state.

Prefer:

```csharp
IsActive
HasPermission
CanEdit
IsEnabled
```

Avoid:

```csharp
Active
Permission
Edit
Enabled
```

when the meaning is ambiguous.

---

## 4.4 Local Variables

Use **camelCase**.

```csharp
var customerName = customer.Name;
var invoiceTotal = CalculateInvoiceTotal(invoice);
```

Avoid:

```csharp
var CustomerName = customer.Name;
var invoice_total = CalculateInvoiceTotal(invoice);
```

---

## 4.5 Parameters

Use **camelCase**.

```csharp
public Customer GetCustomer(int customerId)
{
}
```

---

## 4.6 Private Fields

Private fields should use **camelCase** with an underscore prefix.

```csharp
private readonly ICustomerRepository _customerRepository;
private readonly ILogger<CustomerService> _logger;
```

---

## 4.7 Constants

Use **PascalCase**.

```csharp
private const int MaximumRetryCount = 3;
private const string DefaultStatus = "Active";
```

---

## 4.8 Interfaces

Interface names should begin with `I`.

```csharp
public interface ICustomerService
{
}
```

---

## 4.9 Enums

Use singular names unless the enum represents a collection of flags.

```csharp
public enum CustomerStatus
{
    Active,
    Inactive,
    Suspended
}
```

---

# 5. Formatting

## 5.1 Indentation

Use the **Visual Studio default indentation settings** for C#.

Developers should use the standard Visual Studio formatting behavior and should not manually configure different indentation conventions for individual projects or files unless a project has an explicitly documented requirement.

Code should be formatted using Visual Studio's built-in formatting capabilities to maintain consistent indentation throughout the codebase.

Example:

```csharp
public void ProcessCustomer(Customer customer)
{
    if (customer.IsActive)
        SendNotification(customer);
}
```

When formatting existing code, use Visual Studio's standard formatting commands rather than manually adjusting whitespace.

---

## 5.2 Braces

Opening braces should appear on the following line.

```csharp
public void ProcessCustomer()
{
    ProcessOrders();
}
```

This applies to:

* Classes
* Methods
* Properties with bodies
* Loops
* `switch`
* Multi-line conditional statements
* Exception handling

---

# 6. If Statements

## 6.1 Single Statement — No Braces

If an `if` statement contains exactly **one statement**, braces should not be used.

```csharp
if (customer == null)
    return;
```

```csharp
if (order.IsCancelled)
    CancelOrder(order);
```

```csharp
if (count == 0)
    return false;
```

This standard applies to both `if` and `else` statements.

```csharp
if (customer.IsActive)
    ActivateCustomer();
else
    DeactivateCustomer();
```

---

## 6.2 Multiple Statements — Use Braces

Braces are required when an `if`, `else`, loop, or similar construct contains multiple statements.

```csharp
if (customer.IsActive)
{
    UpdateCustomer(customer);
    SendNotification(customer);
}
```

---

## 6.3 Nested Statements

Avoid unnecessary nesting.

Prefer early returns:

```csharp
public void ProcessOrder(Order order)
{
    if (order == null)
        return;

    if (order.IsCancelled)
        return;

    ProcessOrderItems(order);
}
```

Instead of:

```csharp
public void ProcessOrder(Order order)
{
    if (order != null)
    {
        if (!order.IsCancelled)
        {
            ProcessOrderItems(order);
        }
    }
}
```

Early exits generally make the main path easier to understand.

---

## 6.4 Avoid Ambiguous Single-Line Conditionals

When adding another statement would require braces, add the braces rather than attempting to compress the code.

Prefer:

```csharp
if (customer.IsActive)
{
    UpdateCustomer(customer);
    SendNotification(customer);
}
```

Do not write:

```csharp
if (customer.IsActive)
    UpdateCustomer(customer); SendNotification(customer);
```

---

# 7. Else Statements

Use `else` when it improves readability.

```csharp
if (customer.IsActive)
    ActivateCustomer();
else
    DeactivateCustomer();
```

For more complicated logic, consider returning early rather than building deeply nested `if/else` structures.

---

# 8. Boolean Expressions

Write boolean expressions so their intent is immediately clear.

Prefer:

```csharp
if (customer.IsActive)
    ProcessCustomer(customer);
```

Avoid:

```csharp
if (customer.IsActive == true)
    ProcessCustomer(customer);
```

Similarly:

```csharp
if (!customer.IsActive)
    DisableCustomer(customer);
```

rather than:

```csharp
if (customer.IsActive == false)
    DisableCustomer(customer);
```

---

# 9. Null Checking

Use modern C# null-checking syntax where appropriate.

Prefer:

```csharp
if (customer is null)
    return;
```

and:

```csharp
if (customer is not null)
    ProcessCustomer(customer);
```

Use the null-coalescing operator when it improves clarity:

```csharp
var customerName = customer.Name ?? "Unknown";
```

Use null-coalescing assignment when appropriate:

```csharp
_customerCache ??= new CustomerCache();
```

---

# 10. Variable Declaration

Use `var` when the type is obvious from the right-hand side.

Prefer:

```csharp
var customer = new Customer();
var customers = GetCustomers();
var total = CalculateTotal();
```

Use the explicit type when it improves readability or the type is not obvious.

```csharp
Customer customer = GetCustomer(customerId);
```

Do not use `var` simply because it is shorter.

The goal is readability.

---

# 11. Object Initialization

Prefer object initializers.

```csharp
var customer = new Customer
{
    FirstName = "John",
    LastName = "Smith",
    IsActive = true
};
```

Instead of:

```csharp
var customer = new Customer();

customer.FirstName = "John";
customer.LastName = "Smith";
customer.IsActive = true;
```

---

# 12. Collection Initialization

Prefer collection initializers.

```csharp
var statuses = new List<string>
{
    "Active",
    "Inactive",
    "Suspended"
};
```

Use collection expressions when supported by the project's target C# version and when they improve readability.

```csharp
string[] statuses =
[
    "Active",
    "Inactive",
    "Suspended"
];
```

The language version supported by the application should be considered when selecting newer syntax.

---

# 13. Strings

Use string interpolation when constructing strings.

Prefer:

```csharp
var message = $"Customer {customer.Name} has been activated.";
```

Instead of:

```csharp
var message = "Customer " + customer.Name + " has been activated.";
```

Use `string.Empty` when an explicit empty string improves readability.

```csharp
var customerName = string.Empty;
```

For simple empty string comparisons, use:

```csharp
if (string.IsNullOrEmpty(customerName))
    return;
```

---

# 14. String Literals

Use double quotes for normal strings.

```csharp
var name = "John";
```

Use verbatim strings when appropriate.

```csharp
var path = @"C:\Applications\CustomerService";
```

Use raw string literals when they significantly improve readability for multi-line or complex strings.

```csharp
var json = """
{
    "name": "John",
    "active": true
}
""";
```

---

# 15. Methods

Methods should generally perform **one logical responsibility**.

Avoid methods that perform unrelated operations.

Prefer:

```csharp
var customer = GetCustomer(customerId);
ValidateCustomer(customer);
SaveCustomer(customer);
```

over a method that performs numerous unrelated responsibilities.

Methods should also generally be short enough that their purpose can be understood quickly.

---

# 16. Method Parameters

Avoid excessive numbers of parameters.

Avoid:

```csharp
CreateCustomer(
    firstName,
    lastName,
    address,
    city,
    state,
    zipCode,
    phone,
    email,
    dateOfBirth);
```

When appropriate, group related values into a meaningful object.

```csharp
CreateCustomer(customerInformation);
```

Do not introduce parameter objects simply to avoid a few reasonable parameters. Use judgment based on the domain.

---

# 17. Return Values

Return early when it makes the method easier to understand.

Prefer:

```csharp
public Customer GetCustomer(int customerId)
{
    if (customerId <= 0)
        return null;

    return _repository.GetCustomer(customerId);
}
```

rather than unnecessarily nesting the main operation:

```csharp
public Customer GetCustomer(int customerId)
{
    if (customerId > 0)
    {
        return _repository.GetCustomer(customerId);
    }

    return null;
}
```

---

# 18. Exception Handling

Do not catch exceptions unless the application can meaningfully handle them.

Avoid:

```csharp
try
{
    ProcessOrder(order);
}
catch (Exception)
{
}
```

Never silently swallow exceptions.

If an exception must be caught, handle it appropriately.

```csharp
try
{
    ProcessOrder(order);
}
catch (OrderProcessingException ex)
{
    _logger.LogError(ex, "Unable to process order {OrderId}.", order.Id);
    throw;
}
```

Catch the most specific exception type that can reasonably be handled.

---

# 19. Logging

Log information that is useful for diagnosing problems.

Avoid logging information that merely repeats what the code is doing.

Prefer:

```csharp
_logger.LogWarning(
    "Customer {CustomerId} could not be found.",
    customerId);
```

over:

```csharp
_logger.LogInformation("Getting customer.");
```

Never log passwords, authentication tokens, credit card numbers, or other sensitive information.

---

# 20. LINQ

Use LINQ when it makes collection operations clearer.

Prefer:

```csharp
var activeCustomers = customers
    .Where(customer => customer.IsActive)
    .ToList();
```

Avoid overly complicated LINQ statements that are difficult to understand.

If a LINQ expression becomes difficult to read, use multiple steps or a traditional loop.

Readability takes precedence over minimizing lines of code.

---

# 21. LINQ Formatting

Multi-line LINQ expressions should place each operation on its own line.

```csharp
var customers = orders
    .Where(order => order.IsActive)
    .Select(order => order.Customer)
    .Where(customer => customer.IsActive)
    .OrderBy(customer => customer.LastName)
    .ToList();
```

---

# 22. Switch Statements

Use `switch` when there are multiple mutually exclusive conditions.

```csharp
switch (customer.Status)
{
    case CustomerStatus.Active:
        ActivateCustomer();
        break;

    case CustomerStatus.Inactive:
        DeactivateCustomer();
        break;

    default:
        HandleUnknownStatus();
        break;
}
```

For simple expressions, a switch expression may be preferable.

```csharp
var description = customer.Status switch
{
    CustomerStatus.Active => "Active customer",
    CustomerStatus.Inactive => "Inactive customer",
    _ => "Unknown status"
};
```

Choose the form that is easiest to understand.

---

# 23. Ternary Operator

Use the conditional operator for simple expressions.

```csharp
var status = customer.IsActive ? "Active" : "Inactive";
```

Do not use nested or complicated ternary expressions.

Avoid:

```csharp
var result = a ? b ? c : d : e;
```

Use an `if` statement or switch expression when the logic becomes difficult to read.

---

# 24. Async and Await

Use `async` and `await` for asynchronous operations.

```csharp
public async Task<Customer> GetCustomerAsync(int customerId)
{
    return await _repository.GetCustomerAsync(customerId);
}
```

Asynchronous methods should generally end with `Async`.

```csharp
GetCustomerAsync()
SaveCustomerAsync()
DeleteCustomerAsync()
```

Avoid blocking asynchronous code.

Do not use:

```csharp
var customer = GetCustomerAsync(id).Result;
```

or:

```csharp
var customer = GetCustomerAsync(id).GetAwaiter().GetResult();
```

when an asynchronous alternative is available.

---

# 25. Access Modifiers

Explicitly specify access modifiers.

Prefer:

```csharp
private void ProcessCustomer()
{
}
```

rather than relying on implicit accessibility.

Classes, methods, properties, and fields should have clearly defined visibility.

---

# 26. Properties

Prefer auto-properties when no additional behavior is required.

```csharp
public string Name { get; set; }
```

Use expression-bodied properties for simple calculated values.

```csharp
public string FullName => $"{FirstName} {LastName}";
```

Avoid creating unnecessary backing fields.

---

# 27. Read-Only Values

Use `readonly` for fields that should only be assigned during initialization.

```csharp
private readonly ICustomerService _customerService;
```

Prefer immutable or read-only data where practical.

---

# 28. Dependency Injection

Use dependency injection rather than creating service dependencies directly inside classes.

Prefer:

```csharp
public CustomerService(ICustomerRepository customerRepository)
{
    _customerRepository = customerRepository;
}
```

Avoid:

```csharp
public CustomerService()
{
    _customerRepository = new CustomerRepository();
}
```

Dependency injection improves testability and reduces coupling.

---

# 29. Avoid Magic Numbers and Strings

Do not embed unexplained values directly in business logic.

Avoid:

```csharp
if (retryCount > 3)
    return;
```

Prefer:

```csharp
private const int MaximumRetryCount = 3;

if (retryCount > MaximumRetryCount)
    return;
```

The same principle applies to meaningful strings.

---

# 30. Classes

Classes should have a clear responsibility.

Avoid creating large "god classes" that contain unrelated functionality.

Prefer:

```text
CustomerService
CustomerRepository
CustomerValidator
CustomerMapper
```

over:

```text
CustomerManager
```

containing every possible operation related to customers.

---

# 31. File Organization

Generally, organize a class file in the following order:

1. Namespace
2. Class declaration
3. Constants
4. Fields
5. Constructors
6. Properties
7. Public methods
8. Private methods

Example:

```csharp
namespace Application.Services;

public class CustomerService
{
    private const int MaximumRetryCount = 3;

    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public Customer GetCustomer(int customerId)
    {
        if (customerId <= 0)
            return null;

        return _repository.GetCustomer(customerId);
    }

    private bool IsValid(Customer customer)
    {
        return customer is not null;
    }
}
```

---

# 32. Namespaces

Use file-scoped namespaces when supported by the project's C# version.

Prefer:

```csharp
namespace Application.Services;

public class CustomerService
{
}
```

over:

```csharp
namespace Application.Services
{
    public class CustomerService
    {
    }
}
```

---

# 33. Using Statements

Place `using` statements at the top of the file.

Remove unused `using` statements.

Organize namespaces consistently across the project.

Avoid fully qualifying types throughout the code unless doing so resolves an ambiguity or improves clarity.

---

# 34. Static Members

Use static members when the behavior does not depend on instance state.

Do not make methods static simply to avoid dependency injection or object design.

---

# 35. Avoid Unnecessary Code

Do not write code that does not add value.

Avoid:

```csharp
if (customer != null)
{
    return customer;
}
else
{
    return null;
}
```

Prefer:

```csharp
return customer;
```

Likewise, avoid unnecessary temporary variables:

```csharp
var customer = GetCustomer(customerId);
return customer;
```

when:

```csharp
return GetCustomer(customerId);
```

is equally clear.

---

# 36. Readability Over Cleverness

Code should be written for the developer who will maintain it, not for the developer who wrote it.

Do not use advanced C# features merely because they reduce the number of lines.

Prefer clear code over clever code.

For example, this:

```csharp
if (customer.IsActive)
    ProcessCustomer(customer);
```

is preferable to a complicated expression that attempts to perform the same operation in fewer characters.

---

# 37. Formatting Long Expressions

Break long expressions into logical lines.

Prefer:

```csharp
var customers = customers
    .Where(customer => customer.IsActive)
    .Where(customer => customer.HasValidEmail)
    .OrderBy(customer => customer.LastName)
    .ToList();
```

rather than creating excessively long lines.

---

# 38. One Type Per File

Generally, place one primary class, interface, enum, or record in each file.

The file name should match the type name.

```text
CustomerService.cs
CustomerRepository.cs
ICustomerService.cs
CustomerStatus.cs
```

Small related types may be grouped when doing so clearly improves organization.

---

# 39. Records

Use records when the type represents data where value-based equality and immutability are appropriate.

```csharp
public record CustomerSummary(
    int Id,
    string Name,
    bool IsActive);
```

Do not use records simply because they are newer C# syntax.

Choose the type that best represents the domain.

---

# 40. Pattern Matching

Use modern pattern matching when it improves clarity.

```csharp
if (customer is null)
    return;
```

```csharp
if (order is { IsActive: true, Total: > 0 })
    ProcessOrder(order);
```

Do not use complicated patterns simply to avoid straightforward code.

---

# 41. Expression-Bodied Members

Expression-bodied members are appropriate for simple operations.

```csharp
public string FullName => $"{FirstName} {LastName}";
```

```csharp
public bool IsValid() => !string.IsNullOrEmpty(Name);
```

Use normal block bodies when the implementation contains multiple operations or is easier to understand that way.

---

# 42. Unit Tests

Test names should clearly communicate the scenario being tested.

Prefer:

```csharp
GetCustomer_WhenCustomerDoesNotExist_ReturnsNull()
```

over:

```csharp
TestGetCustomer()
```

Tests should follow the Arrange/Act/Assert structure.

```csharp
// Arrange
var customerId = 123;

// Act
var customer = service.GetCustomer(customerId);

// Assert
Assert.IsNull(customer);
```

**Note:** Test comments are acceptable when they provide meaningful structure or clarification. They should not be used to compensate for poorly named variables or unclear test code.

---

# 43. Code Review Expectations

Code submitted for review should:

* Follow these standards
* Be formatted consistently
* Avoid unnecessary comments
* Use meaningful names
* Avoid unnecessary complexity
* Remove unused code
* Remove debugging statements
* Avoid unrelated changes
* Include appropriate tests
* Handle errors appropriately

Reviewers should focus primarily on correctness, maintainability, security, performance, and readability rather than personal stylistic preferences.

---

# 44. Standard Priorities

When deciding between competing coding approaches, use the following priorities:

1. **Correctness**
2. **Security**
3. **Maintainability**
4. **Readability**
5. **Testability**
6. **Performance**
7. **Conciseness**

Shorter code is not automatically better code.

---

# 45. Quick Reference

### Naming

| Item           | Convention     | Example               |
| -------------- | -------------- | --------------------- |
| Class          | PascalCase     | `CustomerService`     |
| Interface      | I + PascalCase | `ICustomerService`    |
| Method         | PascalCase     | `GetCustomer()`       |
| Property       | PascalCase     | `CustomerName`        |
| Local variable | camelCase      | `customerName`        |
| Parameter      | camelCase      | `customerId`          |
| Private field  | `_camelCase`   | `_customerRepository` |
| Constant       | PascalCase     | `MaximumRetryCount`   |
| Enum           | PascalCase     | `CustomerStatus`      |

### Conditional Statements

Single statement:

```csharp
if (customer is null)
    return;
```

Multiple statements:

```csharp
if (customer.IsActive)
{
    UpdateCustomer(customer);
    SendNotification(customer);
}
```

### Self-Documenting Code

Prefer:

```csharp
if (customer.HasOutstandingBalance)
    SendPaymentReminder(customer);
```

over:

```csharp
// If the customer owes money, send a reminder
if (customer.Balance > 0)
    SendPaymentReminder(customer);
```

### General Rule

> **Write code that explains itself. Use comments only when the reason or complexity cannot be adequately expressed through the code itself.**

> **For a single statement controlled by an `if`, `else`, `for`, `foreach`, `while`, or similar statement, omit braces. Use braces whenever the controlled block contains more than one statement.**

---

# 46. Final Standard

The purpose of these standards is consistency, not rigidity.

Developers should favor code that another experienced C# developer can understand quickly without needing additional explanation.

When a developer encounters code that requires a comment to explain what it does, the first question should be:

**"Can I make the code itself clearer?"**

Only when the answer is no should a comment normally be added.

Likewise, developers should not introduce unnecessary complexity merely to follow a particular language feature or style preference.

**Clear, maintainable, self-documenting code is the primary objective.**
