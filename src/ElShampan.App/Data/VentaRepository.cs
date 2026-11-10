using ElShampan.App.Models;
using MySqlConnector;

namespace ElShampan.App.Data;

public static class VentaRepository
{
    public static int Registrar(Venta venta)
    {
        using var conn = Db.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            foreach (var d in venta.Detalles)
            {
                using var cmd = new MySqlCommand("SELECT stock FROM productos WHERE id = @id FOR UPDATE", conn, tx);
                cmd.Parameters.AddWithValue("@id", d.ProductoId);
                var stock = Convert.ToInt32(cmd.ExecuteScalar());
                if (stock < d.Cantidad)
                    throw new InvalidOperationException(
                        $"Stock insuficiente para '{d.ProductoNombre}' (disponible: {stock}, solicitado: {d.Cantidad}).");
            }

            long ventaId;
            using (var cmd = new MySqlCommand(
                @"INSERT INTO ventas (usuario_id, subtotal, total, metodo_pago)
                  VALUES (@u, @s, @t, @m);
                  SELECT LAST_INSERT_ID();", conn, tx))
            {
                cmd.Parameters.AddWithValue("@u", venta.UsuarioId);
                cmd.Parameters.AddWithValue("@s", venta.Subtotal);
                cmd.Parameters.AddWithValue("@t", venta.Total);
                cmd.Parameters.AddWithValue("@m", venta.MetodoPago);
                ventaId = Convert.ToInt64(cmd.ExecuteScalar());
            }

            foreach (var d in venta.Detalles)
            {
                using var cmd = new MySqlCommand(
                    @"INSERT INTO ventas_detalles (venta_id, producto_id, cantidad, precio_unitario, subtotal)
                      VALUES (@v, @p, @c, @pu, @st);
                      UPDATE productos SET stock = stock - @c WHERE id = @p;", conn, tx);
                cmd.Parameters.AddWithValue("@v", ventaId);
                cmd.Parameters.AddWithValue("@p", d.ProductoId);
                cmd.Parameters.AddWithValue("@c", d.Cantidad);
                cmd.Parameters.AddWithValue("@pu", d.PrecioUnitario);
                cmd.Parameters.AddWithValue("@st", d.Subtotal);
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
            venta.Id = (int)ventaId;
            return venta.Id;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public static Venta? Obtener(int id)
    {
        using var conn = Db.Open();
        Venta venta;
        using (var cmd = new MySqlCommand(
            @"SELECT v.id, v.usuario_id, v.fecha, v.subtotal, v.total, v.metodo_pago, u.nombre_completo
              FROM ventas v
              INNER JOIN usuarios u ON u.id = v.usuario_id
              WHERE v.id = @id", conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            venta = new Venta
            {
                Id = r.GetInt32("id"),
                UsuarioId = r.GetInt32("usuario_id"),
                Fecha = r.GetDateTime("fecha"),
                Subtotal = r.GetDecimal("subtotal"),
                Total = r.GetDecimal("total"),
                MetodoPago = r.GetString("metodo_pago"),
                Vendedor = r.GetString("nombre_completo"),
            };
        }
        using (var cmd = new MySqlCommand(
            @"SELECT d.producto_id, p.nombre, d.cantidad, d.precio_unitario
              FROM ventas_detalles d
              INNER JOIN productos p ON p.id = d.producto_id
              WHERE d.venta_id = @id", conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                venta.Detalles.Add(new DetalleVenta
                {
                    ProductoId = r.GetInt32("producto_id"),
                    ProductoNombre = r.GetString("nombre"),
                    Cantidad = r.GetInt32("cantidad"),
                    PrecioUnitario = r.GetDecimal("precio_unitario"),
                });
            }
        }
        return venta;
    }

    public static void Anular(int id)
    {
        using var conn = Db.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            using (var cmd = new MySqlCommand("SELECT anulada FROM ventas WHERE id = @id FOR UPDATE", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", id);
                var existe = cmd.ExecuteScalar();
                if (existe is null) throw new InvalidOperationException("La venta no existe.");
                if (Convert.ToInt32(existe) != 0) throw new InvalidOperationException("La venta ya está anulada.");
            }

            var detalles = new List<(int ProductoId, int Cantidad)>();
            using (var cmd = new MySqlCommand("SELECT producto_id, cantidad FROM ventas_detalles WHERE venta_id = @id", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using var r = cmd.ExecuteReader();
                while (r.Read()) detalles.Add((r.GetInt32("producto_id"), r.GetInt32("cantidad")));
            }

            foreach (var (productoId, cantidad) in detalles)
            {
                using var cmd = new MySqlCommand("UPDATE productos SET stock = stock + @c WHERE id = @p", conn, tx);
                cmd.Parameters.AddWithValue("@c", cantidad);
                cmd.Parameters.AddWithValue("@p", productoId);
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new MySqlCommand("UPDATE ventas SET anulada = 1 WHERE id = @id", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
