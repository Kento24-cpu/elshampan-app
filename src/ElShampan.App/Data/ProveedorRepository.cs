using ElShampan.App.Models;
using MySqlConnector;

namespace ElShampan.App.Data;

public static class ProveedorRepository
{
    public static List<Proveedor> GetAll()
    {
        using var conn = Db.Open();
        using var cmd = new MySqlCommand(
            "SELECT id, nombre, telefono, email, direccion FROM proveedores ORDER BY nombre", conn);
        using var r = cmd.ExecuteReader();
        var list = new List<Proveedor>();
        while (r.Read())
        {
            list.Add(new Proveedor
            {
                Id = r.GetInt32("id"),
                Nombre = r.GetString("nombre"),
                Telefono = r.IsDBNull(r.GetOrdinal("telefono")) ? null : r.GetString("telefono"),
                Email = r.IsDBNull(r.GetOrdinal("email")) ? null : r.GetString("email"),
                Direccion = r.IsDBNull(r.GetOrdinal("direccion")) ? null : r.GetString("direccion"),
            });
        }
        return list;
    }

    public static void Insert(Proveedor p)
    {
        using var conn = Db.Open();
        using var cmd = new MySqlCommand(
            @"INSERT INTO proveedores (nombre, telefono, email, direccion)
              VALUES (@nom, @tel, @email, @dir);
              SELECT LAST_INSERT_ID();", conn);
        AddParams(cmd, p);
        p.Id = Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static void Update(Proveedor p)
    {
        using var conn = Db.Open();
        using var cmd = new MySqlCommand(
            "UPDATE proveedores SET nombre = @nom, telefono = @tel, email = @email, direccion = @dir WHERE id = @id",
            conn);
        AddParams(cmd, p);
        cmd.Parameters.AddWithValue("@id", p.Id);
        cmd.ExecuteNonQuery();
    }

    public static void Delete(int id)
    {
        using var conn = Db.Open();
        using var cmd = new MySqlCommand("DELETE FROM proveedores WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private static void AddParams(MySqlCommand cmd, Proveedor p)
    {
        cmd.Parameters.AddWithValue("@nom", p.Nombre);
        cmd.Parameters.AddWithValue("@tel", (object?)p.Telefono ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@email", (object?)p.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@dir", (object?)p.Direccion ?? DBNull.Value);
    }
}
