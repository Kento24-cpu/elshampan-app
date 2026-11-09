using System.Text.Json;
using MySqlConnector;

namespace ElShampan.App.Data;

public static class Db
{
    public static string ConnectionString { get; } = Load();

    public static async Task<MySqlConnection> OpenAsync()
    {
        var conn = new MySqlConnection(ConnectionString);
        await conn.OpenAsync();
        return conn;
    }

    private static string Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        return doc.RootElement.GetProperty("ConnectionString").GetString()
            ?? throw new InvalidOperationException("ConnectionString missing in appsettings.json");
    }
}
