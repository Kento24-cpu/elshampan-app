using ElShampan.App.Models;
using MySqlConnector;

namespace ElShampan.App.Data;

public static class ProductoRepository
{
    private const string Cols = "id, codigo, nombre, categoria, descripcion, precio_compra, precio_venta, stock, stock_minimo, activo";

    public static List<Producto> Search(string termino)
    {
        using var conn = Db.Open();
        using var cmd = new MySqlCommand(
            $"SELECT {Cols} FROM productos WHERE activo = 1 AND (nombre LIKE @t OR codigo LIKE @t) ORDER BY nombre", conn);
        cmd.Parameters.AddWithValue("@t", $"%{termino}%");
        using var r = cmd.ExecuteReader();
        var list = new List<Producto>();
        while (r.Read()) list.Add(Map(r));
        return list;
    }

    public static List<Producto> GetAll()
    {
        using var conn = Db.Open();
        using var cmd = new MySqlCommand($"SELECT {Cols} FROM productos ORDER BY nombre", conn);
        using var r = cmd.ExecuteReader();
        var list = new List<Producto>();
        while (r.Read()) list.Add(Map(r));
        return list;
    }

    public static List<Producto> GetLowStock()
    {
        using var conn = Db.Open();
        using var cmd = new MySqlCommand(
            $"SELECT {Cols} FROM productos WHERE activo = 1 AND stock <= stock_minimo ORDER BY stock", conn);
        using var r = cmd.ExecuteReader();
        var list = new List<Producto>();
        while (r.Read()) list.Add(Map(r));
        return list;
    }

    public static void Insert(Producto p)
    {
        using var conn = Db.Open();
        using var cmd = new MySqlCommand(
            @"INSERT INTO productos (codigo, nombre, categoria, descripcion, precio_compra, precio_venta, stock, stock_minimo, activo)
              VALUES (@cod, @nom, @cat, @desc, @pc, @pv, @stock, @min, @act);
              SELECT LAST_INSERT_ID();", conn);
        AddParams(cmd, p);
        p.Id = Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static void Update(Producto p)
    {
        using var conn = Db.Open();
        using var cmd = new MySqlCommand(
            @"UPDATE productos SET codigo = @cod, nombre = @nom, categoria = @cat, descripcion = @desc,
              precio_compra = @pc, precio_venta = @pv, stock = @stock, stock_minimo = @min, activo = @act
              WHERE id = @id", conn);
        AddParams(cmd, p);
        cmd.Parameters.AddWithValue("@id", p.Id);
        cmd.ExecuteNonQuery();
    }

    private static void AddParams(MySqlCommand cmd, Producto p)
    {
        cmd.Parameters.AddWithValue("@cod", p.Codigo);
        cmd.Parameters.AddWithValue("@nom", p.Nombre);
        cmd.Parameters.AddWithValue("@cat", p.Categoria);
        cmd.Parameters.AddWithValue("@desc", (object?)p.Descripcion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@pc", p.PrecioCompra);
        cmd.Parameters.AddWithValue("@pv", p.PrecioVenta);
        cmd.Parameters.AddWithValue("@stock", p.Stock);
        cmd.Parameters.AddWithValue("@min", p.StockMinimo);
        cmd.Parameters.AddWithValue("@act", p.Activo);
    }

    private static Producto Map(MySqlDataReader r) => new()
    {
        Id = r.GetInt32("id"),
        Codigo = r.GetString("codigo"),
        Nombre = r.GetString("nombre"),
        Categoria = r.GetString("categoria"),
        Descripcion = r.IsDBNull(r.GetOrdinal("descripcion")) ? null : r.GetString("descripcion"),
        PrecioCompra = r.GetDecimal("precio_compra"),
        PrecioVenta = r.GetDecimal("precio_venta"),
        Stock = r.GetInt32("stock"),
        StockMinimo = r.GetInt32("stock_minimo"),
        Activo = r.GetBoolean("activo"),
    };
}
