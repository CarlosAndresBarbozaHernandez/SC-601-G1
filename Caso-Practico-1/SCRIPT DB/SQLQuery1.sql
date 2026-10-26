
  USE BookStoreDB;
-- =============================================
-- Caso Práctico 1 - SC-601
-- Script completo de BD BookStoreDB (20 tablas)
-- Servidor según Web.config: .\SQLEXPRESS
-- Se puede correr varias veces: solo crea lo que falte.
-- =============================================
USE master;
GO
IF DB_ID('BookStoreDB') IS NULL
    CREATE DATABASE BookStoreDB;
GO
USE BookStoreDB;
GO

-- =============================================
-- 0. BOOKS (tabla base, por si no existe)
-- =============================================
IF OBJECT_ID('dbo.Books', 'U') IS NULL
CREATE TABLE [dbo].[Books] (
    [Id]              INT             IDENTITY (1, 1) NOT NULL,
    [Name]            NVARCHAR (200)  NOT NULL,
    [Title]           NVARCHAR (200)  NOT NULL,
    [Category]        NVARCHAR (100)  NULL,
    [Publisher]       NVARCHAR (200)  NULL,
    [Author]          NVARCHAR (200)  NULL,
    [Description]     NVARCHAR (MAX)  NULL,
    [PublicationDate] DATE            NULL,
    [Height]          DECIMAL (10, 2) NULL,
    [Width]           DECIMAL (10, 2) NULL,
    [Price]           DECIMAL (18, 2) NOT NULL,
    [IsBestSeller]    BIT             CONSTRAINT [DF__Books__IsBestSel__24927208] DEFAULT ((0)) NOT NULL,
    [Genre]           NVARCHAR (100)  NULL,
    [IsInStock]       BIT             CONSTRAINT [DF__Books__IsInStock__25869641] DEFAULT ((1)) NOT NULL,
    [CreatedAt]       DATETIME2 (7)   NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt]       DATETIME2 (7)   NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_Books] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- =============================================
-- 1. LOOKUP TABLES
-- =============================================
IF OBJECT_ID('dbo.Authors', 'U') IS NULL
CREATE TABLE [dbo].[Authors] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [FirstName] NVARCHAR (100) NOT NULL,
    [LastName]  NVARCHAR (100) NOT NULL,
    [Biography] NVARCHAR (MAX) NULL,
    [BirthDate] DATE           NULL,
    [Country]   NVARCHAR (100) NULL,
    [CreatedAt] DATETIME2 (7)  NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_Authors] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

IF OBJECT_ID('dbo.Publishers', 'U') IS NULL
CREATE TABLE [dbo].[Publishers] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [Name]      NVARCHAR (200) NOT NULL,
    [Country]   NVARCHAR (100) NULL,
    [Website]   NVARCHAR (255) NULL,
    [Email]     NVARCHAR (150) NULL,
    [CreatedAt] DATETIME2 (7)  NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_Publishers] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

IF OBJECT_ID('dbo.Categories', 'U') IS NULL
CREATE TABLE [dbo].[Categories] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (100) NOT NULL,
    [Description] NVARCHAR (500) NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_Categories_Name] UNIQUE ([Name])
);
GO

IF OBJECT_ID('dbo.Genres', 'U') IS NULL
CREATE TABLE [dbo].[Genres] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (100) NOT NULL,
    [Description] NVARCHAR (500) NULL,
    CONSTRAINT [PK_Genres] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_Genres_Name] UNIQUE ([Name])
);
GO

