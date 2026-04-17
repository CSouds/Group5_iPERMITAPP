# Group5_iPERMITAPP

**CS4320/7320 - Software Engineering I | Spring 2026**  
**University of Missouri-Columbia**  
**Project 2: iPERMIT Implementation**
**Repository: https://github.com/CSouds/Group5_iPERMITAPP/tree/main**

---

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or later)
- A web browser (Chrome, Edge, Firefox)

## How to Run Published Version

1. Unzip the folder.
2. Run the Group5_iPERMITAPP.exe

## How to Run without Published Version

### Option 1: Command Line

1. Open a terminal/command prompt
2. Navigate to the project folder:
   ```
   cd Group5_iPERMITAPP
   ```
3. Restore NuGet packages:
   ```
   dotnet restore
   ```
4. Run the application:
   ```
   dotnet run
   ```
5. Open your browser and go to: **https://localhost:5001** or **http://localhost:5000**

### Option 2: Visual Studio

1. Open `Group5_iPERMITAPP.csproj` in Visual Studio 2022
2. Press **F5** or click the green "Run" button
3. The application will open in your default browser

### Option 3: Create Executable

```
dotnet publish -c Release -o ./publish
```
Then run: `./publish/Group5_iPERMITAPP.exe`

---

## Login Credentials

### Environmental Officer (EO)
- **User ID:** `EO001`
- **Password:** `password`
- *The EO can change their password after first login*

### Regulated Entity (RE)
- Register a new RE account through the **Register** page

## Email Configuration

**Important:** Emails are fully implemented!

- **Development Mode (default):** Emails log to console instead of sending
- **Production Mode:** Real emails sent via SMTP (Gmail, SendGrid, Office365, etc.)

See `EMAIL_SETUP.md` for complete configuration instructions. To test without SMTP setup, just run the app normally and watch the console for email output.

---

## Features Implemented

| Feature | Description |
|---------|-------------|
| RE Registration | Register with organization, contact, address, and site info |
| RE Login | Authenticate with user ID and password |
| RE Account Management | Update profile, add sites, change password |
| EO Account | Hard-coded EO001 account, login, password change |
| Manage Environmental Permits | Browse permit types, create applications |
| Submit Applications | Fill out permit request form with activity details |
| Fee Payment (OPS-CPP) | Simulated payment portal with card processing |
| **Email Notifications** | **Real SMTP emails sent at payment, decision, and permit issuance** |
| Acknowledge EO | Email notifications archived after payment |
| Review Applications | EO reviews submitted applications |
| Approve/Reject | EO makes decisions with recorded reasoning |
| Issue Permits | EO issues permits for approved requests |
| Update Status | Full status lifecycle tracking |
| Reports | Permit status report and email archive (EO only) |

---

## Application Lifecycle (State Chart)

1. **Pending Payment** - Application created, awaiting fee payment
2. **Submitted** - Payment confirmed via OPS-CPP, application submitted
3. **Being Reviewed** - EO is actively reviewing the application
4. **Approved** / **Rejected** - EO decision made
5. **Permit Issued** - Official permit issued (if approved)

---

## Technology Stack

- **Framework:** ASP.NET Core 8.0 MVC
- **Database:** SQLite (Group5_iPERMITDB.db - auto-created)
- **ORM:** Entity Framework Core 8.0
- **UI:** Bootstrap 5 + Bootstrap Icons
- **Authentication:** BCrypt password hashing with session-based auth
- **Architecture:** MVC (Model-View-Controller)

---

## Database

The SQLite database file (`Group5_iPERMITDB.db`) is automatically created in the project directory on first run. It is pre-seeded with:
- 1 EO account (ID: EO001, Password: password)
- 6 environmental permit types with fees

---

## Project Structure

```
Group5_iPERMITAPP/
├── Program.cs                    # Application entry point
├── Group5_iPERMITAPP.csproj     # Project configuration
├── appsettings.json             # App settings & DB connection
├── Models/                      # Entity classes (MVC Model)
│   ├── RE.cs                    # Regulated Entity
│   ├── RESite.cs                # RE Activity Sites
│   ├── EnvironmentalPermit.cs   # Available permit types
│   ├── PermitRequest.cs         # Permit applications
│   ├── EO.cs                    # Environmental Officer
│   ├── Decision.cs              # EO decisions
│   ├── RequestStatus.cs         # Status tracking
│   ├── Payment.cs               # Payment records
│   ├── Permit.cs                # Issued permits
│   ├── EmailArchive.cs          # Email log
│   └── ViewModels/              # Form view models
├── Controllers/                 # MVC Controllers
│   ├── HomeController.cs        # Landing page
│   ├── AccountController.cs     # Auth & account management
│   ├── PermitRequestController.cs # Permit applications
│   ├── PaymentController.cs     # OPS-CPP payment processing
│   ├── EOController.cs          # EO review & permit issuance
│   └── ReportController.cs      # EO-only reports
├── Views/                       # Razor Views (MVC View)
│   ├── Shared/_Layout.cshtml    # Master layout
│   ├── Home/                    # Landing pages
│   ├── Account/                 # Login, Register, Manage
│   ├── PermitRequest/           # Dashboard, Create, Details
│   ├── Payment/                 # Pay, Success
│   ├── EO/                      # Dashboard, Review, Issue
│   └── Report/                  # Status & Email reports
├── Data/                        # Database layer
│   ├── ApplicationDbContext.cs  # EF Core DbContext
│   └── DbInitializer.cs        # Seed data
└── wwwroot/                     # Static files
    └── css/site.css             # Custom styles
```
