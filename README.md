# elshampan-app

Sistema de venta de licor sellado — Avalonia (.NET 8) + MySQL Server. Proyecto de SGBD y Programación Visual.

## Requisitos

- .NET SDK 8.0 o superior (Windows o Linux; la UI corre en ambos)
- MySQL Server + MySQL Workbench

## Puesta en marcha

1. En MySQL Workbench, ejecutar contra `localhost` en este orden:
   - `database/schema.sql` (crea la BD `elshampan_db` y las tablas)
   - `database/seed.sql` (usuarios, productos y proveedores de prueba)
2. Editar `src/ElShampan.App/appsettings.json` y poner la contraseña real de MySQL en `Pwd`.
3. Ejecutar:

   ```
   dotnet run --project src/ElShampan.App
   ```

4. Iniciar sesión con una de las cuentas sembradas:

   | Usuario | Contraseña | Rol |
   |---|---|---|
   | `admin` | `admin123` | admin |
   | `vendedor` | `vendedor123` | vendedor |

## Módulos

- **Login** — sesiones con roles admin/vendedor; contraseñas con PBKDF2-SHA256 (100k iteraciones).
- **Productos** (admin) — CRUD con búsqueda, baja lógica y resaltado de stock mínimo.
- **Punto de venta** — carrito, cobro con transacción MySQL (valida y descuenta stock con `FOR UPDATE`), ticket en PDF.
- **Compras** (admin) — entrada de mercancía por proveedor en transacción (aumenta stock); CRUD de proveedores.
- **Reportes** — ventas por rango de fechas, totales por método de pago, top 5 productos, alertas de stock bajo y gráficas.
- **Usuarios** (admin) — alta/baja y roles.

## Compilación

```
dotnet build
```

## Estructura

```
database/            schema.sql y seed.sql (ejecutar en Workbench)
src/ElShampan.App/
  Data/              Db + repositorios (consultas parametrizadas)
  Models/            entidades
  Services/          AuthService (hash) y Sesion
  Views/             ventanas Avalonia (XAML + code-behind)
  App.axaml          tema Fluent y estilos globales
  appsettings.json   connection string
```
