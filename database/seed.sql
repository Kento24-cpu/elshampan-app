-- ElShampan - seed data (run after schema.sql in MySQL Workbench)
USE elshampan_db;

-- users: admin / admin123 and vendedor / vendedor123 (PBKDF2-SHA256, 100k iterations, salt:hash hex)
INSERT INTO usuarios (nombre_usuario, password_hash, nombre_completo, rol, activo) VALUES
  ('admin', '6ee1e688973c6ca79c243cd92ee0a815:d0df756d2fd0f297d342c7291d40134caf0cdcd6d0c31620c739358d4575cad9', 'Administrador', 'admin', 1),
  ('vendedor', '4c7dcf7f9516bf8fd4f88c1fdd8be442:508b0950c6fe9ffe82c310f255820510d86e7ac37f9b5b256e8d897f522b1795', 'Vendedor General', 'vendedor', 1);

INSERT INTO productos (codigo, nombre, categoria, descripcion, precio_compra, precio_venta, stock, stock_minimo, activo) VALUES
  ('LIC-001', 'Tequila Don Julio 70 750ml', 'Tequilas', 'Blanco cristalino, botella sellada', 850.00, 1250.00, 12, 4, 1),
  ('LIC-002', 'Whisky Johnnie Walker Black 750ml', 'Whiskys', 'Blend 12 anos, botella sellada', 780.00, 1150.00, 10, 4, 1),
  ('LIC-003', 'Ron Bacardi Carta Blanca 750ml', 'Rones', 'Blanco, botella sellada', 320.00, 480.00, 20, 5, 1),
  ('LIC-004', 'Vodka Absolut 750ml', 'Vodkas', 'Original, botella sellada', 390.00, 590.00, 15, 5, 1),
  ('LIC-005', 'Gin Bombay Sapphire 750ml', 'Ginebras', 'London dry, botella sellada', 520.00, 780.00, 8, 4, 1),
  ('LIC-006', 'Vino Campo Viejo Reserva 750ml', 'Vinos', 'Tempranillo, botella sellada', 260.00, 420.00, 24, 6, 1),
  ('LIC-007', 'Cerveza Corona 355ml', 'Cervezas', 'Pack 6 botellas selladas', 95.00, 145.00, 48, 12, 1),
  ('LIC-008', 'Mezcal Montelobos 750ml', 'Mezcales', 'Joven, botella sellada', 640.00, 950.00, 6, 4, 1);

INSERT INTO proveedores (nombre, telefono, email, direccion) VALUES
  ('Bebidas del Norte S.A. de C.V.', '55-1234-5678', 'ventas@bebnorte.mx', 'Av. Industrial 456, Monterrey'),
  ('Distribuidora La Central', '55-8765-4321', 'pedidos@lacentral.mx', 'Calle Reposo 12, CDMX');
