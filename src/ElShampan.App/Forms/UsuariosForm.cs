using ElShampan.App.Data;
using ElShampan.App.Models;
using Krypton.Toolkit;
using MySqlConnector;

namespace ElShampan.App.Forms;

public class UsuariosForm : Form
{
    private readonly KryptonDataGridView _grid = new();
    private readonly Panel _panel = new();
    private readonly List<Usuario> _usuarios = new();
    private Usuario? _actual;

    private readonly KryptonTextBox _nombreUsuario = new();
    private readonly KryptonTextBox _nombreCompleto = new();
    private readonly KryptonTextBox _password = new();
    private readonly KryptonComboBox _rol = new();
    private readonly KryptonCheckBox _activo = new();

    public UsuariosForm()
    {
        Text = "Usuarios";
        ClientSize = new Size(820, 460);
        StartPosition = FormStartPosition.CenterParent;

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 38, Padding = new Padding(6) };
        var btnNuevo = new KryptonButton { Text = "Nuevo" };
        var btnGuardar = new KryptonButton { Text = "Guardar" };
        top.Controls.AddRange(new Control[] { btnNuevo, btnGuardar });

        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AutoGenerateColumns = false;
        _grid.RowHeadersVisible = false;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", Visible = false });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Usuario", DataPropertyName = "NombreUsuario", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", DataPropertyName = "NombreCompleto", Width = 200 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Rol", DataPropertyName = "Rol", Width = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Activo", DataPropertyName = "Activo", Width = 60 });

        _panel.Dock = DockStyle.Right;
        _panel.Width = 280;
        _panel.AutoScroll = true;
        int y = 12;
        y = AddField("Usuario:", _nombreUsuario, y);
        y = AddField("Nombre completo:", _nombreCompleto, y);
        y = AddField("Contrasena:", _password, y);

        _panel.Controls.Add(new Label { Text = "Rol:", AutoSize = true, Location = new Point(12, y + 3) });
        _rol.SetBounds(110, y, 155, 23);
        _rol.DropDownStyle = ComboBoxStyle.DropDownList;
        _rol.Items.AddRange(new object[] { "vendedor", "admin" });
        _rol.SelectedIndex = 0;
        _panel.Controls.Add(_rol);
        y += 32;

        _activo.Text = "Activo";
        _activo.SetBounds(110, y, 100, 23);
        _activo.Checked = true;
        _panel.Controls.Add(_activo);
        _panel.Controls.AddRange(new Control[] { _nombreUsuario, _nombreCompleto, _password });

        var body = new Panel { Dock = DockStyle.Fill };
        body.Controls.Add(_grid);
        body.Controls.Add(_panel);
        body.Controls.Add(top);
        Controls.Add(body);

        btnNuevo.Click += (_, _) => Nuevo();
        btnGuardar.Click += (_, _) => Guardar();
        _grid.SelectionChanged += (_, _) => Seleccionar();

        Reload();
    }

    private int AddField(string label, KryptonTextBox box, int y)
    {
        _panel.Controls.Add(new Label { Text = label, AutoSize = true, Location = new Point(12, y + 3) });
        box.SetBounds(110, y, 155, 23);
        return y + 32;
    }

    private void Reload()
    {
        try
        {
            var data = UsuarioRepository.GetAll();
            _usuarios.Clear();
            _usuarios.AddRange(data);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error de base de datos");
            return;
        }
        _grid.DataSource = null;
        _grid.DataSource = _usuarios;
    }

    private void Nuevo()
    {
        _actual = null;
        _nombreUsuario.Text = "";
        _nombreCompleto.Text = "";
        _password.Text = "";
        _rol.SelectedIndex = 0;
        _activo.Checked = true;
        _nombreUsuario.Focus();
    }

    private void Seleccionar()
    {
        if (_grid.CurrentRow?.DataBoundItem is not Usuario u) return;
        _actual = u;
        _nombreUsuario.Text = u.NombreUsuario;
        _nombreCompleto.Text = u.NombreCompleto;
        _password.Text = "";
        _rol.SelectedItem = u.Rol;
        _activo.Checked = u.Activo;
    }

    private void Guardar()
    {
        if (_nombreUsuario.Text.Trim().Length == 0 || _nombreCompleto.Text.Trim().Length == 0)
        {
            MessageBox.Show("Usuario y nombre completo son obligatorios.", "Validacion");
            return;
        }
        var nuevaPassword = _password.Text.Length == 0 ? null : _password.Text;
        if (_actual == null && nuevaPassword == null)
        {
            MessageBox.Show("La contrasena es obligatoria para usuarios nuevos.", "Validacion");
            return;
        }
        if (nuevaPassword != null && nuevaPassword.Length < 6)
        {
            MessageBox.Show("La contrasena debe tener al menos 6 caracteres.", "Validacion");
            return;
        }
        var u = _actual ?? new Usuario();
        u.NombreUsuario = _nombreUsuario.Text.Trim();
        u.NombreCompleto = _nombreCompleto.Text.Trim();
        u.Rol = _rol.SelectedItem!.ToString()!;
        u.Activo = _activo.Checked;
        try
        {
            if (_actual == null) UsuarioRepository.Insert(u, nuevaPassword!);
            else UsuarioRepository.Update(u, nuevaPassword);
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            MessageBox.Show("Ya existe un usuario con ese nombre.", "Validacion");
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error de base de datos");
            return;
        }
        _actual = u;
        _password.Text = "";
        Reload();
    }
}
