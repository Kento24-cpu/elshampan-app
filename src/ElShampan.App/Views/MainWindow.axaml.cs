using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ElShampan.App.Services;

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
        AddNav("Punto de venta", () => new PuntoVentaWindow().ShowDialog(this));
        Closed += (_, _) =>
        {
            if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow = new LoginWindow();
        };
    }

    private void AddNav(string label, Action open, bool adminOnly = false)
    {
        if (adminOnly && !Sesion.Actual!.EsAdmin) return;
        var btn = new Button { Content = label, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
        btn.Click += (_, _) => open();
        Nav.Children.Add(btn);
    }

    private void OnCerrarSesion(object? sender, RoutedEventArgs e) => Close();
}
