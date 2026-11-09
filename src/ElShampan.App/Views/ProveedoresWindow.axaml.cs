using Avalonia.Controls;
using Avalonia.Interactivity;
using ElShampan.App.Data;
using ElShampan.App.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace ElShampan.App.Views;

public partial class ProveedoresWindow : Window
{
    private List<Proveedor> _proveedores = new();
    private Proveedor? _actual;

    public ProveedoresWindow()
    {
        InitializeComponent();
        Reload();
    }

    private static Task Aviso(string texto, string titulo = "Validación") =>
        MessageBoxManager.GetMessageBoxStandard(titulo, texto, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning).ShowWindowAsync();

    private void Reload()
    {
        try
        {
            _proveedores = ProveedorRepository.GetAll();
        }
        catch (Exception ex)
        {
            _ = MessageBoxManager.GetMessageBoxStandard("Error de base de datos", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowWindowAsync();
            return;
        }
        Grid.ItemsSource = null;
        Grid.ItemsSource = _proveedores;
    }

    private void OnNuevo(object? sender, RoutedEventArgs e) => Nuevo();

    private void Nuevo()
    {
        _actual = null;
        Nombre.Text = "";
        Telefono.Text = "";
        Email.Text = "";
        Direccion.Text = "";
        Nombre.Focus();
    }

    private void OnSeleccionar(object? sender, SelectionChangedEventArgs e)
    {
        if (Grid.SelectedItem is not Proveedor p) return;
        _actual = p;
        Nombre.Text = p.Nombre;
        Telefono.Text = p.Telefono ?? "";
        Email.Text = p.Email ?? "";
        Direccion.Text = p.Direccion ?? "";
    }

    private async void OnGuardar(object? sender, RoutedEventArgs e)
    {
        if ((Nombre.Text?.Trim().Length ?? 0) == 0)
        {
            await Aviso("El nombre es obligatorio.");
            return;
        }
        var p = _actual ?? new Proveedor();
        p.Nombre = Nombre.Text!.Trim();
        p.Telefono = string.IsNullOrWhiteSpace(Telefono.Text) ? null : Telefono.Text!.Trim();
        p.Email = string.IsNullOrWhiteSpace(Email.Text) ? null : Email.Text!.Trim();
        p.Direccion = string.IsNullOrWhiteSpace(Direccion.Text) ? null : Direccion.Text!.Trim();
        try
        {
            if (_actual == null) ProveedorRepository.Insert(p);
            else ProveedorRepository.Update(p);
        }
        catch (Exception ex)
        {
            await MessageBoxManager.GetMessageBoxStandard("Error de base de datos", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowWindowAsync();
            return;
        }
        _actual = p;
        Reload();
    }

    private async void OnEliminar(object? sender, RoutedEventArgs e)
    {
        if (_actual == null)
        {
            await Aviso("Seleccione un proveedor.");
            return;
        }
        var r = await MessageBoxManager.GetMessageBoxStandard(
            "Confirmar", $"Eliminar proveedor '{_actual.Nombre}'?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Warning).ShowWindowAsync();
        if (r != ButtonResult.Yes) return;
        try
        {
            ProveedorRepository.Delete(_actual.Id);
        }
        catch (MySqlConnector.MySqlException ex) when (ex.Number == 1451)
        {
            await Aviso("No se puede eliminar: el proveedor tiene compras registradas.");
            return;
        }
        catch (Exception ex)
        {
            await MessageBoxManager.GetMessageBoxStandard("Error de base de datos", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowWindowAsync();
            return;
        }
        Nuevo();
        Reload();
    }
}
