using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
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
            await MessageBoxManager.GetMessageBoxStandard("Error al generar PDF", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowWindowAsync();
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
            await MessageBoxManager.GetMessageBoxStandard("Error al imprimir", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowWindowAsync();
        }
    }
}
