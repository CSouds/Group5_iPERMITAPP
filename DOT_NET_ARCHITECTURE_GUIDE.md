# .NET Core MVC Architecture Guide
## For Experienced Programmers New to .NET

---

## 1. Project Structure & Execution Flow

### How the App Starts: `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);
```
This is like Spring Boot or Django's app initialization. It sets up dependency injection, logging, and middleware.

```csharp
builder.Services.AddControllersWithViews();
```
Registers MVC controllers and Razor views as services. In your IoC container.

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```
Registers the database context (like Hibernate SessionFactory or SQLAlchemy's Session). SQLite is the database provider.

```csharp
builder.Services.AddSession(options => { ... });
```
Enables session state (like servlet sessions in Java or Flask sessions in Python). Stores user ID, role, name.

```csharp
var app = builder.Build();
app.MapControllerRoute(...);
app.Run();
```
Builds the middleware pipeline, maps routes, and starts the HTTP server.

---

## 2. Request Lifecycle

### The HTTP Request Flow

1. **HTTP Request arrives** → `https://localhost:5001/Account/Login`

2. **Routing** (in `Program.cs`):
   ```csharp
   app.MapControllerRoute(
       name: "default",
       pattern: "{controller=Home}/{action=Index}/{id?}");
   ```
   Matches `/Account/Login` → `AccountController` class, `Login` method

3. **Controller Action Execution**:
   ```csharp
   public class AccountController : Controller
   {
       [HttpGet]
       public IActionResult Login()
       {
           return View(new LoginViewModel());
       }
   }
   ```
   - `[HttpGet]` = only handles GET requests
   - `View()` = renders a Razor template (`.cshtml` file)
   - Returns an `IActionResult` (interface for any HTTP response)

4. **View Rendering**:
   - Framework looks for `Views/Account/Login.cshtml`
   - Passes the `LoginViewModel` as the model
   - Renders HTML and returns to client

5. **Response** → Browser displays login form

---

## 3. Models (Data & Domain Objects)

### Entity Models vs View Models

#### Entity Models
These map to database tables:
```csharp
public class RE
{
    [Key]  // Database primary key
    public string ID { get; set; }
    
    public string Email { get; set; }
    
    [DataAnnotations.Required]  // Both validation and schema
    public string Password { get; set; }
    
    // Navigation property (relationship)
    public virtual ICollection<PermitRequest> PermitRequests { get; set; }
}
```

**Key differences from other ORMs:**
- `[Key]` = `@Id` in JPA or `__tablename__` in SQLAlchemy
- `[DataAnnotations.Required]` = `NOT NULL` in SQL + validation
- `virtual` = lazy loading (loads related data on access)
- `ICollection` = one-to-many relationship

#### View Models
These are **data transfer objects** for forms:
```csharp
public class LoginViewModel
{
    [Required(ErrorMessage = "User ID is required")]
    [Display(Name = "User ID")]
    public string UserID { get; set; }
    
    [DataType(DataType.Password)]
    public string Password { get; set; }
}
```

These decouple your database schema from your UI. Different views might need different data.

---

## 4. Database Layer (Entity Framework Core)

### DbContext = Session/Connection Manager

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<RE> REs { get; set; }
    public DbSet<PermitRequest> PermitRequests { get; set; }
    public DbSet<Payment> Payments { get; set; }
    // ... etc
}
```

Think of this like:
- **Hibernate:** SessionFactory, each DbSet is a @Entity
- **SQLAlchemy:** Session, each DbSet is a mapped class
- **Django:** Models, each DbSet is a Model

`DbSet<T>` = represents a table and provides LINQ query methods

### Relationships

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // One RE has many PermitRequests
    modelBuilder.Entity<PermitRequest>()
        .HasOne(pr => pr.RequestedBy)      // Foreign key property
        .WithMany(r => r.PermitRequests)   // Navigation collection
        .HasForeignKey(pr => pr.REID)      // FK column name
        .OnDelete(DeleteBehavior.Cascade); // Delete orphans
}
```

This is like:
- **JPA:** `@OneToMany(mappedBy="...")` + `@ManyToOne` + `@JoinColumn`
- **SQLAlchemy:** `relationship()` + `ForeignKey()`
- **Django:** `ForeignKey()` + `related_name`

### Querying

