using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ElShampan.App.Data;
using ElShampan.App.Services;
using static ElShampan.App.Services.Dialogos;

namespace ElShampan.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var u = Sesion.Actual!;
        Title = $"ElShampan - Sistema de venta de licor [{u.Rol}]";
        Usuario.Text = $"{u.NombreCompleto}\n{u.Rol}";
        Estado.Text = $"{u.NombreCompleto} ({u.Rol})";
        AddNav("Productos", () => new ProductosWindow().ShowDialog(this), adminOnly: true);
        AddNav("Compras", () => new ComprasWindow().ShowDialog(this), adminOnly: true);
        AddNav("Punto de venta", () => new PuntoVentaWindow().ShowDialog(this));
        AddNav("Reportes", () => new ReportesWindow().ShowDialog(this));
        AddNav("Ventas del día", () => new HistorialVentasWindow().ShowDialog(this));
        AddNav("Usuarios", () => new UsuariosWindow().ShowDialog(this), adminOnly: true);
        CargarResumen();
        Closed += (_, _) =>
        {
            if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var login = new LoginWindow();
                desktop.MainWindow = login;
                login.Show();
            }
        };
    }

    private void AddNav(string label, Func<Task> open, bool adminOnly = false)
    {
        if (adminOnly && !Sesion.Actual!.EsAdmin) return;
        var btn = new Button { Content = label, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
        btn.Click += async (_, _) =>
        {
            await open();
            CargarResumen();
        };
        Nav.Children.Add(btn);
    }

    private void CargarResumen()
    {
        try
        {
            var esAdmin = Sesion.Actual!.EsAdmin;
            int? usuarioId = esAdmin ? null : Sesion.Actual.Id;
            var desde = DateTime.Today;
            var hasta = desde.AddDays(1);
            VentasHoyTitulo.Text = esAdmin ? "Ventas de hoy" : "Mis ventas de hoy";
            var ventas = ReporteRepository.Ventas(desde, hasta, usuarioId);
            VentasHoyTotal.Text = ventas.Sum(v => v.Total).ToString("N2");
            VentasHoyCantidad.Text = ventas.Count switch
            {
                0 => "Sin ventas aún",
                1 => "1 venta",
                _ => $"{ventas.Count} ventas",
            };
            StockBajoTotal.Text = ProductoRepository.GetLowStock().Count.ToString();
            var top = ReporteRepository.TopProductos(desde, hasta, 3, usuarioId);
            TopHoy.Text = top.Count == 0
                ? "Sin ventas registradas hoy."
                : string.Join("\n", top.Select(t => $"{t.Nombre} · {t.Cantidad} uds · {t.Total:N2}"));
        }
        catch (Exception ex)
        {
            _ = ErrorDb(ex);
        }
    }

    private void OnCerrarSesion(object? sender, RoutedEventArgs e) => Close();
}
