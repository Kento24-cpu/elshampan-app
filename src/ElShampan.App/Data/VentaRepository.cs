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
}
