using Dapper;
using Microsoft.Data.SqlClient;

namespace PharmaOnline.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var targetDb = builder.InitialCatalog;
        builder.InitialCatalog = "master";

        using (var masterConn = new SqlConnection(builder.ConnectionString))
        {
            await masterConn.OpenAsync();
            await masterConn.ExecuteAsync($@"
                IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{targetDb}')
                BEGIN
                    CREATE DATABASE [{targetDb}];
                END");
        }

        using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUsers')
            BEGIN
                CREATE TABLE AspNetUsers (
                    Id                   NVARCHAR(450) NOT NULL PRIMARY KEY,
                    UserName             NVARCHAR(256) NULL,
                    NormalizedUserName   NVARCHAR(256) NULL,
                    Email                NVARCHAR(256) NULL,
                    NormalizedEmail      NVARCHAR(256) NULL,
                    EmailConfirmed       BIT NOT NULL DEFAULT 0,
                    PasswordHash         NVARCHAR(MAX) NULL,
                    SecurityStamp        NVARCHAR(MAX) NULL,
                    ConcurrencyStamp     NVARCHAR(MAX) NULL,
                    PhoneNumber          NVARCHAR(MAX) NULL,
                    PhoneNumberConfirmed BIT NOT NULL DEFAULT 0,
                    TwoFactorEnabled     BIT NOT NULL DEFAULT 0,
                    LockoutEnd           DATETIMEOFFSET NULL,
                    LockoutEnabled       BIT NOT NULL DEFAULT 1,
                    AccessFailedCount    INT NOT NULL DEFAULT 0,
                    FullName             NVARCHAR(100) NOT NULL DEFAULT '',
                    Address              NVARCHAR(300) NOT NULL DEFAULT '',
                    RegisteredAt         DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                );
            END

            IF COL_LENGTH('AspNetUsers', 'FullName') IS NULL
            BEGIN
                ALTER TABLE AspNetUsers ADD FullName NVARCHAR(100) NOT NULL DEFAULT '';
            END

            IF COL_LENGTH('AspNetUsers', 'Address') IS NULL
            BEGIN
                ALTER TABLE AspNetUsers ADD Address NVARCHAR(300) NOT NULL DEFAULT '';
            END

            IF COL_LENGTH('AspNetUsers', 'RegisteredAt') IS NULL
            BEGIN
                ALTER TABLE AspNetUsers ADD RegisteredAt DATETIME2 NOT NULL DEFAULT GETUTCDATE();
            END

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoles')
            BEGIN
                CREATE TABLE AspNetRoles (
                    Id               NVARCHAR(450) NOT NULL PRIMARY KEY,
                    Name             NVARCHAR(256) NULL,
                    NormalizedName   NVARCHAR(256) NULL,
                    ConcurrencyStamp NVARCHAR(MAX) NULL
                );
            END

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserRoles')
            BEGIN
                CREATE TABLE AspNetUserRoles (
                    UserId NVARCHAR(450) NOT NULL REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
                    RoleId NVARCHAR(450) NOT NULL REFERENCES AspNetRoles(Id) ON DELETE CASCADE,
                    PRIMARY KEY (UserId, RoleId)
                );
            END

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserClaims')
            BEGIN
                CREATE TABLE AspUserClaims (
                    Id         INT IDENTITY(1,1) PRIMARY KEY,
                    UserId     NVARCHAR(450) NOT NULL REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
                    ClaimType  NVARCHAR(MAX) NULL,
                    ClaimValue NVARCHAR(MAX) NULL
                );
            END

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserClaims')
            BEGIN
                CREATE TABLE AspNetUserClaims (
                    Id         INT IDENTITY(1,1) PRIMARY KEY,
                    UserId     NVARCHAR(450) NOT NULL REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
                    ClaimType  NVARCHAR(MAX) NULL,
                    ClaimValue NVARCHAR(MAX) NULL
                );
            END

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserLogins')
            BEGIN
                CREATE TABLE AspNetUserLogins (
                    LoginProvider       NVARCHAR(450) NOT NULL,
                    ProviderKey         NVARCHAR(450) NOT NULL,
                    ProviderDisplayName NVARCHAR(MAX) NULL,
                    UserId              NVARCHAR(450) NOT NULL REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
                    PRIMARY KEY (LoginProvider, ProviderKey)
                );
            END

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserTokens')
            BEGIN
                CREATE TABLE AspNetUserTokens (
                    UserId        NVARCHAR(450) NOT NULL REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
                    LoginProvider NVARCHAR(450) NOT NULL,
                    Name          NVARCHAR(450) NOT NULL,
                    Value         NVARCHAR(MAX) NULL,
                    PRIMARY KEY (UserId, LoginProvider, Name)
                );
            END

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoleClaims')
            BEGIN
                CREATE TABLE AspNetRoleClaims (
                    Id         INT IDENTITY(1,1) PRIMARY KEY,
                    RoleId     NVARCHAR(450) NOT NULL REFERENCES AspNetRoles(Id) ON DELETE CASCADE,
                    ClaimType  NVARCHAR(MAX) NULL,
                    ClaimValue NVARCHAR(MAX) NULL
                );
            END
        ");

        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Medicines')
            BEGIN
                CREATE TABLE Medicines (
                    Id          INT IDENTITY(1,1) PRIMARY KEY,
                    Name        NVARCHAR(200)  NOT NULL,
                    GenericName NVARCHAR(300)  NOT NULL DEFAULT '',
                    Price       DECIMAL(10,2)  NOT NULL DEFAULT 0,
                    Stock       INT            NOT NULL DEFAULT 0,
                    ExpiryDate  DATE           NOT NULL,
                    ImageUrl    NVARCHAR(500)  NOT NULL DEFAULT '',
                    Category    NVARCHAR(100)  NOT NULL DEFAULT '',
                    Description NVARCHAR(1000) NOT NULL DEFAULT ''
                );
            END");

        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CartItems')
            BEGIN
                CREATE TABLE CartItems (
                    Id          INT IDENTITY(1,1) PRIMARY KEY,
                    MedicineId  INT           NOT NULL REFERENCES Medicines(Id) ON DELETE CASCADE,
                    UserId      NVARCHAR(450) NOT NULL,
                    Quantity    INT           NOT NULL DEFAULT 1,
                    AddedAt     DATETIME2     NOT NULL DEFAULT GETUTCDATE()
                );
            END");

        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')
            BEGIN
                CREATE TABLE Orders (
                    Id              INT IDENTITY(1,1) PRIMARY KEY,
                    UserId          NVARCHAR(450)  NOT NULL,
                    TotalAmount     DECIMAL(10,2)  NOT NULL,
                    Status          NVARCHAR(50)   NOT NULL DEFAULT 'Pending',
                    OrderDate       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
                    DeliveryAddress NVARCHAR(500)  NOT NULL DEFAULT ''
                );
            END");

        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderItems')
            BEGIN
                CREATE TABLE OrderItems (
                    Id          INT IDENTITY(1,1) PRIMARY KEY,
                    OrderId     INT           NOT NULL REFERENCES Orders(Id) ON DELETE CASCADE,
                    MedicineId  INT           NOT NULL REFERENCES Medicines(Id),
                    Quantity    INT           NOT NULL,
                    UnitPrice   DECIMAL(10,2) NOT NULL
                );
            END");

        var count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Medicines");
        if (count == 0)
        {
            await conn.ExecuteAsync(@"
                INSERT INTO Medicines (Name, GenericName, Price, Stock, ExpiryDate, ImageUrl, Category, Description)
                VALUES
                ('Panadol Extra 500mg',  'Paracetamol / Caffeine',         250, 120, '2027-12-31', '/img/panadol.png',    'Analgesic',     'Panadol Extra is used for relief of headache, cold & flu symptoms.'),
                ('Augmentin 625mg',      'Amoxicillin / Clavulanate',      480,  45, '2026-08-30', '/img/augmentin.png',  'Antibiotic',    'Broad-spectrum antibiotic for bacterial infections.'),
                ('Arinac Forte',         'Ibuprofen / Pseudoephedrine',    180,  80, '2027-05-15', '/img/arinac.png',     'Decongestant',  'Relieves sinus congestion, nasal blockage and body aches.'),
                ('Rigix 10mg',           'Cetirizine HCl',                 140, 200, '2028-01-20', '/img/rigix.png',      'Antihistamine', 'Non-drowsy antihistamine for allergy relief.'),
                ('Brufen 400mg',         'Ibuprofen',                      120, 150, '2027-09-01', '/img/brufen.png',     'Analgesic',     'Anti-inflammatory pain relief for headache, dental and muscle pain.'),
                ('Cipro 500mg',          'Ciprofloxacin',                  320,  60, '2026-11-30', '/img/cipro.png',      'Antibiotic',    'Fluoroquinolone antibiotic for urinary tract and respiratory infections.'),
                ('ORS Sachet',           'Oral Rehydration Salts',          25, 500, '2028-06-30', '/img/ors.png',        'Rehydration',   'Replenishes electrolytes lost due to diarrhoea or vomiting.'),
                ('Metformin 500mg',      'Metformin HCl',                   85, 300, '2027-03-15', '/img/metformin.png',  'Antidiabetic',  'First-line medication for type 2 diabetes management.')
            ");
        }
    }
}
