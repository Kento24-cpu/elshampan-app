using ElShampan.App.Data;
using ElShampan.App.Services;
using Krypton.Toolkit;
using MySqlConnector;

namespace ElShampan.App.Forms;

public class LoginForm : Form
{
    private readonly KryptonTextBox _usuario = new();
    private readonly KryptonTextBox _password = new();
    private readonly KryptonButton _btnLogin = new();
    private readonly Label _error = new();

    public LoginForm()
    {
        Text = "ElShampan - Inicio de sesion";
        ClientSize = new Size(330, 190);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        Controls.Add(NewLabel("Usuario:", 25, 30));
        _usuario.SetBounds(120, 27, 180, 23);

        Controls.Add(NewLabel("Contrasena:", 25, 65));
        _password.SetBounds(120, 62, 180, 23);
        _password.UseSystemPasswordChar = true;

        _btnLogin.Text = "Ingresar";
        _btnLogin.SetBounds(120, 100, 100, 32);
        _btnLogin.Click += OnLogin;

        _error.ForeColor = Color.Firebrick;
        _error.SetBounds(25, 145, 290, 23);

        Controls.AddRange(new Control[] { _usuario, _password, _btnLogin, _error });
        AcceptButton = _btnLogin;
    }

    private static Label NewLabel(string text, int x, int y)
    {
        var l = new Label { Text = text, AutoSize = true, Location = new Point(x, y + 3) };
        return l;
    }

    private void OnLogin(object? sender, EventArgs e)
    {
        _error.Text = "";
        if (_usuario.Text.Trim().Length == 0 || _password.Text.Length == 0)
        {
            _error.Text = "Ingrese usuario y contrasena.";
            return;
        }
        try
        {
            var user = UsuarioRepository.Authenticate(_usuario.Text.Trim());
            if (user == null || !user.Activo || !AuthService.Verify(_password.Text, user.PasswordHash))
            {
                _error.Text = "Usuario o contrasena incorrectos.";
                return;
            }
            Sesion.Actual = user;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (MySqlException)
        {
            _error.Text = "No hay conexion con la base de datos. Revise appsettings.json.";
        }
        catch (Exception ex)
        {
            _error.Text = ex.Message;
        }
    }
}
