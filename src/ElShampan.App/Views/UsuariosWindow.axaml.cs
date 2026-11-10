using Avalonia.Controls;
using Avalonia.Interactivity;
using ElShampan.App.Data;
using ElShampan.App.Models;
using static ElShampan.App.Services.Dialogos;

namespace ElShampan.App.Views;

public partial class UsuariosWindow : Window
{
    private List<Usuario> _usuarios = new();
    private Usuario? _actual;

    public UsuariosWindow()
    {
        InitializeComponent();
        Reload();
    }

    private void Reload()
    {
        try
        {
            _usuarios = UsuarioRepository.GetAll();
        }
        catch (Exception ex)
        {
            _ = ErrorDb(ex);
            return;
        }
        Grid.ItemsSource = null;
        Grid.ItemsSource = _usuarios;
    }

    private void OnNuevo(object? sender, RoutedEventArgs e) => Nuevo();

    private void Nuevo()
    {
        _actual = null;
        NombreUsuario.Text = "";
        NombreCompleto.Text = "";
        Password.Text = "";
        Rol.SelectedIndex = 0;
        Activo.IsChecked = true;
        NombreUsuario.Focus();
    }

    private void OnSeleccionar(object? sender, SelectionChangedEventArgs e)
    {
        if (Grid.SelectedItem is not Usuario u) return;
        _actual = u;
        NombreUsuario.Text = u.NombreUsuario;
        NombreCompleto.Text = u.NombreCompleto;
        Password.Text = "";
        foreach (var item in Rol.Items.OfType<ComboBoxItem>())
            if (item.Content?.ToString() == u.Rol) Rol.SelectedItem = item;
        Activo.IsChecked = u.Activo;
    }

    private async void OnGuardar(object? sender, RoutedEventArgs e)
    {
        if ((NombreUsuario.Text?.Trim().Length ?? 0) == 0 || (NombreCompleto.Text?.Trim().Length ?? 0) == 0)
        {
            await Aviso("Usuario y nombre completo son obligatorios.");
            return;
        }
        var nuevaPassword = string.IsNullOrEmpty(Password.Text) ? null : Password.Text;
        if (_actual == null && nuevaPassword == null)
        {
            await Aviso("La contraseña es obligatoria para usuarios nuevos.");
            return;
        }
        if (nuevaPassword != null && nuevaPassword.Length < 6)
        {
            await Aviso("La contraseña debe tener al menos 6 caracteres.");
            return;
        }
        var u = _actual ?? new Usuario();
        u.NombreUsuario = NombreUsuario.Text!.Trim();
        u.NombreCompleto = NombreCompleto.Text!.Trim();
        u.Rol = ((ComboBoxItem)Rol.SelectedItem!).Content!.ToString()!;
        u.Activo = Activo.IsChecked == true;
        try
        {
            if (_actual == null) UsuarioRepository.Insert(u, nuevaPassword!);
            else UsuarioRepository.Update(u, nuevaPassword);
        }
        catch (MySqlConnector.MySqlException ex) when (ex.Number == 1062)
        {
            await Aviso("Ya existe un usuario con ese nombre.");
            return;
        }
        catch (Exception ex)
        {
            await ErrorDb(ex);
            return;
        }
        _actual = u;
        Password.Text = "";
        Reload();
    }
}
