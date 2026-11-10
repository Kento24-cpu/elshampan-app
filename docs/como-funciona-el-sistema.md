# Cómo funciona ElShampan — Guion para exponer

> Documento de referencia no técnica. Sirve como guion para una exposición oral del sistema.

---

## 1. ¿Qué es ElShampan?

ElShampan es un **sistema de punto de venta para un negocio de venta de licor sellado**. Es una aplicación de escritorio (se instala en las computadoras del negocio) que reemplaza el cuaderno y la calculadora: lleva el control de productos, registra cada venta, genera el ticket, controla el inventario y produce reportes de cómo va el negocio.

Proyecto académico desarrollado para las materias de SGBD y Programación Visual.

---

## 2. ¿Para quién es?

Dos tipos de usuarios, con permisos distintos:

- **Admin (administrador):** ve todo. Da de alta productos y proveedores, registra compras de mercancía, crea cuentas de empleados y consulta reportes.
- **Vendedor (cajero):** solo ve lo que necesita para trabajar: el **punto de venta** y los **reportes**. Puede vender, pero no toca el inventario ni los usuarios.

El menú se adapta a quien inicie sesión: cada usuario solo ve los botones que le corresponden.

---

## 3. Un día típico de uso

1. **Inicio de sesión.** El empleado entra con su usuario y contraseña. Si está mal, el sistema no lo deja pasar.
2. **Menú principal.** Aparece el menú lateral con los módulos disponibles según su rol, y el botón de cerrar sesión.
3. **Vender (punto de venta):**
   - Busca el producto por nombre o código.
   - Lo agrega al carrito con la cantidad (el sistema no deja vender más de lo que hay en stock).
   - Elige el método de pago: **efectivo** o **tarjeta**.
   - Presiona **COBRAR**. El sistema descuenta el inventario, registra la venta y abre el **ticket**, que se puede imprimir o guardar como PDF (formato de rollo de impresora, 80 mm).
4. **Reponer mercancía (solo admin):** en *Compras*, elige el proveedor, agrega los productos con su precio de compra y registra la compra. El stock sube automáticamente.
5. **Revisar cómo va el negocio:** en *Reportes*, consulta cualquier rango de fechas (por ejemplo, los últimos 30 días).
6. **Cerrar sesión** al terminar la jornada.

---

## 4. Las pantallas del sistema

| Pantalla | Qué se hace ahí | Quién la usa |
|---|---|---|
| **Login** | Entrar al sistema con usuario y contraseña | Todos |
| **Menú principal** | Elegir el módulo; ver usuario y rol activo | Todos |
| **Punto de venta** | Cobrar ventas: carrito, método de pago, botón COBRAR | Admin y vendedor |
| **Ticket** | Ver, imprimir o guardar en PDF el ticket de la venta | Admin y vendedor |
| **Productos** | Dar de alta/editar productos, buscar, ver stock bajo | Solo admin |
| **Compras** | Registrar mercancía entrante de proveedores | Solo admin |
| **Proveedores** | Administrar la ficha de los distribuidores | Solo admin |
| **Reportes** | Ventas por fecha, gráficas, top productos, alertas | Admin y vendedor |
| **Usuarios** | Crear o desactivar cuentas de empleados | Solo admin |

---

## 5. Qué información guarda el sistema

Todo vive en una base de datos MySQL con siete tablas, que se pueden explicar así:

- **Usuarios:** quién trabaja en el sistema, su rol y su contraseña (guardada de forma segura, no en texto plano).
- **Productos:** el catálogo de bebidas: nombre, categoría, precio de compra, precio de venta, stock actual y stock mínimo.
- **Proveedores:** ficha de los distribuidores (nombre, teléfono, correo, dirección).
- **Ventas + detalle de ventas:** cada venta con su fecha, vendedor, método de pago, total, y qué productos se llevaron.
- **Compras + detalle de compras:** cada reposición de mercancía con su proveedor y total.

Dato clave: **el stock se mueve en dos direcciones** — baja con cada venta, sube con cada compra — y siempre de forma atómica (o se completa la operación entera, o no se hace nada). Nunca queda un número a medias.

---

## 6. Qué muestra la pantalla de reportes

Al elegir un rango de fechas y consultar:

1. **Lista de ventas del periodo:** folio, fecha, vendedor, método de pago y total.
2. **Resumen:** total general, cuánto entró en efectivo y cuánto en tarjeta.
3. **Gráfica de columnas:** ingresos por día.
4. **Gráfica de pastel:** proporción efectivo vs. tarjeta.
5. **Top 5 productos:** los que más se vendieron.
6. **Alerta de stock bajo:** productos que llegaron a su mínimo (se pintan de naranja también en la pantalla de Productos).

---

## 7. Detalles que conviene destacar en la exposición

- **Control de inventario en tiempo real:** la venta no se completa si no hay stock; el sistema bloquea la última unidad para que no se vendan dos veces a la vez.
- **Alerta de stock mínimo:** cada producto define un mínimo (5 por defecto). Al llegar a él, el sistema avisa en naranja para reponer a tiempo.
- **Precios duales:** cada producto guarda lo que le cuesta al negocio (precio de compra) y lo que cobra al cliente (precio de venta), permitiendo ver el margen.
- **Ticket real en PDF:** formato de rollo de 80 mm, listo para impresora térmica.
- **Seguridad:** contraseñas con hash (PBKDF2), consultas protegidas contra inyección SQL, y usuarios y productos nunca se borran físicamente: se desactivan para conservar el histórico de ventas.
- **Permisos por rol:** el vendedor no puede tocar inventario ni usuarios; el menú ni siquiera le muestra esas opciones.
- **Integridad referencial:** no se puede borrar un proveedor que ya tenga compras registradas, ni un producto con ventas; los datos del histórico se protegen.

---

## 8. La tecnología, en una frase

- **Interfaz:** Avalonia sobre .NET 8 — escritorio multiplataforma (funciona en Windows, Linux y macOS).
- **Base de datos:** MySQL.
- **Extras:** gráficas de reportes con LiveCharts, tickets en PDF con QuestPDF.

---

## Preguntas típicas de la exposición y respuestas cortas

- **¿Qué pasa si pierdo conexión a la base de datos?** El login avisa "no hay conexión" y no deja entrar; no se pierden datos porque nada se guarda a medias.
- **¿Puede un vendedor eliminar productos?** No. Esos botones ni le aparecen en el menú.
- **¿Borra el histórico si elimino un producto?** No. Se marca como inactivo; las ventas anteriores se conservan.
- **¿Cómo sabes cuándo reponer stock?** La alerta de stock mínimo en Reportes y el resaltado naranja en Productos.
- **¿Se puede pagar en partes o a crédito?** No. Solo efectivo y tarjeta.
