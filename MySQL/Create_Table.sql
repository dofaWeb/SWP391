CREATE TABLE Role_Name (
  id NVARCHAR(8) PRIMARY KEY,
  name VARCHAR(50) NOT NULL
);

CREATE TABLE Account (
  id NVARCHAR(8) PRIMARY KEY,
  username NVARCHAR(50) UNIQUE NOT NULL,
  password NVARCHAR(32) NOT NULL,
  email NVARCHAR(50) NOT NULL,
  phone NVARCHAR(11) NOT NULL,
  is_active BIT NOT NULL,
  role_id NVARCHAR(8) NOT NULL,
  CONSTRAINT FK_Account_Role_Name FOREIGN KEY (role_id) REFERENCES Role_Name(id)
);

CREATE TABLE Staff (
  account_id NVARCHAR(8) PRIMARY KEY,
  name NVARCHAR(50) NOT NULL,
  salary INT NOT NULL,
  CONSTRAINT FK_Staff_Account FOREIGN KEY (account_id) REFERENCES Account(id)
);

CREATE TABLE Category (
  id NVARCHAR(8) PRIMARY KEY,
  name NVARCHAR(50) NOT NULL
);

CREATE TABLE User (
  account_id NVARCHAR(8) PRIMARY KEY,
  name NVARCHAR(50) NOT NULL,
  point INT NOT NULL,
  CONSTRAINT FK_User_Account FOREIGN KEY (account_id) REFERENCES Account(id)
);

DROP TABLE IF EXISTS User_Address;
DROP TABLE IF EXISTS Address;
DROP TABLE IF EXISTS Province;

ALTER TABLE User
ADD province NVARCHAR(50),
ADD district NVARCHAR(50),
ADD address NVARCHAR(100);


CREATE TABLE Province (
  id NVARCHAR(8) PRIMARY KEY,
  name NVARCHAR(50) NOT NULL
);

CREATE TABLE Address (
  id NVARCHAR(8) PRIMARY KEY,
  province_id NVARCHAR(8),
  address NVARCHAR(100) NOT NULL,
  CONSTRAINT FK_Address_Province FOREIGN KEY (province_id) REFERENCES Province(id)
);

CREATE TABLE User_Address (
  user_id NVARCHAR(8) PRIMARY KEY,
  address_id NVARCHAR(8),
  is_default BIT NOT NULL,
  CONSTRAINT FK_User_Address_User FOREIGN KEY (user_id) REFERENCES User(account_id),
  CONSTRAINT FK_User_Address_Address FOREIGN KEY (address_id) REFERENCES Address(id)
);

CREATE TABLE Product_State (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(50) NOT NULL
);

CREATE TABLE Product (
  id NVARCHAR(8) PRIMARY KEY,
  name NVARCHAR(100) NOT NULL,
  picture NVARCHAR(255),
  description NVARCHAR(255),
  category_id NVARCHAR(8) NOT NULL,
  state_id INT NOT NULL,
  CONSTRAINT FK_Product_Category FOREIGN KEY (category_id) REFERENCES Category(id),
  CONSTRAINT FK_Product_State FOREIGN KEY (state_id) REFERENCES Product_State(id)
);

CREATE TABLE Product_Item (
  id NVARCHAR(8) PRIMARY KEY,
  product_id NVARCHAR(8) NOT NULL,
  quantity INT NOT NULL,
  import_price DECIMAL(10, 2) NOT NULL,
  selling_price DECIMAL(10, 2),
  discount DECIMAL(5, 2),
  CONSTRAINT FK_Product_Item_Product FOREIGN KEY (product_id) REFERENCES Product(id)
);

CREATE TABLE Variation (
  id NVARCHAR(8) PRIMARY KEY,
  name NVARCHAR(50) NOT NULL
);

CREATE TABLE Variation_Option (
  id NVARCHAR(8) PRIMARY KEY,
  variation_id NVARCHAR(8) NOT NULL,
  value NVARCHAR(50) NOT NULL,
  CONSTRAINT FK_Variation_Option_Variation FOREIGN KEY (variation_id) REFERENCES Variation(id)
);

CREATE TABLE Product_Configuration (
  id int AUTO_INCREMENT PRIMARY KEY,
  product_item_id NVARCHAR(8) NOT NULL,
  variation_option_id NVARCHAR(8) NOT NULL,	
  CONSTRAINT FK_Product_Configuration_Product_Item FOREIGN KEY (product_item_id) REFERENCES Product_Item(id),
  CONSTRAINT FK_Product_Configuration_Variation_Option FOREIGN KEY (variation_option_id) REFERENCES Variation_Option(id)
);




/*CREATE TABLE Staff_Shift (
  id NVARCHAR(8) PRIMARY KEY,
  staff_id NVARCHAR(8) NOT NULL,
  shift_time_begin DATETIME NOT NULL,
  shift_time_end DATETIME NOT NULL,
  CONSTRAINT FK_Staff_Shift_Staff FOREIGN KEY (staff_id) REFERENCES Staff(account_id)
);

ALTER TABLE Staff_Shift
DROP COLUMN shift_time_begin,
DROP COLUMN shift_time_end;

ALTER TABLE Staff_Shift
ADD `shift` NVARCHAR(50),
ADD `day` DATE,
Add hourly_rate DECIMAL(10,2);*/

CREATE TABLE Staff_Shift(
	id nvarchar(8) PRIMARY KEY,
	staff_id nvarchar(8),
	`shift` nvarchar(50),
	`date` DATE,
	hourly_rate decimal(10,2)
);

