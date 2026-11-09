using ElShampan.App.Models;
using MySqlConnector;

namespace ElShampan.App.Data;

public static class CompraRepository
{
    public static int Registrar(Compra compra)
    {
        using var conn = Db.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            long compraId;
            using (var cmd = new MySqlCommand(
                @"INSERT INTO compras (proveedor_id, usuario_id, total)
                  VALUES (@pr, @u, @t);
                  SELECT LAST_INSERT_ID();", conn, tx))
            {
                cmd.Parameters.AddWithValue("@pr", compra.ProveedorId);
                cmd.Parameters.AddWithValue("@u", compra.UsuarioId);
                cmd.Parameters.AddWithValue("@t", compra.Total);
                compraId = Convert.ToInt64(cmd.ExecuteScalar());
            }

            foreach (var d in compra.Detalles)
            {
                using var cmd = new MySqlCommand(
                    @"INSERT INTO compras_detalles (compra_id, producto_id, cantidad, precio_unitario)
                      VALUES (@c, @p, @cant, @pu);
                      UPDATE productos SET stock = stock + @cant WHERE id = @p;", conn, tx);
                cmd.Parameters.AddWithValue("@c", compraId);
                cmd.Parameters.AddWithValue("@p", d.ProductoId);
                cmd.Parameters.AddWithValue("@cant", d.Cantidad);
                cmd.Parameters.AddWithValue("@pu", d.PrecioUnitario);
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
            compra.Id = (int)compraId;
            return compra.Id;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
