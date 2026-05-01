-- ============================================================
--  Hotel Management and Reservation System — Database Setup
--  Run this script in SQL Server Management Studio (SSMS)
--  or use EF Migrations instead (preferred)
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'HotelManagementDB')
BEGIN
    CREATE DATABASE HotelManagementDB;
END
GO

USE HotelManagementDB;
GO

-- ============================================================
--  TABLE: Guests
-- ============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Guests' AND xtype = 'U')
BEGIN
    CREATE TABLE Guests (
        GuestID   INT IDENTITY(1,1) PRIMARY KEY,
        FirstName NVARCHAR(50)  NOT NULL,
        LastName  NVARCHAR(50)  NOT NULL,
        Phone     NVARCHAR(20)  NOT NULL,
        Email     NVARCHAR(100) NOT NULL
    );
END
GO

-- ============================================================
--  TABLE: Rooms
-- ============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Rooms' AND xtype = 'U')
BEGIN
    CREATE TABLE Rooms (
        RoomID     INT IDENTITY(1,1) PRIMARY KEY,
        RoomNumber NVARCHAR(10)   NOT NULL,
        RoomType   NVARCHAR(20)   NOT NULL DEFAULT 'Single',
        Price      DECIMAL(10,2)  NOT NULL DEFAULT 0.00,
        Status     NVARCHAR(20)   NOT NULL DEFAULT 'Available',
        CONSTRAINT UQ_Rooms_RoomNumber UNIQUE (RoomNumber)
    );
END
GO

-- ============================================================
--  TABLE: Employees
-- ============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Employees' AND xtype = 'U')
BEGIN
    CREATE TABLE Employees (
        EmployeeID INT IDENTITY(1,1) PRIMARY KEY,
        Name       NVARCHAR(100)  NOT NULL,
        Position   NVARCHAR(30)   NOT NULL DEFAULT 'Staff',
        Salary     DECIMAL(10,2)  NOT NULL DEFAULT 0.00,
        Gender     NVARCHAR(10)   NOT NULL,
        Age        INT            NOT NULL
    );
END
GO

-- ============================================================
--  TABLE: Accounts
-- ============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Accounts' AND xtype = 'U')
BEGIN
    CREATE TABLE Accounts (
        UserID     INT IDENTITY(1,1) PRIMARY KEY,
        Username   NVARCHAR(50)  NOT NULL,
        Password   NVARCHAR(255) NOT NULL,
        Role       NVARCHAR(20)  NOT NULL DEFAULT 'Staff',
        EmployeeID INT           NULL,
        CONSTRAINT UQ_Accounts_Username  UNIQUE (Username),
        CONSTRAINT FK_Accounts_Employees FOREIGN KEY (EmployeeID)
            REFERENCES Employees(EmployeeID) ON DELETE SET NULL
    );
END
GO

-- ============================================================
--  TABLE: Reservations
-- ============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Reservations' AND xtype = 'U')
BEGIN
    CREATE TABLE Reservations (
        ReservationID INT IDENTITY(1,1) PRIMARY KEY,
        CheckInDate   DATE NOT NULL,
        CheckOutDate  DATE NOT NULL,
        GuestID       INT  NOT NULL,
        RoomID        INT  NOT NULL,
        EmployeeID    INT  NOT NULL,
        CONSTRAINT FK_Reservations_Guests    FOREIGN KEY (GuestID)    REFERENCES Guests(GuestID),
        CONSTRAINT FK_Reservations_Rooms     FOREIGN KEY (RoomID)     REFERENCES Rooms(RoomID),
        CONSTRAINT FK_Reservations_Employees FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),
        CONSTRAINT CHK_Dates CHECK (CheckOutDate > CheckInDate)
    );
END
GO

-- ============================================================
--  TABLE: Payments
-- ============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Payments' AND xtype = 'U')
BEGIN
    CREATE TABLE Payments (
        PaymentID     INT IDENTITY(1,1) PRIMARY KEY,
        Amount        DECIMAL(10,2) NOT NULL,
        PaymentDate   DATETIME      NOT NULL DEFAULT GETDATE(),
        PaymentMethod NVARCHAR(30)  NOT NULL DEFAULT 'Cash',
        ReservationID INT           NOT NULL,
        CONSTRAINT FK_Payments_Reservations FOREIGN KEY (ReservationID)
            REFERENCES Reservations(ReservationID) ON DELETE CASCADE
    );
END
GO

-- ============================================================
--  SAMPLE DATA — optional, comment out if not needed
-- ============================================================

-- Sample Rooms
IF NOT EXISTS (SELECT 1 FROM Rooms)
BEGIN
    INSERT INTO Rooms (RoomNumber, RoomType, Price, Status) VALUES
        ('101', 'Single',  80.00, 'Available'),
        ('102', 'Single',  80.00, 'Available'),
        ('201', 'Double', 140.00, 'Available'),
        ('202', 'Double', 140.00, 'Available'),
        ('301', 'Suite',  280.00, 'Available'),
        ('302', 'Suite',  320.00, 'Available');
END
GO

-- Sample Employee
IF NOT EXISTS (SELECT 1 FROM Employees)
BEGIN
    INSERT INTO Employees (Name, Position, Salary, Gender, Age) VALUES
        ('System Admin',  'Manager',     5000.00, 'Other', 30),
        ('Jane Receptionist', 'Receptionist', 2500.00, 'Female', 27);
END
GO

-- NOTE: The admin account is auto-created by Program.cs on first startup.
-- The password "admin123" is hashed with SHA256 + random salt at runtime.
-- Do NOT insert it manually here.

PRINT 'Hotel Management Database setup complete.';
GO
