using ElShampan.App.Models;
using MySqlConnector;

namespace ElShampan.App.Data;

public static class UsuarioRepository
{
    private const string Cols = "id, nombre_usuario, password_hash, nombre_completo, rol, activo";

    public static Usuario? Authenticate(string nombreUsuario)
    {
        using var conn = Db.Open();
        using var cmd = new MySqlCommand($"SELECT {Cols} FROM usuarios WHERE nombre_usuario = @u", conn);
        cmd.Parameters.AddWithValue("@u", nombreUsuario);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Map(r) : null;
    }

    public static List<Usuario> GetAll()
    {
        using var conn = Db.Open();
        using var cmd = new MySqlCommand($"SELECT {Cols} FROM usuarios ORDER BY nombre_usuario", conn);
        using var r = cmd.ExecuteReader();
        var list = new List<Usuario>();
        while (r.Read()) list.Add(Map(r));
        return list;
    }

    public static void Insert(Usuario u, string password)
    {
        using var conn = Db.Open();
        using var cmd = new MySqlCommand(
            @"INSERT INTO usuarios (nombre_usuario, password_hash, nombre_completo, rol, activo)
              VALUES (@nu, @ph, @nc, @rol, @activo);
              SELECT LAST_INSERT_ID();", conn);
        cmd.Parameters.AddWithValue("@nu", u.NombreUsuario);
        cmd.Parameters.AddWithValue("@ph", Services.AuthService.Hash(password));
        cmd.Parameters.AddWithValue("@nc", u.NombreCompleto);
        cmd.Parameters.AddWithValue("@rol", u.Rol);
        cmd.Parameters.AddWithValue("@activo", u.Activo);
        u.Id = Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static void Update(Usuario u, string? nuevaPassword = null)
    {
        using var conn = Db.Open();
        var sql = @"UPDATE usuarios SET nombre_usuario = @nu, nombre_completo = @nc,
                    rol = @rol, activo = @activo";
        if (nuevaPassword != null) sql += ", password_hash = @ph";
        sql += " WHERE id = @id";
        using var cmd = new MySqlCommand(sql, conn);
        if (nuevaPassword != null)
            cmd.Parameters.AddWithValue("@ph", Services.AuthService.Hash(nuevaPassword));
        cmd.Parameters.AddWithValue("@nu", u.NombreUsuario);
        cmd.Parameters.AddWithValue("@nc", u.NombreCompleto);
        cmd.Parameters.AddWithValue("@rol", u.Rol);
        cmd.Parameters.AddWithValue("@activo", u.Activo);
        cmd.Parameters.AddWithValue("@id", u.Id);
        cmd.ExecuteNonQuery();
    }

    private static Usuario Map(MySqlDataReader r) => new()
    {
        Id = r.GetInt32("id"),
        NombreUsuario = r.GetString("nombre_usuario"),
        PasswordHash = r.GetString("password_hash"),
        NombreCompleto = r.GetString("nombre_completo"),
        Rol = r.GetString("rol"),
        Activo = r.GetBoolean("activo"),
    };
}
