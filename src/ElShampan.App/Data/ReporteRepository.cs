using MySqlConnector;

namespace ElShampan.App.Data;

public record VentaResumen(int Id, DateTime Fecha, string Vendedor, string MetodoPago, decimal Total);

public record TopProducto(string Nombre, int Cantidad, decimal Total);

public static class ReporteRepository
{
    public static List<VentaResumen> Ventas(DateTime desde, DateTime hasta)
    {
        using var conn = Db.Open();
        using var cmd = new MySqlCommand(
            @"SELECT v.id, v.fecha, u.nombre_completo, v.metodo_pago, v.total
              FROM ventas v
              INNER JOIN usuarios u ON u.id = v.usuario_id
              WHERE v.fecha >= @desde AND v.fecha < @hasta
              ORDER BY v.fecha DESC", conn);
        cmd.Parameters.AddWithValue("@desde", desde);
        cmd.Parameters.AddWithValue("@hasta", hasta);
        using var r = cmd.ExecuteReader();
        var list = new List<VentaResumen>();
        while (r.Read())
        {
            list.Add(new VentaResumen(
                r.GetInt32("id"),
                r.GetDateTime("fecha"),
                r.GetString("nombre_completo"),
                r.GetString("metodo_pago"),
                r.GetDecimal("total")));
        }
        return list;
    }

    public static List<TopProducto> TopProductos(DateTime desde, DateTime hasta, int limit = 5)
    {
        using var conn = Db.Open();
        using var cmd = new MySqlCommand(
            @"SELECT p.nombre, SUM(d.cantidad) AS cantidad, SUM(d.subtotal) AS total
              FROM ventas_detalles d
              INNER JOIN productos p ON p.id = d.producto_id
              INNER JOIN ventas v ON v.id = d.venta_id
              WHERE v.fecha >= @desde AND v.fecha < @hasta
              GROUP BY p.id, p.nombre
              ORDER BY total DESC
              LIMIT @limit", conn);
        cmd.Parameters.AddWithValue("@desde", desde);
        cmd.Parameters.AddWithValue("@hasta", hasta);
        cmd.Parameters.AddWithValue("@limit", limit);
        using var r = cmd.ExecuteReader();
        var list = new List<TopProducto>();
        while (r.Read())
        {
            list.Add(new TopProducto(r.GetString("nombre"), r.GetInt32("cantidad"), r.GetDecimal("total")));
        }
        return list;
    }
}
