using Avalonia.Controls;
using Avalonia.Interactivity;
using ElShampan.App.Data;
using ElShampan.App.Models;
using static ElShampan.App.Services.Dialogos;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace ElShampan.App.Views;

public partial class ProductosWindow : Window
{
    private List<Producto> _productos = new();
    private Producto? _actual;

    public ProductosWindow()
    {
        InitializeComponent();
        Reload();
    }

    private void OnBusquedaChanged(object? sender, TextChangedEventArgs e) => Reload();

    private void OnBuscar(object? sender, RoutedEventArgs e) => Reload();

    private void Reload()
    {
        try
        {
            _productos = ProductoRepository.GetAll();
        }
        catch (Exception ex)
        {
            _ = ErrorDb(ex);
            return;
        }
        var term = Busqueda.Text?.Trim() ?? "";
        var lista = term.Length == 0
            ? _productos
            : _productos.Where(p => p.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase)
                                 || p.Codigo.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        Grid.ItemsSource = null;
        Grid.ItemsSource = lista;
    }

    private void OnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        e.Row.Classes.Remove("inactivo");
        e.Row.Classes.Remove("stock-bajo");
        if (e.Row.DataContext is not Producto p) return;
        if (!p.Activo) e.Row.Classes.Add("inactivo");
        else if (p.Stock <= p.StockMinimo) e.Row.Classes.Add("stock-bajo");
    }

    private void OnSeleccionar(object? sender, SelectionChangedEventArgs e)
    {
        if (Grid.SelectedItem is not Producto p) return;
        _actual = p;
        Codigo.Text = p.Codigo;
        Nombre.Text = p.Nombre;
        Categoria.Text = p.Categoria;
        Descripcion.Text = p.Descripcion ?? "";
        PrecioCompra.Value = p.PrecioCompra;
        PrecioVenta.Value = p.PrecioVenta;
        Stock.Value = p.Stock;
        StockMinimo.Value = p.StockMinimo;
        Activo.IsChecked = p.Activo;
    }

    private void OnNuevo(object? sender, RoutedEventArgs e) => Nuevo();

    private void Nuevo()
    {
        _actual = null;
        Codigo.Text = "";
        Nombre.Text = "";
        Categoria.Text = "";
        Descripcion.Text = "";
        PrecioCompra.Value = 0;
        PrecioVenta.Value = 0;
        Stock.Value = 0;
        StockMinimo.Value = 5;
        Activo.IsChecked = true;
        Codigo.Focus();
    }

    private async void OnGuardar(object? sender, RoutedEventArgs e)
    {
        if ((Codigo.Text?.Trim().Length ?? 0) == 0 || (Nombre.Text?.Trim().Length ?? 0) == 0 || (Categoria.Text?.Trim().Length ?? 0) == 0)
        {
            await Aviso("Código, nombre y categoría son obligatorios.");
            return;
        }
        if ((PrecioVenta.Value ?? 0) <= 0)
        {
            await Aviso("El precio de venta debe ser mayor a cero.");
            return;
        }
        if ((PrecioCompra.Value ?? 0) <= 0)
        {
            await Aviso("El precio de compra debe ser mayor a cero.");
            return;
        }
        var p = _actual ?? new Producto();
        p.Codigo = Codigo.Text!.Trim();
        p.Nombre = Nombre.Text!.Trim();
        p.Categoria = Categoria.Text!.Trim();
        p.Descripcion = string.IsNullOrWhiteSpace(Descripcion.Text) ? null : Descripcion.Text!.Trim();
        p.PrecioCompra = PrecioCompra.Value ?? 0;
        p.PrecioVenta = PrecioVenta.Value ?? 0;
        p.Stock = (int)(Stock.Value ?? 0);
        p.StockMinimo = (int)(StockMinimo.Value ?? 0);
        p.Activo = Activo.IsChecked == true;
        try
        {
            if (_actual == null) ProductoRepository.Insert(p);
            else ProductoRepository.Update(p);
        }
        catch (MySqlConnector.MySqlException ex) when (ex.Number == 1062)
        {
            await Aviso("Ya existe un producto con ese código.");
            return;
        }
        catch (Exception ex)
        {
            await ErrorDb(ex);
            return;
        }
        _actual = p;
        Reload();
    }

    private async void OnDarDeBaja(object? sender, RoutedEventArgs e)
    {
        if (_actual == null || !_actual.Activo)
        {
            await Aviso("Seleccione un producto activo.");
            return;
        }
        var r = await MessageBoxManager.GetMessageBoxStandard(
            "Confirmar", $"Dar de baja '{_actual.Nombre}'?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Warning).ShowWindowAsync();
        if (r != ButtonResult.Yes) return;
        _actual.Activo = false;
        try
        {
            ProductoRepository.Update(_actual);
        }
        catch (Exception ex)
        {
            await ErrorDb(ex);
            return;
        }
        Nuevo();
        Reload();
    }
}