```csharp
var permits = await _context.PermitRequests
    .Include(pr => pr.RequestedBy)        // JOIN
    .Include(pr => pr.Statuses)            // LEFT JOIN
    .Where(pr => pr.REID == userId)        // WHERE clause
    .OrderByDescending(pr => pr.DateOfRequest)
    .ToListAsync();                        // Execute query
```

This is LINQ - similar to:
- **Hibernate:** `session.query(PermitRequest).join(...).filter(...)`
- **SQLAlchemy:** `session.query(PermitRequest).join(...).filter(...)`
- **Django ORM:** `PermitRequest.objects.select_related(...).filter(...)`

**`async/await`** = the method returns a `Task<List<PermitRequest>>`. You must `await` it. This is like Python's `async def` or JavaScript's `async/await`.

---

## 5. Controllers (Request Handlers)

### Basic Structure

```csharp
public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    
    // Dependency Injection via constructor
    public AccountController(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }
}
```

The `_context` and `_emailService` are injected by the DI container (set up in `Program.cs`).

### Action Method (HTTP Handler)

```csharp
[HttpPost]
[ValidateAntiForgeryToken]  // CSRF protection
public async Task<IActionResult> Login(LoginViewModel model)
{
    if (!ModelState.IsValid)  // Validation from DataAnnotations
        return View(model);
    
    var user = await _context.REs.FirstOrDefaultAsync(r => r.ID == model.UserID);
    
    if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
    {
        HttpContext.Session.SetString("UserID", user.ID);
        return RedirectToAction("Dashboard", "PermitRequest");
    }
    
    ModelState.AddModelError("", "Invalid credentials");
    return View(model);
}
```

**Breaking it down:**
- `[HttpPost]` = only handles POST requests (form submission)
- `async Task<IActionResult>` = asynchronous, returns any HTTP response type
- `ModelState.IsValid` = checks all `[Required]`, `[Range]`, etc. annotations
- `BCrypt.Verify()` = securely check password against hash
- `HttpContext.Session` = access user session (like servlet sessions)
- `RedirectToAction()` = HTTP 302 redirect to another action

### Return Types

```csharp
return View(model);                          // 200 OK + rendered HTML
return View("CustomName", model);            // Specific view
return Redirect("/Path");                    // 302 redirect
return RedirectToAction("Action", "Controller");  // Redirect to action
return Json(data);                           // 200 OK + JSON
return BadRequest("Error message");          // 400 Bad Request
return NotFound();                           // 404 Not Found
return Forbid();                             // 403 Forbidden
return StatusCode(500, "Error");             // Custom status code
```

---

## 6. Views (Razor Templates)

### Razor Syntax

```html
@{
    ViewData["Title"] = "Login";
}

<h1>@Model.Title</h1>

@if (string.IsNullOrEmpty(Context.Session.GetString("UserID")))
{
    <p>Please log in</p>
}
else
{
    <p>Welcome, @Context.Session.GetString("UserName")</p>
}

<form asp-action="Login" asp-controller="Account" method="post">
    <input asp-for="UserID" class="form-control" />
    <span asp-validation-for="UserID" class="text-danger"></span>
    
    <button type="submit" class="btn btn-primary">Login</button>
</form>
```

