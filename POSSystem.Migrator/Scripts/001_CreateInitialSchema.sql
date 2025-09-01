-- Initial POS System Database Schema
-- Categories Table
CREATE TABLE Categories (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(100) NOT NULL,
    Description nvarchar(500) NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE()
);

-- Products Table
CREATE TABLE Products (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(200) NOT NULL,
    Description nvarchar(1000) NULL,
    Price decimal(18,2) NOT NULL,
    StockQuantity int NOT NULL DEFAULT 0,
    CategoryId int NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

-- Customers Table
CREATE TABLE Customers (
    Id int IDENTITY(1,1) PRIMARY KEY,
    FirstName nvarchar(100) NOT NULL,
    LastName nvarchar(100) NOT NULL,
    Email nvarchar(255) NULL,
    Phone nvarchar(20) NULL,
    Address nvarchar(500) NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE()
);

-- Orders Table
CREATE TABLE Orders (
    Id int IDENTITY(1,1) PRIMARY KEY,
    CustomerId int NOT NULL,
    TotalAmount decimal(18,2) NOT NULL,
    Status int NOT NULL DEFAULT 0, -- 0: Pending, 1: Processing, 2: Completed, 3: Cancelled
    PaymentMethod nvarchar(50) NULL,
    PaymentProcessed bit NOT NULL DEFAULT 0,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
);

-- OrderItems Table
CREATE TABLE OrderItems (
    Id int IDENTITY(1,1) PRIMARY KEY,
    OrderId int NOT NULL,
    ProductId int NOT NULL,
    Quantity int NOT NULL,
    UnitPrice decimal(18,2) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- Indexes for performance
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_Orders_CustomerId ON Orders(CustomerId);
CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
CREATE INDEX IX_OrderItems_ProductId ON OrderItems(ProductId);
CREATE UNIQUE INDEX IX_Customers_Email ON Customers(Email) WHERE Email IS NOT NULL;
