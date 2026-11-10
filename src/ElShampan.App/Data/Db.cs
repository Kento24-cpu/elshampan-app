using System.Text.Json;
using MySqlConnector;

namespace ElShampan.App.Data;

public static class Db
{
    public static string ConnectionString { get; } = Load();

    private static int _esquemaAsegurado;

    public static MySqlConnection Open()
    {
        AsegurarEsquema();
        var conn = new MySqlConnection(ConnectionString);
        conn.Open();
        return conn;
    }

    // Bases creadas antes de la columna 'anulada' fallan en ReporteRepository;
    // schema.sql solo la documenta como comentario, así que se aplica sola.
    private static void AsegurarEsquema()
    {
        if (Interlocked.Exchange(ref _esquemaAsegurado, 1) != 0) return;
        try
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            using var check = new MySqlCommand(
                "SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = 'ventas' AND column_name = 'anulada'",
                conn);
            if (Convert.ToInt64(check.ExecuteScalar()) > 0) return;
            using var alter = new MySqlCommand(
                "ALTER TABLE ventas ADD COLUMN anulada TINYINT(1) NOT NULL DEFAULT 0", conn);
            alter.ExecuteNonQuery();
        }
        catch (MySqlException)
        {
            // sin conexión o sin permisos: lo dirá la primera consulta real; se reintenta
            Interlocked.Exchange(ref _esquemaAsegurado, 0);
        }
    }

    private static string Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        return doc.RootElement.GetProperty("ConnectionString").GetString()
            ?? throw new InvalidOperationException("ConnectionString missing in appsettings.json");
    }
}
