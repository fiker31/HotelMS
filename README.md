# Hotel Luxe — Hotel Management and Reservation System

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Technology Stack](#2-technology-stack)
3. [System Requirements](#3-system-requirements)
4. [Step 1 — Install Prerequisites](#4-step-1--install-prerequisites)
5. [Step 2 — Get the Project](#5-step-2--get-the-project)
6. [Step 3 — Set Up the Database](#6-step-3--set-up-the-database)
7. [Step 4 — Run the Application](#7-step-4--run-the-application)
8. [Default Login Credentials](#8-default-login-credentials)
9. [User Roles and Permissions](#9-user-roles-and-permissions)
10. [Project Structure](#10-project-structure)
11. [Features](#11-features)
12. [Troubleshooting](#12-troubleshooting)

---

## 1. Project Overview

Hotel Luxe is a complete **Hotel Management and Reservation System** built for the St. Mary University RAD course. It allows hotel staff to manage guests, rooms, reservations, payments, and employees through a role-based web application with a modern luxury UI.

**Key capabilities:**

- Guest registration and management
- Room management with real-time availability
- Reservation booking with date conflict detection
- Payment processing and invoice/bill generation
- Employee management and role assignment
- Role-based access control (Admin, Manager, Receptionist, Staff)
- Animated, responsive luxury UI (navy + gold theme)

---

## 2. Technology Stack

| Layer                 | Technology                                 |
| --------------------- | ------------------------------------------ |
| **Backend**           | ASP.NET Core 8.0 MVC (C#)                  |
| **ORM / Data Access** | Entity Framework Core 8.0                  |
| **Database**          | Microsoft SQL Server (LocalDB)             |
| **Frontend**          | HTML5, CSS3 (custom), JavaScript (vanilla) |
| **Fonts**             | Google Fonts — Playfair Display, Inter     |
| **Icons**             | Font Awesome 6.4 (CDN)                     |
| **Authentication**    | Session-based (ASP.NET Core Session)       |
| **Password Security** | SHA-256 with random salt                   |
| **IDE**               | Visual Studio Code                         |

---

## 3. System Requirements

| Requirement      | Minimum                                             |
| ---------------- | --------------------------------------------------- |
| Operating System | Windows 10 / Windows 11                             |
| RAM              | 4 GB (8 GB recommended)                             |
| Disk Space       | 5 GB free (for SDKs and database)                   |
| Internet         | Required only for Google Fonts and Font Awesome CDN |

---

## 4. Step 1 — Install Prerequisites

Install each of the following **in order**. Skip any you already have.

---

### 4.1 Install .NET 8 SDK

The .NET 8 SDK is required to build and run the backend.

**Download:**
Go to `https://dotnet.microsoft.com/download/dotnet/8.0` and download:

- **.NET 8.0 SDK** (not Runtime — you need the full SDK)
- Choose **Windows x64 Installer**

**Install:** Run the `.exe` installer, click through all defaults.

**Verify installation** — open a new Command Prompt or PowerShell and run:

```
dotnet --version
```

You should see something like: `8.0.xxx`

---

### 4.2 Install SQL Server LocalDB

LocalDB is a free, lightweight version of SQL Server built for development. It runs silently in the background and stores your hotel database.

**Download SQL Server Express (includes LocalDB):**

Go to: `https://www.microsoft.com/en-us/sql-server/sql-server-downloads`

Click **Download now** under the **Express** edition.

Run the installer. When asked to choose an installation type, select **Basic** — this installs LocalDB automatically with no extra configuration needed.

**Verify LocalDB is installed** — open a new PowerShell or Command Prompt and run:

```
sqllocaldb info
```

You should see `MSSQLLocalDB` listed.

---

### 4.3 Install Entity Framework Core Tools

This is needed to run database migrations from the command line.

Open Command Prompt or PowerShell and run:

```
dotnet tool install --global dotnet-ef
```

If you already have it, update it:

```
dotnet tool update --global dotnet-ef
```

**Verify:**

```
dotnet ef --version
```

You should see something like: `Entity Framework Core .NET Command-line Tools 8.0.x`

---

### 4.4 Install Visual Studio Code

VS Code is the code editor used to open, browse, and run this project.

**Download:** `https://code.visualstudio.com/`

Run the installer. Make sure to check:

- **Add to PATH** (lets you open VS Code from the terminal)
- **Register Code as an editor for supported file types**

**After installing**, open VS Code and install these two extensions:

1. Press `Ctrl + Shift + X` to open the Extensions panel
2. Search and install:
   - **C# Dev Kit** — by Microsoft
   - **C#** — by Microsoft

These give you syntax highlighting, error detection, and IntelliSense for C#.

---

### 4.5 (Optional) Install SQL Server Management Studio — SSMS

SSMS is a free graphical tool that lets you open the database and visually inspect all tables and data. It is **not required** to run the app, but very useful for seeing what is stored in the database.

**Download:** `https://aka.ms/ssmsfullsetup`

Install with all defaults. To connect after installing:

- Server name: `(localdb)\MSSQLLocalDB`
- Authentication: **Windows Authentication**
- Click **Connect**

---

## 5. Step 2 — Get the Project

### Option A — Copy the project folder

Copy the `HotelMS` folder to your computer (e.g., `C:\Users\YourName\HotelMS`).

### Option B — Clone from Git (if hosted)

```
git clone <repository-url>
cd HotelMS
```

---

## 6. Step 3 — Set Up the Database

The database is created automatically when you run the migrations. Follow these steps exactly.

### 6.1 Open a terminal in the project folder

Open **Command Prompt** or **PowerShell**, then navigate to the project:

```
cd C:\Users\YourName\HotelMS
```

(Replace `YourName` with your actual Windows username and folder path.)

### 6.2 Restore NuGet packages

```
dotnet restore
```

Wait for all packages to download. You will see output like:

```
Restored C:\Users\...\HotelMS\HotelMS.csproj
```

### 6.3 Create the database migration

```
dotnet ef migrations add InitialCreate
```

This creates a `Migrations` folder with the database schema. You should see:

```
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
```

### 6.4 Apply the migration (create the database)

```
dotnet ef database update
```

This creates the `HotelManagementDB` database in LocalDB and all six tables. You should see:

```
Build started...
Build succeeded.
Applying migration '..._InitialCreate'.
Done.
```

> **Note:** The default **admin account** (`admin` / `admin123`) is created automatically when the application starts for the first time — not during migration. You do NOT need to insert it manually.

### 6.5 (Alternative) Use the SQL script

If EF migrations fail for any reason, you can use the included SQL script as a backup:

1. Open **SQL Server Management Studio (SSMS)**
2. Connect to: `(localdb)\MSSQLLocalDB`
3. Open the file: `Database\hotel_setup.sql`
4. Press **F5** to execute

---

## 7. Step 4 — Run the Application

### Run from VS Code

1. Open VS Code
2. Go to **File → Open Folder** and select the `HotelMS` folder
3. Open the integrated terminal: press `` Ctrl + ` `` (backtick)
4. In the terminal, run:

```
dotnet run
```

You will see output like:

```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7123
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

**Open your browser** (Chrome, Edge, or Firefox) and go to the `https://localhost:XXXX` address shown — always use the **https** link, not http.

You will land on the **Login page** automatically.

To stop the app at any time, click inside the VS Code terminal and press `Ctrl + C`.

---

## 8. Default Login Credentials

When the application starts for the first time, it automatically creates a default admin account:

| Field        | Value      |
| ------------ | ---------- |
| **Username** | `admin`    |
| **Password** | `admin123` |
| **Role**     | Admin      |

> **Security note:** Change the admin password after first login via **Change Password** in the top navbar.

---

## 9. User Roles and Permissions

| Feature             | Admin | Manager | Receptionist | Staff      |
| ------------------- | ----- | ------- | ------------ | ---------- |
| Dashboard           | Full  | Full    | Partial      | Rooms only |
| View Rooms          | Yes   | Yes     | Yes          | Yes        |
| Add/Edit Rooms      | Yes   | Yes     | No           | No         |
| View Guests         | Yes   | Yes     | Yes          | No         |
| Add/Edit Guests     | Yes   | Yes     | Yes          | No         |
| Create Reservations | Yes   | Yes     | Yes          | No         |
| Cancel Reservations | Yes   | Yes     | Yes          | No         |
| Process Payments    | Yes   | Yes     | Yes          | No         |
| View Employees      | Yes   | Yes     | No           | No         |
| Add/Edit Employees  | Yes   | Yes     | No           | No         |
| Manage Accounts     | Yes   | No      | No           | No         |
| View Reports        | Yes   | Yes     | No           | No         |

---

## 10. Project Structure

```
HotelMS/
│
├── Controllers/                  # C# MVC Controllers (business logic)
│   ├── AccountController.cs      # Login, logout, account management
│   ├── DashboardController.cs    # Dashboard with stats and charts
│   ├── GuestController.cs        # Guest CRUD operations
│   ├── RoomController.cs         # Room CRUD operations
│   ├── ReservationController.cs  # Reservation booking, check-in/out
│   ├── PaymentController.cs      # Payments and bill generation
│   └── EmployeeController.cs     # Employee management
│
├── Data/
│   └── HotelDbContext.cs         # Entity Framework database context
│
├── Database/
│   └── hotel_setup.sql           # Manual SQL backup script
│
├── Helpers/
│   ├── AuthHelper.cs             # Password hashing and auth checks
│   └── SessionHelper.cs          # Session read/write utilities
│
├── Models/                       # C# entity classes (database tables)
│   ├── Guest.cs
│   ├── Room.cs
│   ├── Employee.cs
│   ├── Reservation.cs
│   ├── Payment.cs
│   ├── Account.cs
│   └── ViewModels/               # View-specific data models
│       ├── LoginViewModel.cs
│       ├── CreateAccountViewModel.cs
│       ├── ChangePasswordViewModel.cs
│       ├── ReservationViewModel.cs
│       ├── DashboardViewModel.cs
│       └── BillViewModel.cs
│
├── Views/                        # Razor HTML views (.cshtml)
│   ├── Account/                  # Login, create account, change password
│   ├── Dashboard/                # Main dashboard
│   ├── Guest/                    # Guest list, add, edit
│   ├── Room/                     # Room grid, add, edit
│   ├── Reservation/              # Reservation list, create, detail
│   ├── Payment/                  # Process payment, bill, payment list
│   ├── Employee/                 # Employee list, add, edit, profile
│   └── Shared/                   # Layout, navbar, error page
│
├── wwwroot/                      # Static files served to browser
│   ├── css/
│   │   ├── site.css              # Main stylesheet (navy + gold luxury theme)
│   │   └── login.css             # Login page stylesheet (glassmorphism)
│   └── js/
│       └── site.js               # Sidebar toggle, animations, delete confirm
│
├── appsettings.json              # App configuration + DB connection string
├── Program.cs                    # App startup + DI configuration + DB seeding
└── HotelMS.csproj                # Project file with package references
```

---

## 11. Features

### Login Page

- Glassmorphism card on an animated dark navy background
- Floating label form fields with password show/hide toggle
- Loading spinner on submit
- Auto-redirect if already logged in

### Dashboard

- Animated count-up stat cards (Guests, Available Rooms, Active Reservations, Revenue)
- SVG occupancy ring with animated fill
- Monthly revenue bar chart
- Recent reservations table with clickable rows
- Role-based quick action buttons

### Guest Management

- Searchable guest list with reservation count
- Add, edit, delete guests
- Protection against deleting guests with existing reservations

### Room Management

- Card grid with filter by type and status
- Color-coded status badges (Available = green, Booked = red, Maintenance = yellow)
- Room number uniqueness validation

### Reservation System

- Two-panel booking form with live price calculator
- Real-time total cost preview as you select room and dates
- Automatic date conflict detection
- Check-in / Check-out actions
- Reservation status (Upcoming / Active / Completed)

### Payment & Billing

- Process multiple partial payments per reservation
- Generate printable invoice with hotel letterhead
- PAID IN FULL stamp when balance is zero
- Payment history per reservation

### Employee Management

- Full employee profiles with linked system accounts
- Role assignment
- List of reservations handled per employee

### UI & UX

- Collapsible sidebar (state saved in localStorage)
- Confirm modal for all delete operations
- Auto-dismissing success/error alerts
- Responsive design for tablets and mobile
- Print-optimized bill view

---

## 12. Troubleshooting

### "dotnet: command not found" or "'dotnet' is not recognized"

- .NET 8 SDK is not installed or not on PATH
- Reinstall the .NET 8 SDK and restart your terminal/computer

### "Unable to connect to LocalDB"

Run this in Command Prompt to start LocalDB:

```
sqllocaldb start MSSQLLocalDB
```

Then try the migration again.

### "No migrations have been applied"

Run in order:

```
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### "A network-related or instance-specific error" on startup

LocalDB is not running. Start it:

```
sqllocaldb start MSSQLLocalDB
```

### Login page shows but login fails (even with admin/admin123)

The admin account is seeded on first application startup. Make sure `dotnet ef database update` ran successfully before running the app. The app creates the admin account when it first connects to the database.

### Port already in use

Change the port in `Properties\launchSettings.json` or stop the other process using that port.

### Pages load but icons/fonts are missing

You need an internet connection — Font Awesome and Google Fonts are loaded from CDN. Check your internet connection or contact your network administrator if behind a firewall.

### "Microsoft.AspNetCore.Session" warning during build

This is harmless. In .NET 8, Session is built into the framework. The old package reference is redundant but does not break anything.

---

## Database Tables

| Table          | Description                                  |
| -------------- | -------------------------------------------- |
| `Guests`       | Hotel guests / customers                     |
| `Rooms`        | Hotel rooms with type, price, status         |
| `Employees`    | Staff members                                |
| `Accounts`     | System login accounts (linked to employees)  |
| `Reservations` | Bookings connecting guests, rooms, employees |
| `Payments`     | Payment records per reservation              |

---

## Quick Start Summary

```
# 1. Install: VS Code, .NET 8 SDK, SQL Server LocalDB, EF Core Tools

# 2. Open HotelMS folder in VS Code, then open the terminal (Ctrl + `)
cd C:\Users\YourName\HotelMS

# 3. Restore packages
dotnet restore

# 4. Create and apply database
dotnet ef migrations add InitialCreate
dotnet ef database update

# 5. Run the app
dotnet run

# 6. Open browser → https://localhost:PORT
# 7. Login: admin / admin123
```

---

_Hotel Luxe Management System — St. Mary University, RAD Course, 2016 E.C._
