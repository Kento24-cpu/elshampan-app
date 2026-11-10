using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ElShampan.App.Data;
using ElShampan.App.Services;

namespace ElShampan.App.Views;

public partial class LoginWindow : Window
{
    private bool _loginOk;

    public LoginWindow()
    {
        InitializeComponent();
        KeyDown += (_, e) => { if (e.Key == Avalonia.Input.Key.Enter) OnLogin(this, e); };
        Closed += (_, _) =>
        {
            if (_loginOk) return;
            if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                desktop.Shutdown();
        };
    }

    private void OnLogin(object? sender, RoutedEventArgs e)
    {
        Error.Text = "";
        if (string.IsNullOrEmpty(Usuario.Text) || string.IsNullOrEmpty(Password.Text))
        {
            Error.Text = "Ingrese usuario y contraseña.";
            return;
        }
        try
        {
            var user = UsuarioRepository.Authenticate(Usuario.Text.Trim());
            if (user == null || !user.Activo || !AuthService.Verify(Password.Text, user.PasswordHash))
            {
                Error.Text = "Usuario o contraseña incorrectos.";
                return;
            }
            Sesion.Actual = user;
            _loginOk = true;
            if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var main = new MainWindow();
                desktop.MainWindow = main;
                main.Show();
            }
            Close();
        }
        catch (Exception ex)
        {
            Error.Text = Dialogos.DetalleError(ex);
        }
    }
}
