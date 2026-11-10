using System.Diagnostics;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ElShampan.App.Models;
using ElShampan.App.Services;
using static ElShampan.App.Services.Dialogos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ElShampan.App.Views;

public partial class TicketWindow : Window
{
    static TicketWindow() => QuestPDF.Settings.License = LicenseType.Community;

    private readonly string _texto;

    public TicketWindow() : this("")
    {
    }

    public TicketWindow(string texto)
    {
        InitializeComponent();
        _texto = texto;
        VistaPrevia.Text = texto;
    }

    public static string Texto(Venta v)
    {
        var sb = new StringBuilder();
        sb.AppendLine("       ELSHAMSPAN");
        sb.AppendLine("    Ticket de venta");
        sb.AppendLine(new string('-', 36));
        sb.AppendLine($"Venta #{v.Id}  {v.Fecha:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"Vendedor: {Truncar(v.Vendedor ?? Sesion.Actual!.NombreCompleto, 26)}");
        sb.AppendLine(new string('-', 36));
        foreach (var d in v.Detalles)
            sb.AppendLine($"{d.Cantidad,3} x {Truncar(d.ProductoNombre, 20),-20}{d.Subtotal,10:N2}");
        sb.AppendLine(new string('-', 36));
        sb.AppendLine($"TOTAL: {v.Total,29:N2}");
        sb.AppendLine($"Pago: {v.MetodoPago}");
        sb.AppendLine();
        sb.AppendLine("     Gracias por su compra");
        return sb.ToString();
    }

    private static string Truncar(string texto, int max) =>
        texto.Length <= max ? texto : texto[..(max - 1)] + ".";

    private static void GenerarPdf(string texto, string path)
    {
        Document.Create(d =>
        {
            d.Page(page =>
            {
                page.Size(80, 200, Unit.Millimetre);
                page.Margin(5, Unit.Millimetre);
                page.DefaultTextStyle(t => t.FontFamily("Courier New").FontSize(9));
                page.Content().Column(col =>
                {
                    foreach (var linea in texto.Replace("\r", "").Split('\n'))
                        col.Item().Text(linea);
                });
            });
        }).GeneratePdf(path);
    }

    private async void OnGuardarPdf(object? sender, RoutedEventArgs e)
    {
        try
        {
            var archivo = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Guardar ticket",
                SuggestedFileName = "ticket",
                DefaultExtension = "pdf",
                FileTypeChoices = new[] { new FilePickerFileType("PDF") { Patterns = new[] { "*.pdf" } } },
            });
            if (archivo is null) return;
            var path = archivo.TryGetLocalPath();
            if (path is null) return;
            GenerarPdf(_texto, path);
        }
        catch (Exception ex)
        {
            await ErrorDb(ex, "Error al generar PDF");
        }
    }

    private async void OnImprimir(object? sender, RoutedEventArgs e)
    {
        try
        {
            var path = Path.Combine(Path.GetTempPath(), $"ticket_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            GenerarPdf(_texto, path);
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            await ErrorDb(ex, "Error al imprimir");
        }
    }
}
