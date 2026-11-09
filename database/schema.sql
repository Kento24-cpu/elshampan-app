-- ElShampan - schema (run in MySQL Workbench against localhost)
CREATE DATABASE IF NOT EXISTS elshampan_db
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE elshampan_db;

CREATE TABLE usuarios (
  id INT AUTO_INCREMENT PRIMARY KEY,
  nombre_usuario VARCHAR(50) NOT NULL UNIQUE,
  password_hash VARCHAR(200) NOT NULL,
  nombre_completo VARCHAR(100) NOT NULL,
  rol ENUM('admin', 'vendedor') NOT NULL DEFAULT 'vendedor',
  activo TINYINT(1) NOT NULL DEFAULT 1
) ENGINE = InnoDB;

CREATE TABLE productos (
  id INT AUTO_INCREMENT PRIMARY KEY,
  codigo VARCHAR(30) NOT NULL UNIQUE,
  nombre VARCHAR(120) NOT NULL,
  categoria VARCHAR(60) NOT NULL,
  descripcion VARCHAR(255) NULL,
  precio_compra DECIMAL(10, 2) NOT NULL,
  precio_venta DECIMAL(10, 2) NOT NULL,
  stock INT NOT NULL DEFAULT 0,
  stock_minimo INT NOT NULL DEFAULT 5,
  activo TINYINT(1) NOT NULL DEFAULT 1,
  INDEX idx_productos_nombre (nombre)
) ENGINE = InnoDB;

CREATE TABLE proveedores (
  id INT AUTO_INCREMENT PRIMARY KEY,
  nombre VARCHAR(120) NOT NULL,
  telefono VARCHAR(20) NULL,
  email VARCHAR(100) NULL,
  direccion VARCHAR(150) NULL
) ENGINE = InnoDB;

CREATE TABLE ventas (
  id INT AUTO_INCREMENT PRIMARY KEY,
  usuario_id INT NOT NULL,
  fecha DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  subtotal DECIMAL(10, 2) NOT NULL,
  total DECIMAL(10, 2) NOT NULL,
  metodo_pago ENUM('efectivo', 'tarjeta') NOT NULL,
  CONSTRAINT fk_ventas_usuario FOREIGN KEY (usuario_id)
    REFERENCES usuarios (id) ON DELETE RESTRICT,
  INDEX idx_ventas_fecha (fecha)
) ENGINE = InnoDB;

CREATE TABLE ventas_detalles (
  id INT AUTO_INCREMENT PRIMARY KEY,
  venta_id INT NOT NULL,
  producto_id INT NOT NULL,
  cantidad INT NOT NULL,
  precio_unitario DECIMAL(10, 2) NOT NULL,
  subtotal DECIMAL(10, 2) NOT NULL,
  CONSTRAINT fk_det_venta FOREIGN KEY (venta_id)
    REFERENCES ventas (id) ON DELETE RESTRICT,
  CONSTRAINT fk_det_producto FOREIGN KEY (producto_id)
    REFERENCES productos (id) ON DELETE RESTRICT
) ENGINE = InnoDB;

CREATE TABLE compras (
  id INT AUTO_INCREMENT PRIMARY KEY,
  proveedor_id INT NOT NULL,
  usuario_id INT NOT NULL,
  fecha DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  total DECIMAL(10, 2) NOT NULL,
  CONSTRAINT fk_compras_proveedor FOREIGN KEY (proveedor_id)
    REFERENCES proveedores (id) ON DELETE RESTRICT,
  CONSTRAINT fk_compras_usuario FOREIGN KEY (usuario_id)
    REFERENCES usuarios (id) ON DELETE RESTRICT,
  INDEX idx_compras_fecha (fecha)
) ENGINE = InnoDB;

CREATE TABLE compras_detalles (
  id INT AUTO_INCREMENT PRIMARY KEY,
  compra_id INT NOT NULL,
  producto_id INT NOT NULL,
  cantidad INT NOT NULL,
  precio_unitario DECIMAL(10, 2) NOT NULL,
  CONSTRAINT fk_cdet_compra FOREIGN KEY (compra_id)
    REFERENCES compras (id) ON DELETE RESTRICT,
  CONSTRAINT fk_cdet_producto FOREIGN KEY (producto_id)
    REFERENCES productos (id) ON DELETE RESTRICT
) ENGINE = InnoDB;
