using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using MySqlConnector;

namespace ElShampan.App.Services;

public static class Dialogos
{
    public static Task Aviso(string texto, string titulo = "Validación") =>
        MessageBoxManager.GetMessageBoxStandard(titulo, texto, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning).ShowWindowAsync();

    public static Task ErrorDb(Exception ex, string titulo = "Error de base de datos") =>
        MessageBoxManager.GetMessageBoxStandard(titulo, DetalleError(ex), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowWindowAsync();

    public static async Task<bool> Confirmacion(string texto, string titulo = "Confirmar") =>
        await MessageBoxManager.GetMessageBoxStandard(titulo, texto, ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question).ShowWindowAsync()
        == ButtonResult.Yes;

    public static string DetalleError(Exception ex) => ex switch
    {
        MySqlException { Number: 1048 } => "Faltan datos obligatorios.",
        MySqlException { Number: 1062 } => "Ya existe un registro con ese valor.",
        MySqlException { Number: 1265 } => "Uno de los valores ingresados no es válido.",
        MySqlException { Number: 1406 } => "El texto excede el largo permitido.",
        MySqlException { Number: 1451 } => "No se puede eliminar: el registro tiene movimientos asociados.",
        MySqlException { Number: 1452 } => "El registro relacionado no existe.",
        MySqlException { Number: 1045 } => "MySQL rechazó las credenciales (Uid/Pwd). Revise appsettings.json.",
        MySqlException { Number: 1049 } => "No existe la base de datos 'elshampan_db'. Ejecute database/schema.sql.",
        MySqlException { Number: 1146 } => "Faltan tablas en la base de datos. Ejecute database/schema.sql.",
        MySqlException mx => $"No hay conexión con MySQL (localhost:3306). Detalle: {mx.Message}",
        _ => $"Error interno. Detalle: {ex.Message}",
    };
}