-- =============================================
-- 2. BOOK RELATIONSHIPS
-- (Tipos reconstruidos: en el PDF estas líneas salen cortadas)
-- =============================================
IF COL_LENGTH('dbo.Books', 'PublisherId')   IS NULL ALTER TABLE [dbo].[Books] ADD [PublisherId]   INT            NULL;
IF COL_LENGTH('dbo.Books', 'CategoryId')    IS NULL ALTER TABLE [dbo].[Books] ADD [CategoryId]    INT            NULL;
IF COL_LENGTH('dbo.Books', 'GenreId')       IS NULL ALTER TABLE [dbo].[Books] ADD [GenreId]       INT            NULL;
IF COL_LENGTH('dbo.Books', 'ISBN')          IS NULL ALTER TABLE [dbo].[Books] ADD [ISBN]          NVARCHAR (20)  NULL;
IF COL_LENGTH('dbo.Books', 'Pages')         IS NULL ALTER TABLE [dbo].[Books] ADD [Pages]         INT            NULL;
IF COL_LENGTH('dbo.Books', 'Language')      IS NULL ALTER TABLE [dbo].[Books] ADD [Language]      NVARCHAR (50)  NULL;
IF COL_LENGTH('dbo.Books', 'CoverImageUrl') IS NULL ALTER TABLE [dbo].[Books] ADD [CoverImageUrl] NVARCHAR (500) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Books_Publishers')
    ALTER TABLE [dbo].[Books] ADD CONSTRAINT [FK_Books_Publishers] FOREIGN KEY ([PublisherId]) REFERENCES [dbo].[Publishers] ([Id]);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Books_Categories')
    ALTER TABLE [dbo].[Books] ADD CONSTRAINT [FK_Books_Categories] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories] ([Id]);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Books_Genres')
    ALTER TABLE [dbo].[Books] ADD CONSTRAINT [FK_Books_Genres] FOREIGN KEY ([GenreId]) REFERENCES [dbo].[Genres] ([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Books_ISBN' AND object_id = OBJECT_ID('dbo.Books'))
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Books_ISBN]
    ON [dbo].[Books] ([ISBN]) WHERE [ISBN] IS NOT NULL;
GO

IF OBJECT_ID('dbo.BookAuthors', 'U') IS NULL
CREATE TABLE [dbo].[BookAuthors] (
    [BookId]   INT NOT NULL,
    [AuthorId] INT NOT NULL,
    CONSTRAINT [PK_BookAuthors] PRIMARY KEY CLUSTERED ([BookId], [AuthorId]),
    CONSTRAINT [FK_BookAuthors_Books]   FOREIGN KEY ([BookId])   REFERENCES [dbo].[Books] ([Id]),
    CONSTRAINT [FK_BookAuthors_Authors] FOREIGN KEY ([AuthorId]) REFERENCES [dbo].[Authors] ([Id])
);
GO

-- =============================================
-- 3. CUSTOMERS
-- =============================================
IF OBJECT_ID('dbo.Customers', 'U') IS NULL
CREATE TABLE [dbo].[Customers] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [FirstName]    NVARCHAR (100) NOT NULL,
    [LastName]     NVARCHAR (100) NOT NULL,
    [Email]        NVARCHAR (150) NOT NULL,
    [PasswordHash] NVARCHAR (255) NOT NULL,
    [Phone]        NVARCHAR (30)  NULL,
    [BirthDate]    DATE           NULL,
    [IsActive]     BIT            NOT NULL DEFAULT (1),
    [CreatedAt]    DATETIME2 (7)  NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt]    DATETIME2 (7)  NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_Customers_Email] UNIQUE ([Email])
);
GO

IF OBJECT_ID('dbo.Addresses', 'U') IS NULL
CREATE TABLE [dbo].[Addresses] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [CustomerId]  INT            NOT NULL,
    [AddressType] NVARCHAR (20)  NOT NULL DEFAULT ('Shipping'),
    [Street]      NVARCHAR (200) NOT NULL,
    [City]        NVARCHAR (100) NOT NULL,
    [State]       NVARCHAR (100) NULL,
    [PostalCode]  NVARCHAR (20)  NOT NULL,
    [Country]     NVARCHAR (100) NOT NULL,
    [IsDefault]   BIT            NOT NULL DEFAULT (0),
    CONSTRAINT [PK_Addresses] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Addresses_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id])
);
GO

-- =============================================
-- 4. ORDERS AND PAYMENTS
-- =============================================
IF OBJECT_ID('dbo.Orders', 'U') IS NULL
CREATE TABLE [dbo].[Orders] (
    [Id]                INT             IDENTITY (1, 1) NOT NULL,
    [CustomerId]        INT             NOT NULL,
    [ShippingAddressId] INT             NULL,
    [OrderDate]         DATETIME2 (7)   NOT NULL DEFAULT (SYSUTCDATETIME()),
    [Status]            NVARCHAR (30)   NOT NULL DEFAULT ('Pending'),
    [Subtotal]          DECIMAL (18, 2) NOT NULL,
    [TaxAmount]         DECIMAL (18, 2) NOT NULL DEFAULT (0),
    [ShippingCost]      DECIMAL (18, 2) NOT NULL DEFAULT (0),
    [Total]             DECIMAL (18, 2) NOT NULL,
    [Notes]             NVARCHAR (500)  NULL,
    [UpdatedAt]         DATETIME2 (7)   NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Orders_Customers] FOREIGN KEY ([CustomerId])        REFERENCES [dbo].[Customers] ([Id]),
    CONSTRAINT [FK_Orders_Addresses] FOREIGN KEY ([ShippingAddressId]) REFERENCES [dbo].[Addresses] ([Id])
);
GO

