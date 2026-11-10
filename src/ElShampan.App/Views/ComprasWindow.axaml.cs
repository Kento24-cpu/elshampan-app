using Avalonia.Controls;
using Avalonia.Interactivity;
using ElShampan.App.Data;
using ElShampan.App.Models;
using ElShampan.App.Services;
using static ElShampan.App.Services.Dialogos;

namespace ElShampan.App.Views;

public partial class ComprasWindow : Window
{
    private readonly List<DetalleCompra> _carrito = new();

    public ComprasWindow()
    {
        InitializeComponent();
        CargarProveedores();
        CargarProductos();
        RefrescarCarrito();
    }

    private void CargarProveedores()
    {
        try
        {
            Proveedor.ItemsSource = null;
            Proveedor.ItemsSource = ProveedorRepository.GetAll();
        }
        catch (Exception ex)
        {
            _ = ErrorDb(ex);
        }
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
            _ = ErrorDb(ex);
        }
    }

    private void OnProductoSeleccionado(object? sender, SelectionChangedEventArgs e)
    {
        if (ProductosGrid.SelectedItem is Producto p)
            PrecioUnitario.Value = p.PrecioCompra;
    }

    private async void OnGestionar(object? sender, RoutedEventArgs e)
    {
        await new ProveedoresWindow().ShowDialog(this);
        CargarProveedores();
    }

    private async void OnAgregar(object? sender, RoutedEventArgs e)
    {
        if (ProductosGrid.SelectedItem is not Producto p) return;
        if ((PrecioUnitario.Value ?? 0) <= 0)
        {
            await Aviso("Ingrese un precio de compra mayor a cero.");
            return;
        }
        var qty = (int)(Cantidad.Value ?? 1);
        var detalle = _carrito.FirstOrDefault(d => d.ProductoId == p.Id);
        if (detalle != null)
        {
            detalle.Cantidad += qty;
            detalle.PrecioUnitario = PrecioUnitario.Value ?? 0;
        }
        else
        {
            _carrito.Add(new DetalleCompra
            {
                ProductoId = p.Id,
                ProductoNombre = p.Nombre,
                Cantidad = qty,
                PrecioUnitario = PrecioUnitario.Value ?? 0,
            });
        }
        RefrescarCarrito();
    }

    private void OnQuitar(object? sender, RoutedEventArgs e)
    {
        if (CarritoGrid.SelectedItem is not DetalleCompra d) return;
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

    private async void OnRegistrar(object? sender, RoutedEventArgs e)
    {
        if (Proveedor.SelectedItem is not Proveedor prov)
        {
            await Aviso("Seleccione un proveedor.");
            return;
        }
        if (_carrito.Count == 0)
        {
            await Aviso("El carrito está vacío.");
            return;
        }
        var compra = new Compra
        {
            ProveedorId = prov.Id,
            UsuarioId = Sesion.Actual!.Id,
            Total = _carrito.Sum(d => d.Subtotal),
            Detalles = new List<DetalleCompra>(_carrito),
        };
        try
        {
            CompraRepository.Registrar(compra);
        }
        catch (Exception ex)
        {
            await ErrorDb(ex, "No se pudo registrar la compra");
            CargarProductos();
            return;
        }
        await Aviso($"Compra #{compra.Id} registrada. Stock actualizado.", "Éxito");
        _carrito.Clear();
        RefrescarCarrito();
        CargarProductos();
    }
}
