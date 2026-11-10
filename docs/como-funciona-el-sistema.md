# Cómo funciona ElShampan — Guion para exponer

> Documento de referencia para una exposición oral del sistema. Explica el funcionamiento general y cómo se conecta el código con los scripts de la base de datos.

---

## 1. ¿Qué es ElShampan?

ElShampan es un **sistema de punto de venta para un negocio de venta de licor sellado**. Es una aplicación de escritorio (se instala en las computadoras del negocio) que reemplaza el cuaderno y la calculadora: lleva el control de productos, registra cada venta, genera el ticket, controla el inventario y produce reportes de cómo va el negocio.

Proyecto académico desarrollado para las materias de SGBD y Programación Visual.

---

## 2. ¿Para quién es?

Dos tipos de usuarios, con permisos distintos:

- **Admin (administrador):** ve todo. Da de alta productos y proveedores, registra compras de mercancía, crea cuentas de empleados, consulta reportes y anula cualquier venta.
- **Vendedor (cajero):** solo ve lo que necesita para trabajar: **punto de venta**, **reportes** y **ventas del día**. Puede vender y anular sus propias ventas, pero no toca el inventario ni los usuarios.

El menú se adapta a quien inicia sesión: cada usuario solo ve los botones que le corresponden (control en `MainWindow.axaml.cs`, con `Sesion.Actual.EsAdmin`).

---

## 3. Un día típico de uso

1. **Inicio de sesión.** El empleado entra con su usuario y contraseña. Si está mal, el sistema no lo deja pasar.
2. **Pantalla de inicio.** En lugar de un espacio vacío, aparece el **resumen del día**: total y cantidad de ventas de hoy, productos con stock bajo y los más vendidos. Para el vendedor muestra solo **sus** ventas; se refresca cada vez que se cierra un módulo.
3. **Vender (punto de venta):**
   - Busca el producto por nombre o código.
   - Lo agrega al carrito con la cantidad (el sistema no deja vender más de lo que hay en stock).
   - Elige el método de pago: **efectivo** o **tarjeta**.
   - Presiona **COBRAR**: aparece una **confirmación** con el total antes de registrar nada (evita cobros accidentales). Confirmado, el sistema descuenta el inventario, registra la venta y abre el **ticket**, que se puede imprimir o guardar como PDF (formato de rollo de 80 mm).
4. **Corregir un error (ventas del día):** en *Ventas del día* se revisa la lista de ventas de una fecha, se vuelve a abrir cualquier ticket para reimprimirlo y se puede **anular** una venta: el stock vuelve automáticamente y la venta queda marcada como anulada (el vendedor solo las suyas, el admin cualquiera).
5. **Reponer mercancía (solo admin):** en *Compras*, elige el proveedor, agrega los productos con su precio de compra y registra la compra. El stock sube automáticamente.
6. **Revisar cómo va el negocio:** en *Reportes*, consulta cualquier rango de fechas; el checkbox *Mis ventas* filtra por el vendedor actual (activado por defecto para el cajero).
7. **Cerrar sesión** al terminar la jornada.

---

## 4. Las pantallas del sistema

| Pantalla | Qué se hace ahí | Quién la usa | Código |
|---|---|---|---|
| **Login** | Entrar con usuario y contraseña | Todos | `Views/LoginWindow.axaml(.cs)` |
| **Inicio (menú principal)** | Resumen del día, menú según rol, cerrar sesión | Todos | `Views/MainWindow.axaml(.cs)` |
| **Punto de venta** | Carrito, método de pago, confirmación y COBRAR | Admin y vendedor | `Views/PuntoVentaWindow.axaml(.cs)` |
| **Ticket** | Ver, imprimir o guardar en PDF (80 mm) | Admin y vendedor | `Views/TicketWindow.axaml(.cs)` |
| **Ventas del día** | Historial por fecha: reimprimir ticket y anular ventas | Admin y vendedor | `Views/HistorialVentasWindow.axaml(.cs)` |
| **Productos** | Dar de alta/editar productos, buscar, resaltado de stock bajo | Solo admin | `Views/ProductosWindow.axaml(.cs)` |
| **Compras** | Registrar mercancía entrante de proveedores | Solo admin | `Views/ComprasWindow.axaml(.cs)` |
| **Proveedores** | Administrar la ficha de los distribuidores | Solo admin | `Views/ProveedoresWindow.axaml(.cs)` |
| **Reportes** | Ventas por fecha, filtro "mis ventas", gráficas, top productos, alertas | Admin y vendedor | `Views/ReportesWindow.axaml(.cs)` |
| **Usuarios** | Crear o desactivar cuentas de empleados | Solo admin | `Views/UsuariosWindow.axaml(.cs)` |

