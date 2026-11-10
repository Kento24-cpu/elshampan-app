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
  precio_compra DECIMAL(10, 2) NOT NULL CHECK (precio_compra >= 0),
  precio_venta DECIMAL(10, 2) NOT NULL CHECK (precio_venta > 0),
  stock INT NOT NULL DEFAULT 0 CHECK (stock >= 0),
  stock_minimo INT NOT NULL DEFAULT 5 CHECK (stock_minimo >= 0),
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
  subtotal DECIMAL(10, 2) NOT NULL CHECK (subtotal >= 0),
  total DECIMAL(10, 2) NOT NULL CHECK (total >= 0),
  metodo_pago ENUM('efectivo', 'tarjeta') NOT NULL,
  anulada TINYINT(1) NOT NULL DEFAULT 0,
  CONSTRAINT fk_ventas_usuario FOREIGN KEY (usuario_id)
    REFERENCES usuarios (id) ON DELETE RESTRICT,
  INDEX idx_ventas_fecha (fecha)
) ENGINE = InnoDB;
-- Base existente: ALTER TABLE ventas ADD COLUMN anulada TINYINT(1) NOT NULL DEFAULT 0;

CREATE TABLE ventas_detalles (
  id INT AUTO_INCREMENT PRIMARY KEY,
  venta_id INT NOT NULL,
  producto_id INT NOT NULL,
  cantidad INT NOT NULL CHECK (cantidad > 0),
  precio_unitario DECIMAL(10, 2) NOT NULL CHECK (precio_unitario >= 0),
  subtotal DECIMAL(10, 2) NOT NULL CHECK (subtotal >= 0),
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
  total DECIMAL(10, 2) NOT NULL CHECK (total >= 0),
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
  cantidad INT NOT NULL CHECK (cantidad > 0),
  precio_unitario DECIMAL(10, 2) NOT NULL CHECK (precio_unitario >= 0),
  CONSTRAINT fk_cdet_compra FOREIGN KEY (compra_id)
    REFERENCES compras (id) ON DELETE RESTRICT,
  CONSTRAINT fk_cdet_producto FOREIGN KEY (producto_id)
    REFERENCES productos (id) ON DELETE RESTRICT
) ENGINE = InnoDB;

-- Stored procedures (cada una con transaccion + ROLLBACK ante error)
DELIMITER $$

DROP PROCEDURE IF EXISTS sp_ajustar_stock $$
CREATE PROCEDURE sp_ajustar_stock(
  IN p_producto_id INT,
  IN p_delta INT
)
BEGIN
  DECLARE v_stock INT;
  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    ROLLBACK;
    RESIGNAL;
  END;

  START TRANSACTION;
  SELECT stock INTO v_stock FROM productos WHERE id = p_producto_id FOR UPDATE;
  IF IFNULL(v_stock, 0) + p_delta < 0 THEN
    SIGNAL SQLSTATE '45000'
      SET MESSAGE_TEXT = 'El ajuste dejaria el stock en negativo';
  END IF;
  UPDATE productos SET stock = stock + p_delta WHERE id = p_producto_id;
  COMMIT;
END$$

DROP PROCEDURE IF EXISTS sp_anular_venta $$
CREATE PROCEDURE sp_anular_venta(IN p_venta_id INT)
BEGIN
  DECLARE v_fin INT DEFAULT 0;
  DECLARE v_producto_id INT;
  DECLARE v_cantidad INT;
  DECLARE cur CURSOR FOR
    SELECT producto_id, cantidad FROM ventas_detalles WHERE venta_id = p_venta_id;
  DECLARE CONTINUE HANDLER FOR NOT FOUND SET v_fin = 1;
  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    ROLLBACK;
    RESIGNAL;
  END;

  START TRANSACTION;
  OPEN cur;
  loop_detalles: LOOP
    FETCH cur INTO v_producto_id, v_cantidad;
    IF v_fin = 1 THEN
      LEAVE loop_detalles;
    END IF;
    UPDATE productos SET stock = stock + v_cantidad WHERE id = v_producto_id;
  END LOOP;
  CLOSE cur;
  DELETE FROM ventas_detalles WHERE venta_id = p_venta_id;
  DELETE FROM ventas WHERE id = p_venta_id;
  COMMIT;
END$$

-- Triggers (validacion liviana, sin efectos secundarios)
DROP TRIGGER IF EXISTS trg_ventas_montos_validos $$
CREATE TRIGGER trg_ventas_montos_validos BEFORE INSERT ON ventas
FOR EACH ROW
BEGIN
  IF NEW.subtotal < 0 OR NEW.total < 0 THEN
    SIGNAL SQLSTATE '45000'
      SET MESSAGE_TEXT = 'Los montos de la venta no pueden ser negativos';
  END IF;
END$$

DROP TRIGGER IF EXISTS trg_detalle_subtotal_valido $$
CREATE TRIGGER trg_detalle_subtotal_valido BEFORE INSERT ON ventas_detalles
FOR EACH ROW
BEGIN
  IF NEW.cantidad <= 0 THEN
    SIGNAL SQLSTATE '45000'
      SET MESSAGE_TEXT = 'La cantidad debe ser mayor que cero';
  END IF;
  IF NEW.subtotal <> NEW.cantidad * NEW.precio_unitario THEN
    SIGNAL SQLSTATE '45000'
      SET MESSAGE_TEXT = 'El subtotal no coincide con cantidad por precio unitario';
  END IF;
END$$

DELIMITER ;
