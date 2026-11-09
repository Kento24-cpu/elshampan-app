# elshampan-app

Sistema de venta de licor sellado — WinForms (.NET 8) + MySQL Server. Proyecto de SGBD y Programación Visual.

## Requisitos

- Windows (WinForms solo se ejecuta en Windows)
- .NET SDK 8.0 o superior
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
- **Punto de venta** — carrito, cobro con transacción MySQL (valida y descuenta stock con `FOR UPDATE`), ticket imprimible.
- **Compras** (admin) — entrada de mercancía por proveedor en transacción (aumenta stock); CRUD de proveedores.
- **Reportes** — ventas por rango de fechas, totales por método de pago, top 5 productos y alertas de stock bajo.
- **Usuarios** (admin) — alta/baja y roles.

## Compilación

```
dotnet build
```

El csproj trae `EnableWindowsTargeting=true`, así que `dotnet build` compila también en Linux; la ejecución requiere Windows.

## Estructura

```
database/            schema.sql y seed.sql (ejecutar en Workbench)
src/ElShampan.App/
  Data/              Db + repositorios (consultas parametrizadas)
  Models/            entidades
  Services/          AuthService (hash) y Sesion
  Forms/             pantallas WinForms
  appsettings.json   connection string
```