**Razor syntax:**
- `@Model` = the data passed from controller (like Jinja2's `{{ }}` or ERB's `<%= %>`)
- `@{ }` = C# code blocks
- `@if`, `@foreach` = C# control flow embedded in HTML
- `asp-for="Property"` = tag helper (automatic two-way binding)
- `asp-action="Login"` = builds URL to that action (like Django's `{% url %}`)

### Tag Helpers

```html
<!-- Generates: <input type="text" id="UserID" name="UserID" /> -->
<input asp-for="UserID" />

<!-- Generates: <form action="/Account/Login" method="post"> -->
<form asp-action="Login" asp-controller="Account" method="post">

<!-- Generates: <a href="/Account/ChangePassword">Change Password</a> -->
<a asp-action="ChangePassword">Change Password</a>

<!-- Generates: <span class="text-danger">Error message</span> -->
<span asp-validation-for="UserID" class="text-danger"></span>
```

This is like Django's template tags (`{% url %}`, `{% csrf_token %}`) or Jinja2 filters.

---

## 7. Dependency Injection (IoC Container)

### Service Registration (Program.cs)

```csharp
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
```

Scopes:
- **AddTransient** = new instance every time (stateless services)
- **AddScoped** = one instance per HTTP request (stateful services like DbContext)
- **AddSingleton** = one instance for entire app lifetime (shared state)

Think of it like Spring's `@Scope` or Guice's scopes.

### Injection into Controller

```csharp
public class PaymentController : Controller
{
    private readonly IEmailService _emailService;
    
    public PaymentController(IEmailService emailService)
    {
        _emailService = emailService;  // Container injects the implementation
    }
    
    public async Task<IActionResult> Pay(PaymentViewModel model)
    {
        await _emailService.SendEmailAsync(...);
    }
}
```

The container sees the `IEmailService` parameter and injects `SmtpEmailService` (or `ConsoleEmailService` in development).

---

## 8. Authentication & Sessions

### Session-Based Auth (Traditional Approach)

```csharp
// After successful login
HttpContext.Session.SetString("UserID", user.ID);
HttpContext.Session.SetString("UserRole", "RE");
HttpContext.Session.SetString("UserName", user.ContactPersonName);

// Retrieve in another action
var userId = HttpContext.Session.GetString("UserID");
var role = HttpContext.Session.GetString("UserRole");

if (string.IsNullOrEmpty(userId))
    return RedirectToAction("Login");
```

This is like Java servlet sessions or Flask sessions. Session data is stored server-side (in this case, in-memory via `AddDistributedMemoryCache()`).

**Note:** .NET also supports cookie-based authentication with `[Authorize]` attributes and `ClaimsPrincipal`, but this app uses simple session state.

### Protection

```csharp
[ValidateAntiForgeryToken]  // Checks hidden __RequestVerificationToken
public async Task<IActionResult> Pay(PaymentViewModel model)
{
    // Form submission requires matching token
}
```

And in the form:
```html
<form asp-action="Pay" method="post">
    <!-- Razor automatically adds: <input name="__RequestVerificationToken" ... /> -->
    <input asp-for="CardNumber" />
    <button type="submit">Pay</button>
</form>
```

This prevents CSRF attacks. Similar to Django's `{% csrf_token %}` or Spring's CSRF token validation.

---

## 9. Email Service (Dependency Injection Pattern)

### Interface Definition

```csharp
public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body);
}
```

### Two Implementations

#### Development (Console Logging)
```csharp
public class ConsoleEmailService : IEmailService
{
    public Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body)
    {
        _logger.LogInformation("EMAIL TO: {toEmail}", toEmail);
        _logger.LogInformation("SUBJECT: {subject}", subject);
        _logger.LogInformation("BODY: {body}", body);
        return Task.FromResult(true);
    }
}
```

Returns a `Task<bool>` (like `Future<Boolean>` in Java or `Awaitable[bool]` in Python).

#### Production (Real SMTP)
```csharp
public class SmtpEmailService : IEmailService
{
    public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body)
    {
        using (var client = new SmtpClient(smtpServer, port))
        {
            client.Credentials = new NetworkCredential(username, password);
            using (var message = new MailMessage())
            {
                message.From = new MailAddress(fromEmail);
                message.To.Add(new MailAddress(toEmail, toName));
                message.Subject = subject;
                message.Body = body;
                
                await client.SendMailAsync(message);
                return true;
            }
        }
    }
}
```

`using` statements = auto-dispose resources when done (like try-with-resources in Java or context managers in Python).

### Usage in Controller

```csharp
public PaymentController(IEmailService emailService)
{
    _emailService = emailService;  // Injected - could be Console or SMTP version
}

public async Task<IActionResult> Pay(PaymentViewModel model)
{
    // Works regardless of implementation
    await _emailService.SendEmailAsync(
        "user@example.com",
        "John Smith",
        "Payment Confirmation",
        "<html>...</html>"
    );
}
```

This is the **Strategy pattern** - swap implementations without changing client code.

---

## 10. Async/Await Pattern

### What It Does

```csharp
public async Task<List<PermitRequest>> GetPermitsAsync(string userId)
{
    // This doesn't block the thread
    var permits = await _context.PermitRequests
        .Where(pr => pr.REID == userId)
        .ToListAsync();  // Database query happens asynchronously
    
    return permits;
}
```

**Without `async`:**
```csharp
public List<PermitRequest> GetPermits(string userId)
{
    // Thread blocks here until database returns
    var permits = _context.PermitRequests
        .Where(pr => pr.REID == userId)
        .ToList();  // BLOCKS THREAD
    
    return permits;
}
```

With `async`:
1. Thread starts the database query
2. Thread is released back to the thread pool
3. When database responds, continuation runs
4. Result is returned

This is essential for web apps - you can serve thousands of concurrent requests with just a handful of threads.

### In Controllers

```csharp
[HttpPost]
public async Task<IActionResult> Pay(PaymentViewModel model)
{
    var permitRequest = await _context.PermitRequests.FindAsync(model.PermitRequestNo);
    
    var payment = new Payment { ... };
    _context.Payments.Add(payment);
    await _context.SaveChangesAsync();  // Must await
    
    await _emailService.SendEmailAsync(...);  // Must await
    
    return RedirectToAction("Success");
}
```

Every `await` is a point where the thread can be released and reused.

---

## 11. Data Annotations (Validation)

### In Model

```csharp
public class LoginViewModel
{
    [Required(ErrorMessage = "User ID is required")]
    [StringLength(50)]
    public string UserID { get; set; }
    
    [Required]
    [DataType(DataType.Password)]
    [MinLength(6)]
    public string Password { get; set; }
}
```

### In Controller

```csharp
[HttpPost]
public IActionResult Login(LoginViewModel model)
{
    if (!ModelState.IsValid)  // Checks all annotations
    {
        // Re-render form with errors
        return View(model);
    }
    
    // Process valid data
}
```

### In View

```html
<form asp-action="Login" method="post">
    <input asp-for="UserID" class="form-control" />
    <span asp-validation-for="UserID" class="text-danger"></span>
    
    <button type="submit">Login</button>
</form>
```

This is like:
- **Spring:** `@Valid` + `@NotNull`, `@Size`, etc.
- **Django:** Form validation in `forms.py`
- **Express.js:** Libraries like `express-validator`

---

## 12. Configuration (appsettings.json)

### File Structure

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=Group5_iPERMITDB.db"
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "your-email@gmail.com",
    "Password": "app-password"
  }
}
```

### Reading Configuration in Code

```csharp
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    
    public SmtpEmailService(IConfiguration configuration)
    {
        _configuration = configuration;  // Injected
    }
    
    public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body)
    {
        var smtpServer = _configuration["Email:SmtpServer"];
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
        var username = _configuration["Email:Username"];
        var password = _configuration["Email:Password"];
        
        // Use these values
    }
}
```

This is like:
- **Spring:** `application.properties` + `@ConfigurationProperties`
- **Django:** `settings.py`
- **Node.js:** `dotenv` or environment variables

---

## 13. LINQ (Language Integrated Query)

### Query Syntax

```csharp
// SQL-like syntax
var permits = from pr in _context.PermitRequests
              where pr.REID == userId
              orderby pr.DateOfRequest descending
              select pr;