IF OBJECT_ID('dbo.OrderItems', 'U') IS NULL
CREATE TABLE [dbo].[OrderItems] (
    [Id]        INT             IDENTITY (1, 1) NOT NULL,
    [OrderId]   INT             NOT NULL,
    [BookId]    INT             NOT NULL,
    [Quantity]  INT             NOT NULL CHECK ([Quantity] > 0),
    [UnitPrice] DECIMAL (18, 2) NOT NULL,
    [Discount]  DECIMAL (18, 2) NOT NULL DEFAULT (0),
    CONSTRAINT [PK_OrderItems] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_OrderItems_Orders] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id]),
    CONSTRAINT [FK_OrderItems_Books]  FOREIGN KEY ([BookId])  REFERENCES [dbo].[Books] ([Id])
);
GO

IF OBJECT_ID('dbo.Payments', 'U') IS NULL
CREATE TABLE [dbo].[Payments] (
    [Id]             INT             IDENTITY (1, 1) NOT NULL,
    [OrderId]        INT             NOT NULL,
    [PaymentMethod]  NVARCHAR (30)   NOT NULL,
    [Amount]         DECIMAL (18, 2) NOT NULL,
    [PaymentDate]    DATETIME2 (7)   NOT NULL DEFAULT (SYSUTCDATETIME()),
    [Status]         NVARCHAR (30)   NOT NULL DEFAULT ('Completed'),
    [TransactionRef] NVARCHAR (100)  NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Payments_Orders] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id])
);
GO

-- =============================================
-- 5. SHOPPING CART
-- =============================================
IF OBJECT_ID('dbo.ShoppingCarts', 'U') IS NULL
CREATE TABLE [dbo].[ShoppingCarts] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [CustomerId] INT           NOT NULL,
    [CreatedAt]  DATETIME2 (7) NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt]  DATETIME2 (7) NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_ShoppingCarts] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ShoppingCarts_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id])
);
GO

IF OBJECT_ID('dbo.CartItems', 'U') IS NULL
CREATE TABLE [dbo].[CartItems] (
    [Id]       INT           IDENTITY (1, 1) NOT NULL,
    [CartId]   INT           NOT NULL,
    [BookId]   INT           NOT NULL,
    [Quantity] INT           NOT NULL DEFAULT (1) CHECK ([Quantity] > 0),
    [AddedAt]  DATETIME2 (7) NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_CartItems] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CartItems_Carts] FOREIGN KEY ([CartId]) REFERENCES [dbo].[ShoppingCarts] ([Id]),
    CONSTRAINT [FK_CartItems_Books] FOREIGN KEY ([BookId]) REFERENCES [dbo].[Books] ([Id]),
    CONSTRAINT [UQ_CartItems_Cart_Book] UNIQUE ([CartId], [BookId])
);
GO

-- =============================================
-- 6. REVIEWS
-- =============================================
IF OBJECT_ID('dbo.Reviews', 'U') IS NULL
CREATE TABLE [dbo].[Reviews] (
    [Id]         INT             IDENTITY (1, 1) NOT NULL,
    [BookId]     INT             NOT NULL,
    [CustomerId] INT             NOT NULL,
    [Rating]     TINYINT         NOT NULL CHECK ([Rating] BETWEEN 1 AND 5),
    [Comment]    NVARCHAR (1000) NULL,
    [CreatedAt]  DATETIME2 (7)   NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_Reviews] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Reviews_Books]     FOREIGN KEY ([BookId])     REFERENCES [dbo].[Books] ([Id]),
    CONSTRAINT [FK_Reviews_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id]),
    CONSTRAINT [UQ_Reviews_Book_Customer] UNIQUE ([BookId], [CustomerId])
);
GO

