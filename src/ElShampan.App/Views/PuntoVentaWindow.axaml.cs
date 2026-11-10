using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ElShampan.App.Data;
using ElShampan.App.Models;
using ElShampan.App.Services;
using static ElShampan.App.Services.Dialogos;

namespace ElShampan.App.Views;

public partial class PuntoVentaWindow : Window
{
    private readonly List<DetalleVenta> _carrito = new();

    public PuntoVentaWindow()
    {
        InitializeComponent();
        CargarProductos();
        RefrescarCarrito();
    }

    private void OnBuscarChanged(object? sender, TextChangedEventArgs e) => CargarProductos();

    private void CargarProductos()
    {
        try
        {
            ProductosGrid.ItemsSource = null;
            ProductosGrid.ItemsSource = ProductoRepository.Search(Buscar.Text?.Trim() ?? "");
        }
        catch (Exception ex)
        {
            ErrorDb(ex);
        }
    }

    private void OnProductoDobleClick(object? sender, RoutedEventArgs e) => Agregar();

    private void OnAgregar(object? sender, RoutedEventArgs e) => Agregar();

    private async void Agregar()
    {
        if (ProductosGrid.SelectedItem is not Producto p)
        {
            await Aviso("Seleccione un producto.");
            return;
        }
        var qty = (int)(Cantidad.Value ?? 1);
        var enCarrito = _carrito.Where(d => d.ProductoId == p.Id).Sum(d => d.Cantidad);
        if (enCarrito + qty > p.Stock)
        {
            await Aviso($"Stock insuficiente. Disponible: {p.Stock}, en carrito: {enCarrito}.");
            return;
        }
        var detalle = _carrito.FirstOrDefault(d => d.ProductoId == p.Id);
        if (detalle != null) detalle.Cantidad += qty;
        else _carrito.Add(new DetalleVenta { ProductoId = p.Id, ProductoNombre = p.Nombre, Cantidad = qty, PrecioUnitario = p.PrecioVenta });
        RefrescarCarrito();
    }

    private async void OnQuitar(object? sender, RoutedEventArgs e)
    {
        if (CarritoGrid.SelectedItem is not DetalleVenta d)
        {
            await Aviso("Seleccione un artículo del carrito.");
            return;
        }
        _carrito.Remove(d);
        RefrescarCarrito();
    }

    private void OnLimpiar(object? sender, RoutedEventArgs e)
    {
        _carrito.Clear();
        RefrescarCarrito();
    }

    private void RefrescarCarrito()
    {
        CarritoGrid.ItemsSource = null;
        CarritoGrid.ItemsSource = _carrito.ToList();
        Total.Text = _carrito.Sum(d => d.Subtotal).ToString("N2");
    }

    private async void OnCobrar(object? sender, RoutedEventArgs e)
    {
        if (_carrito.Count == 0)
        {
            await Aviso("El carrito está vacío.");
            return;
        }
        if (Pago.SelectedItem is not ComboBoxItem pago)
        {
            await Aviso("Seleccione un método de pago.");
            return;
        }
        var venta = new Venta
        {
            UsuarioId = Sesion.Actual!.Id,
            MetodoPago = pago.Content!.ToString()!,
            Subtotal = _carrito.Sum(d => d.Subtotal),
            Total = _carrito.Sum(d => d.Subtotal),
            Detalles = new List<DetalleVenta>(_carrito),
        };
        try
        {
            VentaRepository.Registrar(venta);
        }
        catch (Exception ex)
        {
            await ErrorDb(ex, "No se pudo registrar la venta");
            CargarProductos();
            return;
        }
        venta.Fecha = DateTime.Now;
        var texto = TicketTexto(venta);
        _carrito.Clear();
        RefrescarCarrito();
        CargarProductos();
        await new TicketWindow(texto).ShowDialog(this);
    }

    private static string TicketTexto(Venta v)
    {
        var sb = new StringBuilder();
        sb.AppendLine("       ELSHAMSPAN");
        sb.AppendLine("    Ticket de venta");
        sb.AppendLine(new string('-', 36));
        sb.AppendLine($"Venta #{v.Id}  {v.Fecha:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"Vendedor: {Truncar(Sesion.Actual!.NombreCompleto, 26)}");
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
}