// Method chain syntax (more common)
var permits = _context.PermitRequests
    .Where(pr => pr.REID == userId)
    .OrderByDescending(pr => pr.DateOfRequest)
    .ToList();
```

This compiles to SQL at the database layer. The lambdas (`pr => pr.REID == userId`) are **expression trees** that get translated to SQL.

### Common Methods

```csharp
_context.PermitRequests
    .Where(pr => pr.Status == "Submitted")        // WHERE
    .OrderBy(pr => pr.DateOfRequest)              // ORDER BY
    .Skip(10)                                      // OFFSET
    .Take(20)                                      // LIMIT
    .Include(pr => pr.RequestedBy)                // JOIN
    .ThenInclude(re => re.Sites)                  // NESTED JOIN
    .FirstOrDefaultAsync();                        // LIMIT 1 + execute async

// Aggregation
var count = _context.PermitRequests.Count(pr => pr.Status == "Approved");
var total = _context.PermitRequests.Sum(pr => pr.PermitFee);
var average = _context.PermitRequests.Average(pr => pr.PermitFee);
```

This is like:
- **Hibernate:** HQL queries
- **SQLAlchemy:** SQLAlchemy ORM queries
- **Django ORM:** QuerySet methods
- **SQL directly:** Obviously more limited than LINQ's fluency

---

## 14. Request/Response Cycle Example

### Full Flow: Login to Dashboard

1. **User submits form**
   ```html
   POST /Account/Login HTTP/1.1
   Content-Type: application/x-www-form-urlencoded
   
   UserID=john123&Password=secret
   ```

2. **Router matches** → `AccountController.Login(LoginViewModel)`

3. **Controller executes**
   ```csharp
   [HttpPost]
   public async Task<IActionResult> Login(LoginViewModel model)
   {
       if (!ModelState.IsValid)  // Validates UserID & Password
           return View(model);
       
       var user = await _context.REs.FirstOrDefaultAsync(r => r.ID == model.UserID);
       
       if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
       {
           HttpContext.Session.SetString("UserID", user.ID);
           return RedirectToAction("Dashboard", "PermitRequest");
       }
       
       return View(model);
   }
   ```

4. **Database query executed**
   ```sql
   SELECT * FROM REs WHERE ID = 'john123'
   ```

5. **Password verification** (BCrypt comparison - doesn't hit database)

6. **Session set** (stored in-memory)

7. **Redirect response sent**
   ```
   HTTP/1.1 302 Found
   Location: /PermitRequest/Dashboard
   ```

8. **Browser follows redirect** → GET `/PermitRequest/Dashboard`

9. **PermitRequestController.Dashboard executes**
   ```csharp
   public async Task<IActionResult> Dashboard()
   {
       var userId = HttpContext.Session.GetString("UserID");  // Retrieved from session
       
       var permits = await _context.PermitRequests
           .Include(pr => pr.Statuses)
           .Where(pr => pr.REID == userId)
           .ToListAsync();
       
       return View(permits);
   }
   ```

10. **View rendered** with data
    ```html
    @foreach (var permit in Model)
    {
        <tr>
            <td>@permit.RequestNo</td>
            <td>$@permit.PermitFee</td>
        </tr>
    }
    ```

11. **HTML sent to browser**

---

## 15. Key Differences from Other Frameworks

| Concept | .NET | Java | Python | Node.js |
|---------|------|------|--------|---------|
| Framework | ASP.NET Core MVC | Spring MVC | Django/Flask | Express |
| Request Handler | Controller Action | Controller Method | View Function | Route Handler |
| Dependency Injection | `IServiceProvider` (built-in) | `@Autowired` + Spring | Depends on library | Not standard |
| ORM | Entity Framework Core | Hibernate | SQLAlchemy | Sequelize/TypeORM |
| Sessions | `HttpContext.Session` | `HttpSession` | Flask session / Django session | `express-session` |
| Views | Razor `.cshtml` | JSP/Thymeleaf | Django/Jinja2 templates | EJS/Handlebars |
| Async | `async/await` (first-class) | `CompletableFuture` (verbose) | `async/await` (Python 3.5+) | `async/await` (since ES2017) |
| Validation | Data Annotations | JSR-303/Hibernate Validator | Django Forms / Marshmallow | Manual or libraries |
| Config | `appsettings.json` | `application.properties` | `settings.py` | `.env` files |

---

## Summary: How It All Fits Together

```
HTTP Request
    ↓