CREATE TABLE Order_State (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name NVARCHAR(50) NOT NULL
);

CREATE TABLE `Order` (
  id NVARCHAR(8) PRIMARY KEY,
  user_id NVARCHAR(8) NOT NULL,
  address NVARCHAR(100) NOT NULL,
  state_id INT NOT NULL,
  date DATETIME NOT NULL,
  use_point DECIMAL(10, 2) NOT NULL,
  earn_point DECIMAL(10, 2) NOT NULL,
  staff_shift_id NVARCHAR(8) NOT NULL,
  CONSTRAINT FK_Order_User FOREIGN KEY (user_id) REFERENCES `User`(account_id),
  CONSTRAINT FK_Order_Staff_Shift FOREIGN KEY (staff_shift_id) REFERENCES Staff_Shift(id),
  CONSTRAINT FK_Order_State FOREIGN KEY (state_id) REFERENCES Order_State(id)
);

CREATE TABLE Order_Item (
  id NVARCHAR(8) PRIMARY KEY,
  order_id NVARCHAR(8) NOT NULL,
  product_item_id NVARCHAR(8) NOT NULL,
  quantity INT NOT NULL,
  price DECIMAL(10, 2) NOT NULL,
  discount DECIMAL(5, 2),
  CONSTRAINT FK_Order_Item_Order FOREIGN KEY (order_id) REFERENCES `Order`(id),
  CONSTRAINT FK_Order_Item_Product_Item FOREIGN KEY (product_item_id) REFERENCES Product_Item(id)
);

CREATE TABLE Product_Name_Log (
  id NVARCHAR(8) PRIMARY KEY,
  product_item_id NVARCHAR(8) NOT NULL,
  old_name NVARCHAR(50) NOT NULL,
  new_name NVARCHAR(50) NOT NULL,
  change_timestamp DATETIME NOT NULL,
  CONSTRAINT FK_Name_Log_Product_Item FOREIGN KEY (product_item_id) REFERENCES Product_Item(id)
);

CREATE TABLE Quantity_Log (
  id NVARCHAR(8) PRIMARY KEY,
  product_item_id NVARCHAR(8) NOT NULL,
  old_quantity INT NOT NULL,
  new_quantity INT NOT NULL,
  change_timestamp DATETIME NOT NULL,
  CONSTRAINT FK_Quantity_Log_Product_Item FOREIGN KEY (product_item_id) REFERENCES Product_Item(id)
);

CREATE TABLE Price_Log (
  id NVARCHAR(8) PRIMARY KEY,
  product_item_id NVARCHAR(8) NOT NULL,
  old_price DECIMAL(10, 2) NOT NULL,
  new_price DECIMAL(10, 2) NOT NULL,
  change_timestamp DATETIME NOT NULL,
  CONSTRAINT FK_Price_Log_Product_Item FOREIGN KEY (product_item_id) REFERENCES Product_Item(id)
);

CREATE TABLE Discount_Log (
  id NVARCHAR(8) PRIMARY KEY,
  product_item_id NVARCHAR(8) NOT NULL,
  old_discount DECIMAL(10, 2) NOT NULL,
  new_discount DECIMAL(10, 2) NOT NULL,
  change_timestamp DATETIME NOT NULL,
  CONSTRAINT FK_Discount_Log_Product_Item FOREIGN KEY (product_item_id) REFERENCES Product_Item(id)
);

CREATE TABLE Change_Reason (
  id NVARCHAR(8) PRIMARY KEY,
  reason NVARCHAR(50)
);

CREATE TABLE Product_Log (
  id NVARCHAR(8) PRIMARY KEY,
  name_log_id NVARCHAR(8),
  quantity_log_id NVARCHAR(8),
  price_log_id NVARCHAR(8),
  disocunt_log_id NVARCHAR(8),
  change_reason_id NVARCHAR(8),
  CONSTRAINT FK_Product_Log_Product_Name_Log FOREIGN KEY (name_log_id) REFERENCES Product_Name_Log(id),
  CONSTRAINT FK_Product_Log_Quantity_Log FOREIGN KEY (quantity_log_id) REFERENCES Quantity_Log(id),
  CONSTRAINT FK_Product_Log_Price_Log FOREIGN KEY (price_log_id) REFERENCES Price_Log(id),
  CONSTRAINT FK_Product_Log_Discount_Log FOREIGN KEY (disocunt_log_id) REFERENCES Discount_Log(id),
  CONSTRAINT FK_Product_Log_Change_Reason FOREIGN KEY (change_reason_id) REFERENCES Change_Reason(id)
);

CREATE TABLE Rating (
  id NVARCHAR(8) PRIMARY KEY,
  user_id NVARCHAR(8) NOT NULL,
  product_item_id NVARCHAR(8) NOT NULL,
  rating INT NOT NULL,
  CONSTRAINT FK_Rating_User FOREIGN KEY (user_id) REFERENCES `User`(account_id),
  CONSTRAINT FK_Rating_Product_Item FOREIGN KEY (product_item_id) REFERENCES Product_Item(id)
);

CREATE TABLE Comment (
  id NVARCHAR(8) PRIMARY KEY,
  user_id NVARCHAR(8) NOT NULL,
  product_item_id NVARCHAR(8) NOT NULL,
  comment NVARCHAR(255),
  date DATETIME NOT NULL,
  CONSTRAINT FK_Comment_User FOREIGN KEY (user_id) REFERENCES `User`(account_id),
  CONSTRAINT FK_Comment_Product_Item FOREIGN KEY (product_item_id) REFERENCES Product_Item(id)
);