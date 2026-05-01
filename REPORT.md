# Technical Report

# Hotel Luxe — Hotel Management and Reservation System

# Table of Contents

1. [System Architecture Overview](#1-system-architecture-overview)
2. [Design Patterns Used](#2-design-patterns-used)
3. [Database Layer — Models](#3-database-layer--models)
4. [ViewModels](#4-viewmodels)
5. [Data Layer — DbContext](#5-data-layer--dbcontext)
6. [Helper Classes](#6-helper-classes)
7. [Application Entry Point — Program.cs](#7-application-entry-point--programcs)
8. [Controllers](#8-controllers)
9. [Views](#9-views)
10. [Static Assets — CSS and JavaScript](#10-static-assets--css-and-javascript)
11. [Security Implementation](#11-security-implementation)
12. [Database Design and Relationships](#12-database-design-and-relationships)
13. [How Everything Connects — Request Lifecycle](#13-how-everything-connects--request-lifecycle)

---

# 1. System Architecture Overview

## What is ASP.NET Core MVC?

This project is built using **ASP.NET Core 8.0 MVC**, which stands for **Model-View-Controller**. MVC is a software architectural pattern that separates an application into three main components:

- **Model** — The data and business rules. In this project, models represent database tables (Guest, Room, Employee, etc.)
- **View** — The user interface. These are the HTML pages the user sees in their browser (.cshtml files)
- **Controller** — The middleman that receives requests from the browser, talks to the database through models, and returns a view (webpage) as a response

### Why MVC?

MVC is used because it separates concerns. The person writing HTML does not need to understand the database. The person writing C# business logic does not need to understand how the page looks. This makes the code organized, maintainable, and easy to extend.

## The Three-Tier Architecture

The system is organized in three tiers:

```
Browser (Client)
      ↕  HTTP Requests / Responses
Web Server (ASP.NET Core)
   ├── Controllers  ← receives requests, applies logic
   ├── Views        ← generates HTML responses
   └── Models       ← represents data
      ↕  SQL Queries via Entity Framework
SQL Server Database (LocalDB)
   ├── Guests table
   ├── Rooms table
   ├── Employees table
   ├── Accounts table
   ├── Reservations table
   └── Payments table
```

## Project File Structure Explained

```
HotelMS/
├── Controllers/     ← C# classes that handle browser requests
├── Data/            ← Entity Framework database context
├── Database/        ← SQL backup script
├── Helpers/         ← Utility classes (password hashing, session)
├── Models/          ← C# classes that map to database tables
│   └── ViewModels/  ← C# classes that carry data to views
├── Views/           ← HTML templates (.cshtml files)
├── wwwroot/         ← CSS, JavaScript, images (served directly)
├── appsettings.json ← Configuration (database connection string)
├── Program.cs       ← Application startup and configuration
└── HotelMS.csproj   ← Project definition and package references
```

---

# 2. Design Patterns Used

## Model-View-Controller (MVC)

As explained above, this separates data (Model), presentation (View), and logic (Controller).

## Repository Pattern (via Entity Framework)

Instead of writing raw SQL queries, we use **Entity Framework Core (EF Core)** as an Object-Relational Mapper (ORM). This means:

- We write C# code: `_context.Guests.ToListAsync()`
- EF Core translates this to SQL: `SELECT * FROM Guests`

This protects against SQL injection attacks and makes the code much more readable.

## Dependency Injection (DI)

Every controller receives its dependencies (like the database context) through its constructor. ASP.NET Core manages the creation of these objects automatically. This is called **Dependency Injection**.

```csharp
public class GuestController : Controller
{
    private readonly HotelDbContext _context;

    // ASP.NET Core automatically provides HotelDbContext here
    public GuestController(HotelDbContext context)
    {
        _context = context;
    }
}
```

Instead of the controller creating the database connection itself, ASP.NET Core creates it and passes it in. This makes the code testable and loosely coupled.

## Session-Based Authentication

Instead of using cookies with JWT tokens, this system uses **server-side sessions** to remember who is logged in. When a user logs in, their UserID, Username, and Role are stored in the session on the server. Every request checks the session to verify authentication.

---

# 3. Database Layer — Models

Models are C# classes that represent real-world entities. Each model maps to a database table via Entity Framework Core. The property names become column names.

## 3.1 Guest.cs

**File location:** `Models/Guest.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace HotelMS.Models
{
    public class Guest
    {
        public int GuestID { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required, MaxLength(100), EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
```

### Explanation Line by Line

**`namespace HotelMS.Models`**
A namespace is a way to organize code. All model classes belong to `HotelMS.Models` so they can be referenced using this name from other files.

**`public class Guest`**
This defines a class named `Guest`. In EF Core, each class becomes a database table named `Guests` (EF pluralizes by default).

**`public int GuestID { get; set; }`**
This is the primary key. EF Core recognizes properties ending in `ID` as primary keys automatically. In the database, this becomes an `INT IDENTITY(1,1)` column — a number that auto-increments starting from 1.

**`{ get; set; }`**
This is a C# auto-property. `get` means the value can be read, `set` means it can be written. Without `set`, the value would be read-only.

**`= string.Empty`**
This initializes the string to an empty string `""` instead of `null`. This avoids null reference errors because C# 8+ has nullable reference warnings enabled.

**`[Required]`**
This is a **Data Annotation attribute**. It tells the system two things:

1. In the database, this column cannot be NULL
2. When submitting a form, this field cannot be left empty (model validation)

**`[MaxLength(50)]`**
Sets the maximum character length of the database column to 50. Without this, EF creates a `nvarchar(max)` column. With it, it creates `nvarchar(50)`.

**`[EmailAddress]`**
Validates that the value looks like a valid email address (contains `@` and a domain). This runs during form submission validation.

**`public string FullName => $"{FirstName} {LastName}";`**
This is a **computed property** (expression-bodied property). It has no `set` accessor, so it is read-only. It combines FirstName and LastName with a space. The `$` before the string makes it an **interpolated string** — `{FirstName}` is replaced with the actual value. This property is NOT stored in the database (no column is created for it).

**`public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();`**
This is a **navigation property**. It represents the relationship: one Guest can have many Reservations. EF Core uses this to know that the `Reservations` table has a foreign key pointing to `Guests`. When you call `.Include(g => g.Reservations)` in a query, EF loads the related reservations from the database automatically. `ICollection<T>` is an interface for a collection that can be iterated, counted, and added to.

---

## 3.2 Room.cs

**File location:** `Models/Room.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelMS.Models
{
    public class Room
    {
        public int RoomID { get; set; }

        [Required, MaxLength(10)]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        public string RoomType { get; set; } = "Single";

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public string Status { get; set; } = "Available";

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
```

### Explanation

**`using System.ComponentModel.DataAnnotations.Schema`**
This imports the `Schema` namespace which contains the `[Column]` attribute.

**`public string RoomType { get; set; } = "Single";`**
The default value is `"Single"`. This means if you create a Room object without setting RoomType, it will be "Single" by default. Valid values in this system are: `"Single"`, `"Double"`, `"Suite"`.

**`[Column(TypeName = "decimal(10,2)")]`**
This tells EF Core to create a `DECIMAL(10, 2)` column in SQL Server. This means the number can have up to 10 digits total, with 2 of them after the decimal point. For example: `99999999.99`. Without this attribute, EF might create a column with different precision, which can cause rounding errors with money values.

**`public decimal Price { get; set; }`**
`decimal` is the C# data type for precise monetary values. Unlike `float` or `double`, `decimal` does not have floating-point rounding errors, making it suitable for prices and money calculations.

**`public string Status { get; set; } = "Available";`**
Possible values: `"Available"`, `"Booked"`, `"Maintenance"`. When a room is reserved, the controller changes this to `"Booked"`. When a guest checks out, it changes back to `"Available"`.

**`public ICollection<Reservation> Reservations { get; set; }`**
Navigation property for the one-to-many relationship. One Room can appear in many Reservations (across different time periods).

---

## 3.3 Employee.cs

**File location:** `Models/Employee.cs`

```csharp
public class Employee
{
    public int EmployeeID { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Position { get; set; } = "Staff";

    [Required, Column(TypeName = "decimal(10,2)")]
    public decimal Salary { get; set; }

    [Required, MaxLength(10)]
    public string Gender { get; set; } = string.Empty;

    [Required]
    public int Age { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public Account? Account { get; set; }
}
```

### Explanation

**`public string Position { get; set; } = "Staff";`**
Valid values: `"Manager"`, `"Receptionist"`, `"Staff"`. This determines what the employee is allowed to do in the hotel.

**`public int Age { get; set; }`**
An `int` (integer) stores whole numbers. Age is appropriate as an integer since ages are not fractional.

**`public ICollection<Reservation> Reservations { get; set; }`**
Every reservation is handled by one employee (the receptionist who created it). This navigation property gives access to all reservations an employee has handled.

**`public Account? Account { get; set; }`**
The `?` after `Account` means this is **nullable** — an employee may or may not have a system login account. For example, a cleaning staff member may not need to log in. The `Account?` means the reference can be `null` without causing a compile error. This represents a one-to-one relationship between Employee and Account.

---

## 3.4 Reservation.cs

**File location:** `Models/Reservation.cs`

```csharp
public class Reservation
{
    public int ReservationID { get; set; }

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    [Required]
    public int GuestID { get; set; }

    [Required]
    public int RoomID { get; set; }

    [Required]
    public int EmployeeID { get; set; }

    public Guest Guest { get; set; } = null!;
    public Room Room { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public int TotalNights => (CheckOutDate - CheckInDate).Days;
}
```

### Explanation

**`public DateTime CheckInDate { get; set; }`**
`DateTime` is a C# type that stores both a date and time. For check-in and check-out, we care about the date portion. `DateTime.Today` gives today's date with time set to midnight.

**`public int GuestID { get; set; }`**
This is the **foreign key** — it stores the ID of the guest who made this reservation. In the database, this becomes an `INT` column with a `FOREIGN KEY` constraint pointing to `Guests.GuestID`.

**`public int RoomID { get; set; }` and `public int EmployeeID { get; set; }`**
Similarly, these are foreign keys pointing to the Rooms and Employees tables. A reservation ties together one Guest, one Room, and one Employee.

**`public Guest Guest { get; set; } = null!;`**
This is the **navigation property** for the foreign key `GuestID`. The `null!` is a C# null-forgiving operator — it tells the compiler "I know this might look like it's null but trust me, EF Core will populate it when needed via `.Include()`". Without `= null!`, the compiler would warn about an uninitialized non-nullable reference.

**`public int TotalNights => (CheckOutDate - CheckInDate).Days;`**
This is a computed property. Subtracting two `DateTime` values gives a `TimeSpan` object. `.Days` extracts the whole number of days from that `TimeSpan`. For example: checking out on the 5th and checking in on the 3rd = 2 nights. This property is not stored in the database.

---

## 3.5 Payment.cs

**File location:** `Models/Payment.cs`

```csharp
public class Payment
{
    public int PaymentID { get; set; }

    [Required, Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = "Cash";

    [Required]
    public int ReservationID { get; set; }

    public Reservation Reservation { get; set; } = null!;
}
```

### Explanation

**`public string PaymentMethod { get; set; } = "Cash";`**
Valid values: `"Cash"`, `"CreditCard"`, `"DebitCard"`, `"Online"`. A reservation can have multiple partial payments (for example, a deposit and then the final payment).

**`public int ReservationID { get; set; }`**
Foreign key pointing to the Reservations table. This links each payment to its reservation.

**`public DateTime PaymentDate { get; set; }`**
Stores when the payment was made. In the controller, this is automatically set to `DateTime.Now` (current date and time) at the moment of payment processing. The user does not enter this manually.

---

## 3.6 Account.cs

**File location:** `Models/Account.cs`

```csharp
public class Account
{
    public int UserID { get; set; }

    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Staff";

    public int? EmployeeID { get; set; }

    public Employee? Employee { get; set; }
}
```

### Explanation

**`public string Password { get; set; } = string.Empty;`**
This stores the **hashed** password, never the plain text password. The `MaxLength(255)` is set to 255 because our SHA-256 hash output is 64 hex characters, plus a 32-character salt, plus the colon separator = about 97 characters. 255 gives plenty of room.

**`public string Role { get; set; } = "Staff";`**
Valid values: `"Admin"`, `"Manager"`, `"Receptionist"`, `"Staff"`. The role controls what pages and actions the user is allowed to access.

**`public int? EmployeeID { get; set; }`**
The `?` makes this an **nullable integer**. In C#, value types like `int` cannot normally be null. Adding `?` makes it `Nullable<int>`, which can hold either an integer value or `null`. The admin account created at startup has `EmployeeID = null` because there may not be a corresponding employee record.

**`public Employee? Employee { get; set; }`**
Navigation property for the one-to-one relationship. An account belongs to one employee (or no employee in the case of the admin account).

---

# 4. ViewModels

ViewModels are special classes that are NOT mapped to database tables. They exist purely to carry data between a controller and a view. They often combine data from multiple models or add validation rules specific to a form.

## 4.1 LoginViewModel.cs

```csharp
public class LoginViewModel
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
```

### Why use a ViewModel for Login?

We do not pass an `Account` model to the login view because the `Account` model has a `Role` field and other properties that the login form does not need. The ViewModel contains exactly what the login form needs: a username and a password.

**`[Required(ErrorMessage = "Username is required")]`**
The `ErrorMessage` parameter customizes the error message shown when validation fails. Without it, the default message would be "The Username field is required."

**`[DataType(DataType.Password)]`**
This tells Razor (the HTML template engine) to render this field as `<input type="password">` in the HTML, which shows dots instead of characters. Without this, the tag helper would render `type="text"`.

---

## 4.2 ReservationViewModel.cs

```csharp
public class ReservationViewModel
{
    public int ReservationID { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public string GuestFullName { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public decimal RoomPrice { get; set; }
    public string EmployeeName { get; set; } = string.Empty;

    public int TotalNights => Math.Max((CheckOutDate - CheckInDate).Days, 0);
    public decimal TotalAmount => TotalNights * RoomPrice;

    public string Status
    {
        get
        {
            var today = DateTime.Today;
            if (today < CheckInDate) return "Upcoming";
            if (today <= CheckOutDate) return "Active";
            return "Completed";
        }
    }
}
```

### Why use a ViewModel for Reservations?

The `Reservation` model only stores IDs (GuestID, RoomID, EmployeeID). The view needs to display names (Guest's full name, room number, employee name). The controller queries the database with `.Include()` to load related data, then maps it into this ViewModel. This separation keeps views clean — they receive exactly the data they need, pre-formatted.

**`public int TotalNights => Math.Max((CheckOutDate - CheckInDate).Days, 0);`**
`Math.Max(x, 0)` ensures the result is never negative. If someone enters a check-out date before check-in (which the controller should prevent, but this handles edge cases), the nights displayed would be 0 instead of a negative number.

**`public decimal TotalAmount => TotalNights * RoomPrice;`**
This computed property automatically calculates the total charge. Since both `TotalNights` and `RoomPrice` are properties of the same ViewModel, this always stays in sync.

**`public string Status { get { ... } }`**
This property with a `get` block (called a **getter**) determines the reservation status dynamically by comparing today's date against the reservation dates:

- If today is before check-in → "Upcoming"
- If today is between check-in and check-out (inclusive) → "Active"
- If today is after check-out → "Completed"

---

## 4.3 DashboardViewModel.cs

```csharp
public class MonthlyRevenueData
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}

public class DashboardViewModel
{
    public int TotalGuests { get; set; }
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
    public int TotalReservations { get; set; }
    public int ActiveReservations { get; set; }
    public decimal TotalRevenue { get; set; }
    public double OccupancyRate { get; set; }
    public List<ReservationViewModel> RecentReservations { get; set; } = new();
    public List<MonthlyRevenueData> MonthlyRevenue { get; set; } = new();
    public Dictionary<string, int> RoomTypeBreakdown { get; set; } = new();
    public int TotalEmployees { get; set; }
    public int TodayCheckIns { get; set; }
    public int TodayCheckOuts { get; set; }
}
```

### Explanation

**`public List<ReservationViewModel> RecentReservations { get; set; } = new();`**
`List<T>` is a generic collection that can grow and shrink. The `= new()` at the end initializes an empty list (so it is never null). This holds the 5 most recent reservations to display on the dashboard.

**`public Dictionary<string, int> RoomTypeBreakdown { get; set; } = new();`**
A `Dictionary<TKey, TValue>` stores key-value pairs. Here `string` keys (like "Single", "Double", "Suite") map to `int` values (room counts). This is used to show a breakdown of room types.

**`public double OccupancyRate { get; set; }`**
`double` is used here (instead of `decimal`) because this is a percentage for display purposes, and floating-point precision is acceptable for percentages.

**`public List<MonthlyRevenueData> MonthlyRevenue { get; set; } = new();`**
This holds up to 6 months of revenue data for the bar chart on the dashboard. `MonthlyRevenueData` is a small helper class defined in the same file.

---

## 4.4 BillViewModel.cs

```csharp
public class BillViewModel
{
    public string GuestName { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int TotalNights => Math.Max((CheckOutDate - CheckInDate).Days, 0);
    public decimal SubTotal => TotalNights * PricePerNight;
    public List<Payment> Payments { get; set; } = new();
    public decimal TotalPaid => Payments.Sum(p => p.Amount);
    public decimal BalanceDue => SubTotal - TotalPaid;
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}
```

### Explanation

**`public decimal TotalPaid => Payments.Sum(p => p.Amount);`**
`Payments.Sum(p => p.Amount)` is a **LINQ** (Language Integrated Query) expression. `p => p.Amount` is a **lambda expression** — it means "for each payment p, take its Amount". `.Sum()` adds all those amounts together. This calculates the total of all payments made against a reservation.

**`public decimal BalanceDue => SubTotal - TotalPaid;`**
This automatically stays correct because `SubTotal` and `TotalPaid` are themselves computed. If a new payment is added and `Payments` list is updated, `BalanceDue` recalculates automatically.

**`public DateTime GeneratedAt { get; set; } = DateTime.Now;`**
This initializes the property to the current date and time at the moment the object is created. This is used to timestamp the generated bill.

---

# 5. Data Layer — DbContext

**File location:** `Data/HotelDbContext.cs`

The `HotelDbContext` is the central class that connects the C# application to the SQL Server database. It is provided by Entity Framework Core.

```csharp
public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

    public DbSet<Guest> Guests { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Account> Accounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Room>()
            .HasIndex(r => r.RoomNumber)
            .IsUnique();

        modelBuilder.Entity<Account>()
            .HasIndex(a => a.Username)
            .IsUnique();

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Guest)
            .WithMany(g => g.Reservations)
            .HasForeignKey(r => r.GuestID)
            .OnDelete(DeleteBehavior.Restrict);
        // ... (other relationships)
    }
}
```

## Explanation

**`public class HotelDbContext : DbContext`**
The colon `:` means `HotelDbContext` **inherits** from `DbContext`. This means our class gets all the database functionality that EF Core provides, and we add our specific tables on top.

**`public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }`**
This is the **constructor**. It receives configuration options (like the database connection string) and passes them to the parent class (`DbContext`) using `: base(options)`. The options are configured in `Program.cs`.

**`public DbSet<Guest> Guests { get; set; }`**
A `DbSet<T>` represents a database table. `DbSet<Guest>` represents the `Guests` table. You can query it with LINQ:

- `_context.Guests.ToList()` → `SELECT * FROM Guests`
- `_context.Guests.Where(g => g.Email == "x")` → `SELECT * FROM Guests WHERE Email = 'x'`
- `_context.Guests.Find(5)` → `SELECT * FROM Guests WHERE GuestID = 5`

**`protected override void OnModelCreating(ModelBuilder modelBuilder)`**
This method is called by EF Core when it is building the database schema. We override it to add extra configuration that cannot be expressed through attributes alone.

**`modelBuilder.Entity<Room>().HasIndex(r => r.RoomNumber).IsUnique();`**
This creates a **unique index** on the `RoomNumber` column in the `Rooms` table. This means the database will reject any attempt to insert two rooms with the same room number. This enforces a business rule at the database level.

**`.HasOne(r => r.Guest).WithMany(g => g.Reservations).HasForeignKey(r => r.GuestID).OnDelete(DeleteBehavior.Restrict)`**

This defines the relationship between Reservation and Guest:

- `.HasOne(r => r.Guest)` — A reservation has ONE guest
- `.WithMany(g => g.Reservations)` — A guest has MANY reservations
- `.HasForeignKey(r => r.GuestID)` — The foreign key is `GuestID` in the Reservations table
- `.OnDelete(DeleteBehavior.Restrict)` — If someone tries to delete a Guest who has reservations, the database will **refuse** and throw an error. This protects data integrity. Without this, EF might default to cascade delete (deleting all reservations if a guest is deleted), which would be destructive.

**`DeleteBehavior.Cascade` on Payments:**

```csharp
modelBuilder.Entity<Payment>()
    .HasOne(p => p.Reservation)
    .WithMany(r => r.Payments)
    .OnDelete(DeleteBehavior.Cascade);
```

For payments, we DO want cascade delete — if a reservation is cancelled (deleted), all its payment records should also be deleted because they are meaningless without the reservation.

---

# 6. Helper Classes

## 6.1 AuthHelper.cs

**File location:** `Helpers/AuthHelper.cs`

This class handles password security and authentication checks. It uses the `System.Security.Cryptography` namespace which is built into .NET — no extra packages needed.

```csharp
public static class AuthHelper
{
    public static string HashPassword(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var salt = Convert.ToHexString(saltBytes);
        var combined = Encoding.UTF8.GetBytes(salt + password);
        using var sha = SHA256.Create();
        var hash = Convert.ToHexString(sha.ComputeHash(combined));
        return $"{salt}:{hash}";
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2) return false;
        var salt = parts[0];
        var expectedHash = parts[1];
        var combined = Encoding.UTF8.GetBytes(salt + password);
        using var sha = SHA256.Create();
        var hash = Convert.ToHexString(sha.ComputeHash(combined));
        return hash == expectedHash;
    }

    public static bool IsAuthenticated(ISession session)
    {
        return session.GetString("UserID") != null;
    }

    public static bool HasRole(ISession session, params string[] roles)
    {
        var role = session.GetString("UserRole");
        return role != null && roles.Contains(role);
    }
}
```

### How Password Hashing Works

**Why we never store plain text passwords:**
If the database is ever stolen, the attacker should not be able to read passwords. Even if they have the database, hashed passwords are useless to them.

**Step 1 — Generate a random salt:**

```csharp
var saltBytes = RandomNumberGenerator.GetBytes(16);
var salt = Convert.ToHexString(saltBytes);
```

A **salt** is a random string added to the password before hashing. This ensures that two users with the same password get different hashes. `RandomNumberGenerator` is cryptographically secure — it generates truly random bytes. `GetBytes(16)` generates 16 random bytes. `Convert.ToHexString()` converts those bytes to a hexadecimal string (32 characters).

**Step 2 — Combine salt with password:**

```csharp
var combined = Encoding.UTF8.GetBytes(salt + password);
```

We concatenate the salt with the password, then convert the combined string to bytes using UTF-8 encoding. SHA-256 operates on bytes, not strings.

**Step 3 — Hash with SHA-256:**

```csharp
using var sha = SHA256.Create();
var hash = Convert.ToHexString(sha.ComputeHash(combined));
```

`SHA256.Create()` creates a SHA-256 hasher. `.ComputeHash(combined)` runs the SHA-256 algorithm on our combined bytes and returns 32 bytes (256 bits). `Convert.ToHexString()` converts those 32 bytes to a 64-character hexadecimal string.

The `using` keyword ensures `sha` is properly disposed of when done (releases memory/resources).

**Step 4 — Store as "salt:hash":**

```csharp
return $"{salt}:{hash}";
```

We store the salt alongside the hash, separated by `:`. The salt is not secret — it does not need to be. Its purpose is just to make each hash unique.

**Example stored value:**
`A3F29B1C...(32 chars)...:9E4D7A2B...(64 chars)...`

**Verification process:**

```csharp
var parts = storedHash.Split(':');
var salt = parts[0];  // extract the original salt
```

When verifying, we split the stored string on `:` to recover the salt, then hash the submitted password with the same salt and compare. If the hashes match, the password is correct.

### `public static bool IsAuthenticated(ISession session)`

```csharp
return session.GetString("UserID") != null;
```

If `"UserID"` exists in the session, the user is logged in. If not, they need to log in first. This is the simplest possible authentication check.

### `public static bool HasRole(ISession session, params string[] roles)`

```csharp
var role = session.GetString("UserRole");
return role != null && roles.Contains(role);
```

`params string[] roles` means this method can be called with any number of role arguments:

- `HasRole(session, "Admin")` — checks if the user is an Admin
- `HasRole(session, "Admin", "Manager")` — checks if the user is either Admin or Manager

`.Contains(role)` checks if the user's role is in the list of allowed roles.

---

## 6.2 SessionHelper.cs

**File location:** `Helpers/SessionHelper.cs`

```csharp
public static class SessionHelper
{
    public static void SetUserID(ISession session, int id) =>
        session.SetString("UserID", id.ToString());

    public static int? GetUserID(ISession session)
    {
        var val = session.GetString("UserID");
        return val != null ? int.Parse(val) : null;
    }

    public static void SetUsername(ISession session, string name) =>
        session.SetString("Username", name);

    public static string? GetUsername(ISession session) =>
        session.GetString("Username");

    public static void SetUserRole(ISession session, string role) =>
        session.SetString("UserRole", role);

    public static string? GetUserRole(ISession session) =>
        session.GetString("UserRole");

    public static void ClearSession(ISession session) =>
        session.Clear();
}
```

### How ASP.NET Core Sessions Work

A **session** is server-side storage tied to a specific browser. When a user visits the site, ASP.NET Core creates a unique session ID and stores it in a browser cookie. On each subsequent request, the browser sends that cookie, and the server retrieves the associated session data.

ASP.NET Core sessions can only store `string` and `byte[]` values. That is why we convert the integer `UserID` to a string with `.ToString()` when storing it, and parse it back with `int.Parse(val)` when reading.

**`public static void SetUserID(ISession session, int id) => session.SetString("UserID", id.ToString());`**
The `=>` is the **expression body** — a shorthand for a method that is just one statement. This stores the UserID in the session under the key `"UserID"`.

**`return val != null ? int.Parse(val) : null;`**
This is a **ternary operator** — a shorthand for if/else. It reads: "if val is not null, return `int.Parse(val)`, otherwise return null". The return type `int?` (nullable int) allows returning `null`.

**`public static void ClearSession(ISession session) => session.Clear();`**
`session.Clear()` removes all stored values from the session. This is called on logout, effectively "forgetting" who the user is.

---

# 7. Application Entry Point — Program.cs

**File location:** `Program.cs`

`Program.cs` is the first file that runs when the application starts. It configures everything the application needs and then launches the web server.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".HotelMS.Session";
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed admin account on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    db.Database.Migrate();
    if (!db.Accounts.Any(a => a.Username == "admin"))
    {
        db.Accounts.Add(new Account { ... });
        db.SaveChanges();
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
```

## Explanation Section by Section

**`var builder = WebApplication.CreateBuilder(args);`**
Creates a **builder** object that is used to configure everything before the application starts. `args` are command-line arguments passed when running `dotnet run`.

**`builder.Services.AddDbContext<HotelDbContext>(...)`**
Registers `HotelDbContext` with the **Dependency Injection (DI) container**. This means whenever a controller asks for a `HotelDbContext` in its constructor, ASP.NET Core automatically creates one and provides it. The `.UseSqlServer(...)` tells EF Core to use SQL Server as the database.

**`builder.Configuration.GetConnectionString("DefaultConnection")`**
Reads the connection string from `appsettings.json` under the key `"DefaultConnection"`. This is: `Server=(localdb)\mssqllocaldb;Database=HotelManagementDB;...`

**`builder.Services.AddDistributedMemoryCache()`**
Sessions need a cache to store their data. `DistributedMemoryCache` stores session data in the server's memory. In production, you would use Redis or SQL Server as a distributed cache, but in-memory is fine for development.

**`builder.Services.AddSession(options => { ... })`**
Registers the session service with configuration:

- `options.IdleTimeout = TimeSpan.FromMinutes(30)` — The session expires after 30 minutes of inactivity
- `options.Cookie.HttpOnly = true` — The session cookie cannot be accessed by JavaScript (prevents XSS attacks from stealing the session)
- `options.Cookie.IsEssential = true` — The cookie is considered essential (not subject to GDPR consent requirements for functionality)
- `options.Cookie.Name = ".HotelMS.Session"` — Custom name for the session cookie

**`var app = builder.Build();`**
This finalizes the configuration and creates the actual `WebApplication` object that will handle requests.

**The seeding block:**

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    db.Database.Migrate();
    if (!db.Accounts.Any(a => a.Username == "admin"))
    {
        db.Accounts.Add(new Account { ... });
        db.SaveChanges();
    }
}
```

- `app.Services.CreateScope()` creates a temporary DI scope (needed to get scoped services like `HotelDbContext` outside of a request)
- `db.Database.Migrate()` automatically applies any pending EF Core migrations when the app starts — this creates the database and tables if they do not exist
- `db.Accounts.Any(a => a.Username == "admin")` — checks if an admin account already exists
- `db.Accounts.Add(...)` then `db.SaveChanges()` — adds the admin account if it does not exist

**Middleware pipeline order:**

```csharp
app.UseHttpsRedirection();  // Redirect HTTP to HTTPS
app.UseStaticFiles();        // Serve CSS, JS, images from wwwroot
app.UseRouting();            // Match URLs to controllers
app.UseSession();            // Enable session (MUST come before controllers)
app.UseAuthorization();      // Apply authorization rules
```

The order of middleware matters. `UseSession()` MUST come before the controllers handle requests, because controllers need to read the session. `UseStaticFiles()` is early because static files do not need routing or session — they are served directly.

**`app.MapControllerRoute(name: "default", pattern: "{controller=Account}/{action=Login}/{id?}")`**
This defines the URL routing pattern:

- `{controller=Account}` — If no controller is specified, default to `AccountController`
- `{action=Login}` — If no action is specified, default to the `Login` method
- `{id?}` — Optional parameter

So when someone visits `https://localhost:PORT/`, they are routed to `AccountController.Login()`, which shows the login page. When visiting `https://localhost:PORT/Guest/AddGuest`, they are routed to `GuestController.AddGuest()`.

---

# 8. Controllers

Controllers are C# classes that receive HTTP requests, apply business logic, talk to the database, and return responses (usually HTML views). Every public method in a controller is called an **action method**.

## 8.1 AccountController.cs

### The CheckAuth Pattern

Every controller has a private helper method:

```csharp
private IActionResult? CheckAuth(params string[] roles)
{
    if (!AuthHelper.IsAuthenticated(HttpContext.Session))
        return RedirectToAction("Login");
    if (roles.Length > 0 && !AuthHelper.HasRole(HttpContext.Session, roles))
        return RedirectToAction("Index", "Dashboard");
    return null;
}
```

This method:

1. Checks if the user is logged in. If not → redirect to Login page
2. If roles are specified, checks if the user has the right role. If not → redirect to Dashboard
3. If everything is fine → return `null` (meaning "no problem, proceed")

Every protected action method starts with:

```csharp
var auth = CheckAuth("Admin", "Manager");
if (auth != null) return auth;
```

If `auth` is not null, it means CheckAuth wants to redirect somewhere, so we return that redirect immediately and stop executing the rest of the method.

### Login — GET

```csharp
[HttpGet]
public IActionResult Login()
{
    if (AuthHelper.IsAuthenticated(HttpContext.Session))
        return RedirectToAction("Index", "Dashboard");
    return View();
}
```

**`[HttpGet]`** — This attribute restricts this method to only respond to HTTP GET requests (when you type a URL in the browser or click a link).

**`return RedirectToAction("Index", "Dashboard")`** — If already logged in, redirect to Dashboard. This prevents logged-in users from seeing the login page again.

**`return View()`** — Returns the `Login.cshtml` view file (ASP.NET MVC automatically finds `Views/Account/Login.cshtml` by convention).

### Login — POST

```csharp
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Login(LoginViewModel model)
{
    if (!ModelState.IsValid) return View(model);

    var account = await _context.Accounts
        .FirstOrDefaultAsync(a => a.Username == model.Username);

    if (account == null || !AuthHelper.VerifyPassword(model.Password, account.Password))
    {
        ViewBag.Error = "Invalid username or password.";
        return View(model);
    }

    SessionHelper.SetUserID(HttpContext.Session, account.UserID);
    SessionHelper.SetUsername(HttpContext.Session, account.Username);
    SessionHelper.SetUserRole(HttpContext.Session, account.Role);

    if (account.Role == "Staff")
        return RedirectToAction("ViewRooms", "Room");

    return RedirectToAction("Index", "Dashboard");
}
```

**`[HttpPost]`** — Only responds to HTTP POST requests (form submissions).

**`[ValidateAntiForgeryToken]`** — Validates a hidden token embedded in the form. This prevents **Cross-Site Request Forgery (CSRF)** attacks, where a malicious website tricks a logged-in user into submitting a form on your site without their knowledge.

**`async Task<IActionResult>`** — This method is **asynchronous**. It uses `async`/`await` so the server thread is freed while waiting for the database to respond, allowing other requests to be handled. `Task<IActionResult>` is the async version of `IActionResult`.

**`if (!ModelState.IsValid) return View(model);`**
`ModelState` automatically validates the model based on Data Annotations (`[Required]`, `[EmailAddress]`, etc.). If validation fails (e.g., empty username), `IsValid` is `false` and we return the view with the model so the user sees their filled-in form with error messages.

**`await _context.Accounts.FirstOrDefaultAsync(a => a.Username == model.Username)`**
This is an async database query using LINQ. `FirstOrDefaultAsync` returns the first matching record or `null` if none found. The `await` keyword pauses this method until the database responds, but without blocking the server thread.

**`ViewBag.Error = "Invalid username or password.";`**
`ViewBag` is a dynamic property bag that passes data from the controller to the view. `ViewBag.Error` is not a built-in property — we are defining it dynamically. In the view, we read it with `@ViewBag.Error`.

**The session storage after successful login:**

```csharp
SessionHelper.SetUserID(HttpContext.Session, account.UserID);
SessionHelper.SetUsername(HttpContext.Session, account.Username);
SessionHelper.SetUserRole(HttpContext.Session, account.Role);
```

This stores three pieces of information in the session. These are read throughout the application to know who is logged in and what they are allowed to do.

---

## 8.2 DashboardController.cs

```csharp
public async Task<IActionResult> Index()
{
    var auth = CheckAuth();
    if (auth != null) return auth;

    var today = DateTime.Today;
    var sixMonthsAgo = today.AddMonths(-6);

    var reservations = await _context.Reservations
        .Include(r => r.Guest)
        .Include(r => r.Room)
        .Include(r => r.Employee)
        .Include(r => r.Payments)
        .ToListAsync();

    var payments = await _context.Payments.ToListAsync();

    var monthly = payments
        .Where(p => p.PaymentDate >= sixMonthsAgo)
        .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
        .Select(g => new MonthlyRevenueData
        {
            Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
            Revenue = g.Sum(p => p.Amount)
        })
        .OrderBy(m => m.Month)
        .ToList();
    // ... builds DashboardViewModel
}
```

### Explanation of Complex LINQ Queries

**`.Include(r => r.Guest)`**
This is EF Core's **eager loading**. Without `.Include()`, loading reservations would only give you `GuestID` numbers, not the guest's actual name. `.Include(r => r.Guest)` tells EF to also fetch the related Guest record with a SQL JOIN.

**The monthly revenue LINQ chain:**

```csharp
payments
  .Where(p => p.PaymentDate >= sixMonthsAgo)        // Filter last 6 months
  .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })  // Group by year+month
  .Select(g => new MonthlyRevenueData { ... })       // Transform each group
  .OrderBy(m => m.Month)                             // Sort by month
  .ToList()                                          // Execute and return List
```

- `.Where(...)` — filters the data (like SQL `WHERE`)
- `.GroupBy(p => new { ... })` — groups payments by year and month together. `new { Year, Month }` creates an anonymous type with two properties
- `.Select(g => new MonthlyRevenueData { ... })` — transforms (projects) each group into a `MonthlyRevenueData` object
- `g.Sum(p => p.Amount)` — sums up all payment amounts within each month group
- `.OrderBy(m => m.Month)` — sorts the results alphabetically by month string

**`Math.Round((double)rooms.Count(r => r.Status == "Booked") / rooms.Count * 100, 1)`**
Calculates occupancy rate:

1. `rooms.Count(r => r.Status == "Booked")` — count booked rooms
2. Divide by total rooms
3. Multiply by 100 to get a percentage
4. `Math.Round(..., 1)` rounds to 1 decimal place
5. `(double)` casts the integer to a double before division (otherwise integer division would give 0 for anything less than 1)

---

## 8.3 ReservationController.cs

### Room Availability Check

```csharp
bool unavailable = await _context.Reservations.AnyAsync(r =>
    r.RoomID == model.RoomID &&
    r.CheckInDate < model.CheckOutDate &&
    r.CheckOutDate > model.CheckInDate);
```

This is the **date overlap algorithm**. Two date ranges overlap if:

- One range's start is before the other's end, AND
- One range's end is after the other's start

In plain English: "Does any existing reservation for this room have dates that overlap with the requested dates?"

Example: Room 101 is booked from May 5 to May 10.

- New request: May 8 to May 12 → `CheckInDate (May 5) < May 12 (newCheckOut)` AND `CheckOutDate (May 10) > May 8 (newCheckIn)` → OVERLAP → room is unavailable
- New request: May 10 to May 14 → `May 5 < May 14` AND `May 10 > May 10` → `10 > 10` is FALSE → no overlap → room is available

**`AnyAsync()`** returns `true` if at least one record matches the condition. We do not need to retrieve all matching records — just knowing one exists is enough.

### CreateReservation — POST

```csharp
var room = await _context.Rooms.FindAsync(model.RoomID);
if (room != null) room.Status = "Booked";

_context.Reservations.Add(model);
await _context.SaveChangesAsync();
```

**`_context.Rooms.FindAsync(model.RoomID)`** — Finds a room by its primary key. `.FindAsync()` first checks EF's local cache before hitting the database.

**`room.Status = "Booked"`** — We modify the room's status. Since the `room` object was retrieved from `_context`, EF is tracking it. When we call `SaveChangesAsync()`, EF knows the room was modified and generates an `UPDATE` statement.

**`_context.Reservations.Add(model)`** — Marks the new reservation to be inserted. EF generates an `INSERT` statement.

**`await _context.SaveChangesAsync()`** — Commits all pending changes (the room UPDATE and reservation INSERT) to the database in a single transaction.

---

## 8.4 PaymentController.cs

### GenerateBill

```csharp
public async Task<IActionResult> GenerateBill(int reservationId)
{
    var res = await _context.Reservations
        .Include(r => r.Guest)
        .Include(r => r.Room)
        .Include(r => r.Employee)
        .Include(r => r.Payments)
        .FirstOrDefaultAsync(r => r.ReservationID == reservationId);

    var vm = new BillViewModel
    {
        GuestName = res.Guest.FullName,
        PricePerNight = res.Room.Price,
        CheckInDate = res.CheckInDate,
        CheckOutDate = res.CheckOutDate,
        Payments = res.Payments.ToList(),
        HandledBy = res.Employee.Name
    };
    return View(vm);
}
```

This method:

1. Loads the reservation with all related data using multiple `.Include()` calls
2. Maps the data into a `BillViewModel` — copying specific fields into a format the view understands
3. Returns the `GenerateBill.cshtml` view with the bill data

The `BillViewModel` computes `SubTotal`, `TotalPaid`, and `BalanceDue` automatically through its computed properties.

---

# 9. Views

Views are `.cshtml` files — a mix of HTML and **Razor syntax** (C# code embedded in HTML). The Razor engine processes these files and generates pure HTML that is sent to the browser.

## 9.1 Razor Syntax Fundamentals

**`@`** — The at sign switches from HTML mode to C# mode:

- `@Model.GuestName` — Outputs the value of `Model.GuestName`
- `@DateTime.Now.Year` — Outputs the current year
- `@{ var x = 5; }` — A C# code block (not outputted)
- `@if (condition) { ... }` — Conditional HTML rendering
- `@foreach (var item in list) { ... }` — Loops over a list

**`@model HotelMS.Models.Guest`** — Declares the model type. This tells Razor what type `Model` is, enabling IntelliSense and type safety.

**`asp-for`, `asp-action`, `asp-controller`** — These are **Tag Helpers** that generate correct HTML attributes:

- `<input asp-for="FirstName" />` generates `<input name="FirstName" id="FirstName" />`
- `<form asp-action="AddGuest" asp-controller="Guest">` generates `<form action="/Guest/AddGuest" method="post">`

**`asp-validation-for`** — Generates a `<span>` that displays validation error messages.

---

## 9.2 Login.cshtml

```html
@model HotelMS.Models.ViewModels.LoginViewModel @{ Layout = null; }
```

**`Layout = null`** — The login page does NOT use the shared layout (`_Layout.cshtml`). This is because the login page has its own full-screen design without the sidebar and navbar. All other pages inherit the layout.

```html
<div class="login-card @(ViewBag.Error != null ? "shake" : "")">
```

This ternary expression in Razor: if there is an error, add the CSS class `shake` to make the card animate with a shake effect. Otherwise, add nothing.

```html
<form asp-action="Login" asp-controller="Account" method="post" id="login-form">
  @Html.AntiForgeryToken()
</form>
```

`@Html.AntiForgeryToken()` generates a hidden input field with a unique token:

```html
<input type="hidden" name="__RequestVerificationToken" value="..." />
```

This token is checked by `[ValidateAntiForgeryToken]` in the controller to prevent CSRF attacks.

```html
<input asp-for="Username" type="text" class="form-input" placeholder=" " />
<label asp-for="Username" class="float-lbl">Username</label>
```

The floating label effect works with CSS. When the input is empty, the label appears inside the input. When focused or filled, the label floats above. The `placeholder=" "` (a space, not empty) is required for the CSS `:not(:placeholder-shown)` selector to work correctly.

---

## 9.3 \_Layout.cshtml

The layout is the master template used by all pages (except Login). It defines the sidebar, navbar, and the area where page content is inserted.

```html
@RenderBody()
```

This is where each individual page's HTML is inserted. Every view that uses this layout replaces `@RenderBody()` with its own content.

```html
@{ var role = Context.Session.GetString("UserRole") ?? ""; var username =
Context.Session.GetString("Username") ?? ""; }
```

Reading the session directly in the layout. `Context.Session` is available in all Razor views. The `?? ""` is the **null-coalescing operator** — if `GetString()` returns `null`, use `""` instead.

```html
@if (role == "Admin" || role == "Manager") {
<li class="nav-item">
  <a href="/Employee/ListEmployees">Employees</a>
</li>
}
```

Role-based navigation — menu items are only shown if the user has the right role. This runs on the server every time the page loads.

```html
@RenderSection("Scripts", required: false)
```

This allows individual views to inject JavaScript specific to that page. The `required: false` means it is optional — most pages do not have a `@section Scripts` block. The `CreateReservation.cshtml` view uses this to inject the live price calculator script.

---

## 9.4 Dashboard/Index.cshtml

```html
@model HotelMS.Models.ViewModels.DashboardViewModel
```

The Dashboard receives a `DashboardViewModel` filled with all the statistics.

```html
<div class="stat-card" style="--card-index:0">
  <div class="stat-number" data-count-target="@Model.TotalGuests">0</div>
  <div class="stat-label">Total Guests</div>
</div>
```

**`style="--card-index:0"`** — Sets a CSS Custom Property (CSS variable) directly in the HTML. The CSS uses this to stagger the animation delay of each card.

**`data-count-target="@Model.TotalGuests"`** — A custom HTML data attribute. The JavaScript reads this attribute and animates the number from 0 up to the target value. Initially showing `0`, then counting up.

```html
<circle
  cx="60"
  cy="60"
  r="50"
  class="ring-fill"
  id="occupancy-ring"
  style="--occupancy: @Model.OccupancyRate"
/>
```

The SVG occupancy ring. The CSS variable `--occupancy` is used to calculate `stroke-dashoffset`, which controls how much of the circle's stroke is visible, creating the ring effect.

---

## 9.5 Reservation/CreateReservation.cshtml

```html
@section Scripts {
<script>
  function updatePreview() {
    const roomSel = document.getElementById("room-select");
    const checkIn = document.getElementById("checkin-date").value;
    const checkOut = document.getElementById("checkout-date").value;
    const opt = roomSel.options[roomSel.selectedIndex];

    if (checkIn && checkOut && roomSel.value) {
      const d1 = new Date(checkIn),
        d2 = new Date(checkOut);
      const nights = Math.max(0, Math.round((d2 - d1) / 86400000));
      const price = parseFloat(opt.dataset.price) || 0;
      document.getElementById("preview-total").textContent =
        "$" + (nights * price).toFixed(2);
    }
  }
</script>
}
```

**`@section Scripts { ... }`** — This JavaScript block is injected into the layout's `@RenderSection("Scripts")` placeholder. This keeps page-specific JavaScript with its page, not in the global `site.js`.

**`opt.dataset.price`** — Each `<option>` element has `data-price="@r.Price"` set server-side. `dataset.price` reads this HTML data attribute in JavaScript.

**`(d2 - d1) / 86400000`** — JavaScript `Date` subtraction gives milliseconds. 86400000 is the number of milliseconds in one day (1000ms × 60s × 60min × 24hr). Dividing converts milliseconds to days.

**`.toFixed(2)`** — Formats a JavaScript number to exactly 2 decimal places (for currency display).

---

## 9.6 Payment/GenerateBill.cshtml

```html
@media print { .no-print { display: none !important; } }
```

The `.no-print` CSS class is applied to buttons, navigation, and other UI elements. When the user presses `Ctrl+P` or clicks "Print Bill", the browser switches to print mode, and everything with `.no-print` disappears, leaving only the clean invoice.

```html
@if (Model.BalanceDue <= 0) {
<div class="bill-paid-stamp">
  <i class="fas fa-check-circle"></i> PAID IN FULL
</div>
}
```

This server-side C# condition adds the "PAID IN FULL" stamp only when the balance is zero or negative (meaning the guest has paid everything).

---

# 10. Static Assets — CSS and JavaScript

## 10.1 site.css — Master Stylesheet

### CSS Custom Properties (Variables)

```css
:root {
  --primary: #0a1628;
  --gold: #c9a84c;
  --gold-gradient: linear-gradient(135deg, #c9a84c 0%, #e4c470 100%);
}
```

`:root` targets the `<html>` element. Variables defined here are accessible everywhere in the CSS with `var(--name)`. This makes it easy to change the entire color scheme by editing one place.

`#0a1628` is a **hexadecimal color** — `0a` = red (10), `16` = green (22), `28` = blue (40) — a very dark navy blue.

### Sidebar CSS

```css
#sidebar {
  position: fixed;
  top: 0;
  left: 0;
  height: 100vh;
  width: var(--sidebar-width);
  z-index: 1000;
  transition: var(--transition);
}
```

**`position: fixed`** — The sidebar stays in place even when the page scrolls. It is positioned relative to the viewport (browser window), not the document.

**`height: 100vh`** — `vh` = viewport height. `100vh` means 100% of the browser window's height.

**`z-index: 1000`** — Controls stacking order. Elements with higher z-index appear on top of elements with lower z-index. 1000 ensures the sidebar appears above all page content.

**`transition: var(--transition)`** — `--transition` is `all 0.3s cubic-bezier(0.4,0,0.2,1)`. This means ALL CSS properties that change (like `width`) will animate smoothly over 0.3 seconds with a specific easing curve. The `cubic-bezier` values create a natural-feeling deceleration (fast start, slow end) — similar to how Google Material Design animations feel.

### Active Nav Link with Pseudo-element

```css
.nav-link::before {
  content: "";
  position: absolute;
  left: 0;
  top: 0;
  width: 0;
  height: 100%;
  background: rgba(201, 168, 76, 0.08);
  transition: var(--transition);
}
.nav-link:hover::before,
.nav-link.active::before {
  width: 100%;
}
```

`::before` creates a pseudo-element — a virtual element that exists only in CSS, not in the HTML. This creates the hover background effect without adding extra HTML. When hovered, the width transitions from 0 to 100%, creating a smooth slide-in highlight effect.

### Stat Card Animation

```css
.stat-card {
  animation: fadeInUp 0.5s ease both;
  animation-delay: calc(var(--card-index, 0) * 0.1s);
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(16px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
```

**`animation-delay: calc(var(--card-index, 0) * 0.1s)`** — Each stat card has `--card-index` set via `style="--card-index:0"` etc. in the HTML. This creates a **stagger effect**: card 0 starts immediately, card 1 starts after 0.1s, card 2 after 0.2s, etc. The `calc()` function performs math in CSS.

**`@keyframes fadeInUp`** — Defines a named animation:

- `from` = starting state: invisible (opacity 0) and shifted down (translateY 16px)
- `to` = ending state: fully visible (opacity 1) at normal position

`transform: translateY(16px)` moves the element 16 pixels downward. Animating to `translateY(0)` slides it up to its natural position.

### Occupancy Ring

```css
.ring-fill {
  stroke-dasharray: 314.159;
  stroke-dashoffset: calc(314.159 * (1 - var(--occupancy, 0) / 100));
}
```

**`stroke-dasharray: 314.159`** — The circumference of the SVG circle (2 × π × radius = 2 × 3.14159 × 50 = 314.159). Setting `stroke-dasharray` to the full circumference makes the entire stroke visible as a single dash.

**`stroke-dashoffset: calc(314.159 * (1 - occupancy/100))`** — `stroke-dashoffset` shifts the dash pattern. If occupancy is 60%, the offset is `314.159 × 0.40 = 125.66`, leaving 60% of the stroke visible. This creates the ring "fill" effect based on occupancy percentage.

### Glassmorphism Effect (Login page)

```css
.login-card {
  background: rgba(13, 33, 55, 0.7);
  backdrop-filter: blur(24px);
  border: 1px solid rgba(201, 168, 76, 0.25);
  box-shadow:
    0 24px 80px rgba(0, 0, 0, 0.6),
    inset 0 1px 0 rgba(255, 255, 255, 0.06);
}
```

**`background: rgba(13, 33, 55, 0.7)`** — `rgba` = Red, Green, Blue, Alpha (opacity). The fourth value `0.7` makes it 70% opaque, allowing the background to show through slightly.

**`backdrop-filter: blur(24px)`** — Applies a blur effect to everything **behind** the element. This creates the frosted glass effect. Without this, the background would just be semi-transparent but not blurry.

**`box-shadow: 0 24px 80px rgba(0,0,0,0.6), inset 0 1px 0 rgba(255,255,255,0.06)`** — Two box shadows:

1. `0 24px 80px rgba(0,0,0,0.6)` — A large dark shadow below the card (x=0, y=24px offset, blur=80px)
2. `inset 0 1px 0 rgba(255,255,255,0.06)` — A very subtle white line at the top inside edge, simulating light hitting the glass surface

---

## 10.2 site.js — Main JavaScript

### Sidebar Toggle

```javascript
const savedState = localStorage.getItem("sidebarCollapsed");
if (savedState === "true" && !isMobile()) {
  sidebar?.classList.add("collapsed");
  main?.classList.add("expanded");
}

toggleBtn.addEventListener("click", function () {
  const collapsed = sidebar?.classList.toggle("collapsed");
  main?.classList.toggle("expanded");
  localStorage.setItem("sidebarCollapsed", collapsed ? "true" : "false");
});
```

**`localStorage.getItem('sidebarCollapsed')`** — `localStorage` persists data in the browser even after the page is closed. Here we remember if the user collapsed the sidebar, so it stays collapsed when they navigate to another page.

**`classList.toggle('collapsed')`** — Adds the class if it is not present, removes it if it is. Returns `true` if the class was added (sidebar is now collapsed).

**`?.` (optional chaining)** — `sidebar?.classList` is safe even if `sidebar` is `null` — it returns `undefined` instead of throwing an error.

### Count-Up Animation

```javascript
const countUp = (el, target, duration) => {
  let start = null;
  const step = (ts) => {
    if (!start) start = ts;
    const progress = Math.min((ts - start) / duration, 1);
    const ease = 1 - Math.pow(1 - progress, 3);
    const val = Math.round(ease * target);
    el.textContent = val.toLocaleString();
    if (progress < 1) requestAnimationFrame(step);
  };
  requestAnimationFrame(step);
};
```

**`requestAnimationFrame(step)`** — Tells the browser to call the `step` function before the next screen repaint (typically 60 times per second). This creates a smooth animation synchronized with the screen refresh rate.

**`ts` (timestamp)** — The browser passes the current time in milliseconds to `requestAnimationFrame` callbacks.

**`progress = Math.min((ts - start) / duration, 1)`** — Calculates how far through the animation we are (0 to 1). `Math.min(..., 1)` caps it at 1 so it never exceeds 100%.

**`ease = 1 - Math.pow(1 - progress, 3)`** — This is a **cubic ease-out** formula. At the start, the number increases quickly. As it approaches the target, it slows down, creating a natural-feeling animation instead of a mechanical constant-speed increment.

### Delete Confirm Modal

```javascript
document.querySelectorAll(".delete-form").forEach((form) => {
  form.addEventListener("submit", function (e) {
    e.preventDefault(); // Stop the form from submitting
    pendingForm = form; // Remember which form was submitted
    modal.style.display = "flex"; // Show the confirm modal
  });
});

confirmBtn.addEventListener("click", () => {
  if (pendingForm) {
    pendingForm.submit(); // Actually submit the saved form
  }
  modal.style.display = "none";
});
```

**`e.preventDefault()`** — Stops the default browser behavior (form submission). We intercept it to show a confirmation dialog first.

**`pendingForm = form`** — We save a reference to the form. If the user clicks "Confirm", we submit this saved form. If they click "Cancel", we discard `pendingForm`.

**`pendingForm.submit()`** — Submits the form programmatically (bypasses the event listener, so it doesn't trigger the confirmation again).

### IntersectionObserver for Table Row Animations

```javascript
const rowObserver = new IntersectionObserver(
  (entries) => {
    entries.forEach((entry, i) => {
      if (entry.isIntersecting) {
        entry.target.style.animation = "fadeInUp 0.35s ease both";
        rowObserver.unobserve(entry.target);
      }
    });
  },
  { threshold: 0.05 },
);

rows.forEach((row) => rowObserver.observe(row));
```

**`IntersectionObserver`** — A browser API that watches when elements enter or exit the viewport (the visible area of the page). This is more efficient than listening to `scroll` events.

**`{ threshold: 0.05 }`** — The callback fires when at least 5% of the element is visible.

**`entry.isIntersecting`** — `true` when the observed element is visible in the viewport.

**`rowObserver.unobserve(entry.target)`** — Stops observing once the animation has been triggered. There is no need to re-animate a row that has already been animated.

---

# 11. Security Implementation

## 11.1 Authentication Flow

```
User submits login form
         ↓
AccountController.Login (POST) receives form data
         ↓
Look up account by username in database
         ↓
If found: verify password with AuthHelper.VerifyPassword()
         ↓
If verified: store UserID, Username, Role in session
         ↓
Redirect to Dashboard
         ↓
On every subsequent request: controller checks session
         ↓
If session has UserID: user is authenticated → allow
If session is empty: user is not logged in → redirect to Login
```

## 11.2 Role-Based Authorization

The authorization happens inside each controller action method. There is no separate middleware for this (unlike JWT-based systems). For example:

```csharp
public async Task<IActionResult> AddRoom()
{
    var auth = CheckAuth("Admin", "Manager");  // Only these roles allowed
    if (auth != null) return auth;
    return View();
}
```

If a Receptionist tries to access `/Room/AddRoom`, `CheckAuth("Admin", "Manager")` will detect they have the wrong role and redirect them to the Dashboard.

## 11.3 Anti-Forgery Protection

Every POST form includes:

```html
@Html.AntiForgeryToken()
```

And every POST controller action has:

```csharp
[ValidateAntiForgeryToken]
```

This prevents CSRF attacks by ensuring form submissions can only come from your own site, not from external sites.

## 11.4 SQL Injection Prevention

Because we use Entity Framework Core with LINQ instead of raw SQL strings, SQL injection is automatically prevented. EF Core parameterizes all queries:

```csharp
// This code:
_context.Accounts.FirstOrDefaultAsync(a => a.Username == model.Username)

// Generates this SQL internally:
SELECT * FROM Accounts WHERE Username = @p0  -- @p0 is a parameter, not string concatenation
```

Even if a user enters `'; DROP TABLE Accounts; --` as their username, EF treats it as a literal string value, not SQL code.

---

# 12. Database Design and Relationships

## Entity-Relationship Summary

```
Guest (1) ────────── (many) Reservation
Room  (1) ────────── (many) Reservation
Employee (1) ──────── (many) Reservation
Reservation (1) ───── (many) Payment
Employee (1) ──────── (0 or 1) Account
```

## Foreign Key Flow

When a reservation is created:

```
Reservation.GuestID    → references Guests.GuestID
Reservation.RoomID     → references Rooms.RoomID
Reservation.EmployeeID → references Employees.EmployeeID
```

When a payment is made:

```
Payment.ReservationID  → references Reservations.ReservationID
```

## Delete Behavior Summary

| If you delete... | What happens to...                                                   |
| ---------------- | -------------------------------------------------------------------- |
| A Guest          | Blocked if they have reservations (`DeleteBehavior.Restrict`)        |
| A Room           | Blocked if it has reservations (`DeleteBehavior.Restrict`)           |
| An Employee      | Blocked if they handled reservations (`DeleteBehavior.Restrict`)     |
| A Reservation    | All related Payments are also deleted (`DeleteBehavior.Cascade`)     |
| An Employee      | Their Account's `EmployeeID` becomes NULL (`DeleteBehavior.SetNull`) |

## Unique Constraints

| Column       | Table    | Why                                        |
| ------------ | -------- | ------------------------------------------ |
| `RoomNumber` | Rooms    | Two rooms cannot have the same number      |
| `Username`   | Accounts | Two accounts cannot have the same username |

These constraints are enforced both at the application level (controller checks) and at the database level (unique indexes), providing double protection.

---

# 13. How Everything Connects — Request Lifecycle

## Example: A Receptionist Creates a Reservation

### Step 1 — Browser sends request

The receptionist clicks "New Reservation" link.
Browser sends: `GET /Reservation/CreateReservation`

### Step 2 — Routing

ASP.NET Core's routing engine reads `/Reservation/CreateReservation` and maps it to:

- Controller: `ReservationController`
- Action method: `CreateReservation()` with `[HttpGet]`

### Step 3 — Authentication Check

```csharp
var auth = CheckAuth("Admin", "Manager", "Receptionist");
if (auth != null) return auth;
```

The session is checked. The receptionist is logged in with role `"Receptionist"`, which is in the allowed list, so `auth` is null and execution continues.

### Step 4 — Database Queries

```csharp
ViewBag.Guests = await _context.Guests.OrderBy(g => g.LastName).ToListAsync();
ViewBag.Rooms = await _context.Rooms.Where(r => r.Status == "Available").ToListAsync();
```

EF Core generates and executes SQL:

```sql
SELECT * FROM Guests ORDER BY LastName
SELECT * FROM Rooms WHERE Status = 'Available'
```

### Step 5 — View Rendering

`return View();` triggers `Views/Reservation/CreateReservation.cshtml`.

Razor processes the file:

- `@foreach (var g in (IEnumerable<Guest>)ViewBag.Guests)` loops over guests and generates `<option>` elements
- CSS and JS references in `_Layout.cshtml` are included

### Step 6 — Browser receives HTML

The browser renders the page. The JavaScript `updatePreview()` function is ready to fire when the user selects a room and dates.

### Step 7 — User fills form and submits

Browser sends: `POST /Reservation/CreateReservation` with form data in the request body.

### Step 8 — Model Binding

ASP.NET Core reads the POST body and automatically fills a `Reservation` object:

```
CheckInDate  = 2024-05-10
CheckOutDate = 2024-05-15
GuestID      = 3
RoomID       = 7
EmployeeID   = 2
```

This is called **model binding** — the framework matches form field names to model property names automatically.

### Step 9 — Validation

`if (!ModelState.IsValid)` checks all Data Annotations. If any required field is missing or invalid, the form is returned with error messages.

### Step 10 — Business Logic

```csharp
// 1. Check date order
if (model.CheckOutDate <= model.CheckInDate) ...

// 2. Check room availability (no overlap)
bool unavailable = await _context.Reservations.AnyAsync(...);

// 3. Update room status
var room = await _context.Rooms.FindAsync(model.RoomID);
room.Status = "Booked";

// 4. Save reservation
_context.Reservations.Add(model);
await _context.SaveChangesAsync();  // Executes INSERT and UPDATE in one transaction
```

### Step 11 — Redirect

```csharp
return RedirectToAction("GetReservationById", new { id = model.ReservationID });
```

After saving, the user is redirected to view the reservation they just created. This follows the **Post/Redirect/Get** pattern — redirecting after a POST prevents the form from being resubmitted if the user refreshes the page.

### Step 12 — Success displayed

The `TempData["Success"]` message (if set) is displayed in the `_Layout.cshtml` alert box and auto-dismisses after 4 seconds via JavaScript.

---

## Summary Table — Files and Their Roles

| File                       | Language          | Role                                              |
| -------------------------- | ----------------- | ------------------------------------------------- |
| `Models/*.cs`              | C#                | Define database tables and their relationships    |
| `Models/ViewModels/*.cs`   | C#                | Carry specific data between controllers and views |
| `Data/HotelDbContext.cs`   | C#                | Connect to database, configure relationships      |
| `Helpers/AuthHelper.cs`    | C#                | Hash/verify passwords, check authentication       |
| `Helpers/SessionHelper.cs` | C#                | Read/write session data (who is logged in)        |
| `Program.cs`               | C#                | Configure and start the application               |
| `Controllers/*.cs`         | C#                | Handle HTTP requests, apply business logic        |
| `Views/**/*.cshtml`        | HTML + Razor (C#) | Generate HTML pages for the browser               |
| `wwwroot/css/site.css`     | CSS               | Style all pages (colors, layout, animations)      |
| `wwwroot/css/login.css`    | CSS               | Style the standalone login page                   |
| `wwwroot/js/site.js`       | JavaScript        | Client-side interactivity (sidebar, animations)   |
| `appsettings.json`         | JSON              | Configuration (database connection string)        |
| `Database/hotel_setup.sql` | SQL               | Manual database creation backup script            |

---

_Hotel Luxe Management System — Technical Report_
_St. Mary University, Rapid Application Development Course, 2016 E.C._