-- =============================================
-- 7. INVENTORY AND SUPPLIERS
-- =============================================
IF OBJECT_ID('dbo.Inventory', 'U') IS NULL
CREATE TABLE [dbo].[Inventory] (
    [Id]              INT           IDENTITY (1, 1) NOT NULL,
    [BookId]          INT           NOT NULL,
    [QuantityInStock] INT           NOT NULL DEFAULT (0) CHECK ([QuantityInStock] >= 0),
    [ReorderLevel]    INT           NOT NULL DEFAULT (5),
    [LastRestockedAt] DATETIME2 (7) NULL,
    CONSTRAINT [PK_Inventory] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Inventory_Books] FOREIGN KEY ([BookId]) REFERENCES [dbo].[Books] ([Id]),
    CONSTRAINT [UQ_Inventory_Book] UNIQUE ([BookId])
);
GO

IF OBJECT_ID('dbo.Suppliers', 'U') IS NULL
CREATE TABLE [dbo].[Suppliers] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (200) NOT NULL,
    [ContactName] NVARCHAR (150) NULL,
    [Email]       NVARCHAR (150) NULL,
    [Phone]       NVARCHAR (30)  NULL,
    [Address]     NVARCHAR (300) NULL,
    [IsActive]    BIT            NOT NULL DEFAULT (1),
    CONSTRAINT [PK_Suppliers] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

IF OBJECT_ID('dbo.PurchaseOrders', 'U') IS NULL
CREATE TABLE [dbo].[PurchaseOrders] (
    [Id]           INT             IDENTITY (1, 1) NOT NULL,
    [SupplierId]   INT             NOT NULL,
    [OrderDate]    DATETIME2 (7)   NOT NULL DEFAULT (SYSUTCDATETIME()),
    [ExpectedDate] DATE            NULL,
    [Status]       NVARCHAR (30)   NOT NULL DEFAULT ('Pending'),
    [TotalCost]    DECIMAL (18, 2) NOT NULL DEFAULT (0),
    CONSTRAINT [PK_PurchaseOrders] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PurchaseOrders_Suppliers] FOREIGN KEY ([SupplierId]) REFERENCES [dbo].[Suppliers] ([Id])
);
GO

IF OBJECT_ID('dbo.PurchaseOrderItems', 'U') IS NULL
CREATE TABLE [dbo].[PurchaseOrderItems] (
    [Id]              INT             IDENTITY (1, 1) NOT NULL,
    [PurchaseOrderId] INT             NOT NULL,
    [BookId]          INT             NOT NULL,
    [Quantity]        INT             NOT NULL CHECK ([Quantity] > 0),
    [UnitCost]        DECIMAL (18, 2) NOT NULL,
    CONSTRAINT [PK_PurchaseOrderItems] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_POItems_PurchaseOrders] FOREIGN KEY ([PurchaseOrderId]) REFERENCES [dbo].[PurchaseOrders] ([Id]),
    CONSTRAINT [FK_POItems_Books]          FOREIGN KEY ([BookId])          REFERENCES [dbo].[Books] ([Id])
);
GO

-- =============================================
-- 8. STAFF / ADMIN USERS
-- =============================================
IF OBJECT_ID('dbo.Roles', 'U') IS NULL
CREATE TABLE [dbo].[Roles] (
    [Id]   INT           IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_Roles_Name] UNIQUE ([Name])
);
GO

IF OBJECT_ID('dbo.Employees', 'U') IS NULL
CREATE TABLE [dbo].[Employees] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [RoleId]       INT            NOT NULL,
    [FirstName]    NVARCHAR (100) NOT NULL,
    [LastName]     NVARCHAR (100) NOT NULL,
    [Email]        NVARCHAR (150) NOT NULL,
    [PasswordHash] NVARCHAR (255) NOT NULL,
    [IsActive]     BIT            NOT NULL DEFAULT (1),
    [CreatedAt]    DATETIME2 (7)  NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_Employees] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Employees_Roles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([Id]),
    CONSTRAINT [UQ_Employees_Email] UNIQUE ([Email])
);
GO

-- Seed roles (only if missing)
INSERT INTO [dbo].[Roles] ([Name])
SELECT v.Name FROM (VALUES ('Admin'), ('Manager'), ('Cashier')) AS v(Name)
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Roles] r WHERE r.Name = v.Name);
GO

-- =============================================
-- VERIFICACIÓN (deberían salir 20 tablas)

SELECT name AS Tabla FROM sys.tables ORDER BY name;
SELECT * FROM dbo.Roles;
GO