-- Seed initial data for POS System
-- Insert sample categories
INSERT INTO Categories (Name, Description) VALUES 
('Electronics', 'Electronic devices and accessories'),
('Clothing', 'Apparel and fashion items'),
('Books', 'Books and educational materials'),
('Home & Garden', 'Home improvement and garden supplies'),
('Sports', 'Sports and fitness equipment');

-- Insert sample customers
INSERT INTO Customers (FirstName, LastName, Email, Phone, Address) VALUES 
('John', 'Doe', 'john.doe@email.com', '+1-555-0101', '123 Main St, City, State 12345'),
('Jane', 'Smith', 'jane.smith@email.com', '+1-555-0102', '456 Oak Ave, City, State 12346'),
('Mike', 'Johnson', 'mike.johnson@email.com', '+1-555-0103', '789 Pine Rd, City, State 12347'),
('Sarah', 'Williams', 'sarah.williams@email.com', '+1-555-0104', '321 Elm St, City, State 12348'),
('David', 'Brown', 'david.brown@email.com', '+1-555-0105', '654 Maple Dr, City, State 12349');

-- Insert sample products
INSERT INTO Products (Name, Description, Price, StockQuantity, CategoryId) VALUES 
('Wireless Headphones', 'High-quality wireless Bluetooth headphones', 99.99, 50, 1),
('Smartphone', 'Latest model smartphone with advanced features', 699.99, 25, 1),
('Laptop Computer', 'Professional laptop for work and gaming', 1299.99, 15, 1),
('Cotton T-Shirt', 'Comfortable cotton t-shirt in various colors', 19.99, 100, 2),
('Jeans', 'Classic denim jeans in multiple sizes', 49.99, 75, 2),
('Programming Book', 'Learn modern programming techniques', 39.99, 30, 3),
('Cook Book', 'Delicious recipes for home cooking', 24.99, 40, 3),
('Garden Tools Set', 'Complete set of essential garden tools', 79.99, 20, 4),
('Indoor Plant', 'Beautiful houseplant for home decoration', 15.99, 60, 4),
('Basketball', 'Professional quality basketball', 29.99, 35, 5),
('Running Shoes', 'Comfortable running shoes for athletes', 89.99, 45, 5);
