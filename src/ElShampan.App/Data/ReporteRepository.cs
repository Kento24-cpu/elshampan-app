using MySqlConnector;

namespace ElShampan.App.Data;

public record VentaResumen(int Id, int UsuarioId, DateTime Fecha, string Vendedor, string MetodoPago, decimal Total, bool Anulada)
{
    public string Estado => Anulada ? "Anulada" : "";
}

public record TopProducto(string Nombre, int Cantidad, decimal Total);

public static class ReporteRepository
{
    public static List<VentaResumen> Ventas(DateTime desde, DateTime hasta, int? usuarioId = null, bool incluirAnuladas = false)
    {
        using var conn = Db.Open();
        var sql = @"SELECT v.id, v.usuario_id, v.fecha, u.nombre_completo, v.metodo_pago, v.total, v.anulada
              FROM ventas v
              INNER JOIN usuarios u ON u.id = v.usuario_id
              WHERE v.fecha >= @desde AND v.fecha < @hasta";
        if (!incluirAnuladas) sql += " AND v.anulada = 0";
        if (usuarioId is not null) sql += " AND v.usuario_id = @usuario";
        sql += " ORDER BY v.fecha DESC";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@desde", desde);
        cmd.Parameters.AddWithValue("@hasta", hasta);
        if (usuarioId is not null) cmd.Parameters.AddWithValue("@usuario", usuarioId);
        using var r = cmd.ExecuteReader();
        var list = new List<VentaResumen>();
        while (r.Read())
        {
            list.Add(new VentaResumen(
                r.GetInt32("id"),
                r.GetInt32("usuario_id"),
                r.GetDateTime("fecha"),
                r.GetString("nombre_completo"),
                r.GetString("metodo_pago"),
                r.GetDecimal("total"),
                r.GetBoolean("anulada")));
        }
        return list;
    }

    public static List<TopProducto> TopProductos(DateTime desde, DateTime hasta, int limit = 5, int? usuarioId = null)
    {
        using var conn = Db.Open();
        var sql = @"SELECT p.nombre, SUM(d.cantidad) AS cantidad, SUM(d.subtotal) AS total
              FROM ventas_detalles d
              INNER JOIN productos p ON p.id = d.producto_id
              INNER JOIN ventas v ON v.id = d.venta_id
              WHERE v.fecha >= @desde AND v.fecha < @hasta AND v.anulada = 0";
        if (usuarioId is not null) sql += " AND v.usuario_id = @usuario";
        sql += @" GROUP BY p.id, p.nombre
              ORDER BY total DESC
              LIMIT @limit";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@desde", desde);
        cmd.Parameters.AddWithValue("@hasta", hasta);
        cmd.Parameters.AddWithValue("@limit", limit);
        if (usuarioId is not null) cmd.Parameters.AddWithValue("@usuario", usuarioId);
        using var r = cmd.ExecuteReader();
        var list = new List<TopProducto>();
        while (r.Read())
        {
            list.Add(new TopProducto(r.GetString("nombre"), r.GetInt32("cantidad"), r.GetDecimal("total")));
        }
        return list;
    }
}
