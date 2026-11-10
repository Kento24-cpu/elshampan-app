using Avalonia.Controls;
using Avalonia.Interactivity;
using ElShampan.App.Data;
using ElShampan.App.Services;
using static ElShampan.App.Services.Dialogos;

namespace ElShampan.App.Views;

public partial class HistorialVentasWindow : Window
{
    public HistorialVentasWindow()
    {
        InitializeComponent();
        Fecha.SelectedDate = DateTime.Today;
        Consultar();
    }

    private void OnConsultar(object? sender, RoutedEventArgs e) => Consultar();

    private void Consultar()
    {
        if (Fecha.SelectedDate is not { } f)
        {
            _ = Aviso("Seleccione la fecha.");
            return;
        }
        try
        {
            VentasGrid.ItemsSource = null;
            VentasGrid.ItemsSource = ReporteRepository.Ventas(f.Date, f.Date.AddDays(1), null, incluirAnuladas: true);
        }
        catch (Exception ex)
        {
            _ = ErrorDb(ex);
        }
    }

    private async void OnVerTicket(object? sender, RoutedEventArgs e)
    {
        if (VentasGrid.SelectedItem is not VentaResumen v)
        {
            await Aviso("Seleccione una venta.");
            return;
        }
        try
        {
            var venta = VentaRepository.Obtener(v.Id);
            if (venta is null)
            {
                await Aviso("La venta no existe.");
                return;
            }
            await new TicketWindow(TicketWindow.Texto(venta)).ShowDialog(this);
        }
        catch (Exception ex)
        {
            await ErrorDb(ex);
        }
    }

    private async void OnAnular(object? sender, RoutedEventArgs e)
    {
        if (VentasGrid.SelectedItem is not VentaResumen v)
        {
            await Aviso("Seleccione una venta.");
            return;
        }
        if (v.Anulada)
        {
            await Aviso("La venta ya está anulada.");
            return;
        }
        if (!Sesion.Actual!.EsAdmin && v.UsuarioId != Sesion.Actual.Id)
        {
            await Aviso("Solo puede anular sus propias ventas.");
            return;
        }
        if (!await Confirmacion($"¿Anular la venta #{v.Id} por {v.Total:N2}? Se devolverá el stock."))
            return;
        try
        {
            VentaRepository.Anular(v.Id);
        }
        catch (Exception ex)
        {
            await ErrorDb(ex);
            return;
        }
        Consultar();
    }
}