---

## 5. Qué información guarda el sistema

Todo vive en una base de datos MySQL con **siete tablas** (script `database/schema.sql`, datos de ejemplo en `database/seed.sql`):

- **usuarios:** quién trabaja en el sistema, su rol (`admin`/`vendedor`) y su contraseña (hash PBKDF2, nunca en texto plano).
- **productos:** catálogo de bebidas: nombre, categoría, precio de compra, precio de venta, stock actual y stock mínimo.
- **proveedores:** ficha de los distribuidores (nombre, teléfono, correo, dirección).
- **ventas + ventas_detalles:** cada venta con fecha, vendedor, método de pago, total, si está **anulada**, y qué productos se llevaron.
- **compras + compras_detalles:** cada reposición de mercancía con su proveedor y total.

Dato clave: **el stock se mueve en dos direcciones** — baja con cada venta, sube con cada compra, y vuelve a subir si se anula una venta — y siempre de forma atómica (o se completa la operación entera, o no se hace nada).

---

## 6. Cómo se conecta el código con la base de datos

La aplicación tiene tres capas visibles en `src/ElShampan.App/`:

**a) Capa de datos (`Data/`)** — un repositorio por tabla, con consultas SQL parametrizadas (MySqlConnector):

| Archivo | Tabla(s) | Qué hace |
|---|---|---|
| `Db.cs` | — | Abre la conexión; la cadena vive en `appsettings.json` |
| `UsuarioRepository.cs` | usuarios | Alta, búsqueda y login (`AuthService.cs` verifica con PBKDF2, 100 000 iteraciones) |
| `ProductoRepository.cs` | productos | Búsqueda para el punto de venta, alta/edición, lista de stock bajo |
| `ProveedorRepository.cs` | proveedores | CRUD de distribuidores |
| `VentaRepository.cs` | ventas, ventas_detalles | `Registrar`: transacción que valida stock con bloqueo `FOR UPDATE`, inserta la venta y descuenta el inventario. `Anular`: transacción que devuelve el stock y marca `anulada = 1`. `Obtener`: carga venta + detalle para reimprimir el ticket |
| `CompraRepository.cs` | compras, compras_detalles | Transacción que registra la compra y **sube** el stock |
| `ReporteRepository.cs` | ventas (+ joins) | Ventas por rango de fechas (con filtro por vendedor y exclusiones de anuladas), top productos e historial |

**b) Vistas (`Views/`)** — ventanas Avalonia (`.axaml` + `.axaml.cs`). Se abren como diálogos desde `MainWindow`; cada una llama al repositorio correspondiente y muestra los errores con mensajes traducidos (`Services/Dialogos.cs`).

**c) Servicios (`Services/`)** — `Sesion` (usuario actual y rol), `AuthService` (hash/verificación de contraseñas), `Dialogos` (avisos, errores de base de datos y confirmaciones).

**Scripts de la base de datos (`database/`):**

- **`schema.sql`:** crea las 7 tablas con claves foráneas (`ON DELETE RESTRICT`), **constraints `CHECK`** (montos ≥ 0, stock ≥ 0, cantidad > 0, subtotal = cantidad × precio), los **procedimientos almacenados** `sp_ajustar_stock` y `sp_anular_venta` (cada uno con transacción y `ROLLBACK` ante error) y los **triggers** `trg_ventas_montos_validos` y `trg_detalle_subtotal_valido`, que rechazan inserciones inválidas desde cualquier programa que escriba en la base. Incluye el `ALTER` pendiente para bases existentes (`ventas.anulada`).
- **`seed.sql`:** datos de ejemplo (usuarios, productos, proveedores) para levantar el demo.

