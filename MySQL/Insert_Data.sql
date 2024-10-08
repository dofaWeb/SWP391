INSERT INTO Category Values('B0000001','Lenovo');
INSERT INTO Category Values('B1000001','Iphone');


INSERT INTO Product_State (name) VALUES ('Available');
INSERT INTO Product_State (name) VALUES ('Unavailable');
INSERT INTO Product_State (name) VALUES ('Out of Stock');



INSERT INTO Product VALUES ('P0000001', 'Lenovo ideapad S340', 'LenovoIdeapadS340.jpg', 'Lenovo laptop',
	'B0000001', 1);
INSERT INTO Variation VALUES ('Va000001', 'Ram');
INSERT INTO Variation VALUES ('Va000002', 'Storage');

INSERT INTO Variation_Option VALUES ('Vo000001', 'Va000001', '4GB');
INSERT INTO Variation_Option VALUES ('Vo000002', 'Va000001', '8GB');
INSERT INTO Variation_Option VALUES ('Vo000003', 'Va000001', '12GB');
INSERT INTO Variation_Option VALUES ('Vo000004', 'Va000001', '16GB');
INSERT INTO Variation_Option VALUES ('Vo000005', 'Va000001', '32GB');
INSERT INTO Variation_Option VALUES ('Vo000006', 'Va000001', '64GB');

INSERT INTO Variation_Option VALUES ('Vo100001', 'Va000002', '128GB');
INSERT INTO Variation_Option VALUES ('Vo100002', 'Va000002', '256GB');
INSERT INTO Variation_Option VALUES ('Vo100003', 'Va000002', '512GB');
INSERT INTO Variation_Option VALUES ('Vo100004', 'Va000002', '1T');
INSERT INTO Variation_Option VALUES ('Vo100005', 'Va000002', '2T');

INSERT INTO Product_Item VALUES('Pi000001', 'P0000001', 10, 10000000, 12000000, 0);
INSERT INTO Product_Item VALUES('Pi000002', 'P0000001', 10, 15000000, 19000000, 0);


INSERT INTO Product_Configuration(product_item_id, variation_option_id) VALUES('Pi000001', 'Vo000001');
INSERT INTO Product_Configuration(product_item_id, variation_option_id) VALUES('Pi000001', 'Vo100001');

INSERT INTO Product_Configuration(product_item_id, variation_option_id) VALUES('Pi000002', 'Vo000001');
INSERT INTO Product_Configuration(product_item_id, variation_option_id) VALUES('Pi000002', 'Vo100001');