[Program.cs] Router matches URL to Controller Action
    ↓
[DI Container] Injects dependencies (DbContext, Services)
    ↓
[Controller Action] Handles request (GET/POST)
    - Reads session / request data
    - Validates with data annotations
    - Queries database via Entity Framework (async)
    - Calls services (Email, etc.)
    ↓
[Return Result]
    - View + Model → Renders Razor template
    - Redirect → HTTP 302
    - Json → Serialized response
    ↓
[Razor View] 
    - Access Model data with @Model
    - Tag helpers generate HTML
    - Built-in validation error display
    ↓
HTTP Response (HTML/JSON/Redirect)
    ↓
Browser renders response
```

---

## Quick Reference

### Common Patterns

**Query with filtering:**
```csharp
var items = await _context.Items
    .Where(x => x.Status == "Active")
    .Include(x => x.RelatedData)
    .ToListAsync();
```

**Create & Save:**
```csharp
var item = new Item { Name = "Test" };
_context.Items.Add(item);
await _context.SaveChangesAsync();
```

**Update:**
```csharp
var item = await _context.Items.FindAsync(id);
item.Name = "Updated";
await _context.SaveChangesAsync();
```

**Delete:**
```csharp
var item = await _context.Items.FindAsync(id);
_context.Items.Remove(item);
await _context.SaveChangesAsync();
```

**Redirect after action:**
```csharp
return RedirectToAction("ActionName", "ControllerName", new { id = model.Id });
```

**Return data as JSON:**
```csharp
return Json(new { success = true, data = result });
```

---

That's the core of how .NET MVC works! The principles are similar to other frameworks, but .NET has some strengths (async/await is built-in, strong typing, entity relationships are cleaner).