---

## 7. Qué muestra la pantalla de reportes

Al elegir un rango de fechas y consultar:

1. **Lista de ventas del periodo:** folio, fecha, vendedor, método de pago y total (las anuladas quedan fuera).
2. **Resumen:** total general, cuánto entró en efectivo y cuánto en tarjeta.
3. **Gráfica de columnas:** ingresos por día.
4. **Gráfica de pastel:** proporción efectivo vs. tarjeta.
5. **Top 5 productos:** los que más se vendieron.
6. **Alerta de stock bajo:** productos que llegaron a su mínimo (en naranja).
7. **Filtro "Mis ventas":** muestra solo las ventas del usuario que inició sesión; activado por defecto para el vendedor.

---

## 8. Detalles que conviene destacar en la exposición

- **Control de inventario en tiempo real:** la venta no se completa si no hay stock; el `SELECT ... FOR UPDATE` dentro de la transacción bloquea las filas para que dos cajeros no vendan la última unidad a la vez.
- **Confirmación antes de cobrar:** un clic accidental no registra nada; primero pregunta, después transacción.
- **Anulación con devolución de stock:** `VentaRepository.Anular` devuelve el stock y marca `anulada = 1` en la misma transacción; el histórico nunca se borra y los reportes excluyen las anuladas.
- **Alerta de stock mínimo:** cada producto define un mínimo (5 por defecto); avisa en naranja en Productos, en Reportes y en el resumen del inicio.
- **Precios duales:** cada producto guarda lo que le cuesta al negocio (precio de compra) y lo que cobra al cliente (precio de venta).
- **Ticket real en PDF:** formato de rollo de 80 mm, reimpresible desde Ventas del día.
- **Seguridad:** contraseñas con hash PBKDF2, consultas SQL parametrizadas, y usuarios y productos nunca se borran físicamente: se desactivan para conservar el histórico.
- **Protecciones en la base de datos:** constraints `CHECK` + triggers que validan montos y subtotales aunque la escritura venga de fuera de la app; claves foráneas con `RESTRICT`; transacciones con *rollback* en C# (`Registrar`, `Anular`, `CompraRepository`).
- **Permisos por rol:** el menú ni siquiera muestra las opciones de inventario o usuarios al vendedor; además, este solo puede anular sus propias ventas.
- **Validaciones de entrada:** el largo de cada texto coincide con el tamaño de su columna, y se validan costos, correos, selecciones y método de pago antes de tocar la base.

---

## 9. La tecnología, en una frase

- **Interfaz:** Avalonia sobre .NET 8 — escritorio multiplataforma (Windows, Linux y macOS).
- **Base de datos:** MySQL, con esquema, procedimientos, triggers y seed en `database/`.
- **Extras:** gráficas de reportes con LiveCharts, tickets en PDF con QuestPDF.

---

## Preguntas típicas de la exposición y respuestas cortas

- **¿Qué pasa si pierdo conexión a la base de datos?** El login avisa "no hay conexión" y no deja entrar; nada se guarda a medias porque cada operación es una transacción.
- **¿Puede un vendedor eliminar productos?** No. Esos botones ni le aparecen en el menú.
- **¿Y anular ventas?** Solo las suyas, con confirmación; el admin anula cualquier venta.
- **¿Qué pasa con una venta anulada?** El stock vuelve, queda marcada como anulada (no se borra) y desaparece de los totales de reportes.
- **¿Borra el histórico si elimino un producto?** No. Se marca como inactivo; las ventas anteriores se conservan.
- **¿Cómo sabes cuándo reponer stock?** La alerta de stock mínimo en Reportes, en el resumen del inicio y el resaltado naranja en Productos.
- **¿Se puede pagar en partes o a crédito?** No. Solo efectivo y tarjeta.
- **¿Y si el programador escribe directo en la base?** Los `CHECK` y los triggers rechazan montos negativos, cantidades inválidas y subtotales que no cuadran.
