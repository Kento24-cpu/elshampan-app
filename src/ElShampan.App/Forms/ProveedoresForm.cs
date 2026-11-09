using ElShampan.App.Data;
using ElShampan.App.Models;
using Krypton.Toolkit;
using MySqlConnector;

namespace ElShampan.App.Forms;

public class ProveedoresForm : Form
{
    private readonly KryptonDataGridView _grid = new();
    private readonly Panel _panel = new();
    private readonly List<Proveedor> _proveedores = new();
    private Proveedor? _actual;

    private readonly KryptonTextBox _nombre = new();
    private readonly KryptonTextBox _telefono = new();
    private readonly KryptonTextBox _email = new();
    private readonly KryptonTextBox _direccion = new();

    public ProveedoresForm()
    {
        Text = "Proveedores";
        ClientSize = new Size(820, 440);
        StartPosition = FormStartPosition.CenterParent;

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 38, Padding = new Padding(6) };
        var btnNuevo = new KryptonButton { Text = "Nuevo" };
        var btnGuardar = new KryptonButton { Text = "Guardar" };
        var btnEliminar = new KryptonButton { Text = "Eliminar" };
        top.Controls.AddRange(new Control[] { btnNuevo, btnGuardar, btnEliminar });

        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AutoGenerateColumns = false;
        _grid.RowHeadersVisible = false;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", Visible = false });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", DataPropertyName = "Nombre", Width = 220 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Telefono", DataPropertyName = "Telefono", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Email", DataPropertyName = "Email", Width = 170 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Direccion", DataPropertyName = "Direccion", Width = 200 });

        _panel.Dock = DockStyle.Right;
        _panel.Width = 280;
        _panel.AutoScroll = true;
        int y = 12;
        y = AddField("Nombre:", _nombre, y);
        y = AddField("Telefono:", _telefono, y);
        y = AddField("Email:", _email, y);
        y = AddField("Direccion:", _direccion, y);
        _panel.Controls.AddRange(new Control[] { _nombre, _telefono, _email, _direccion });

        var body = new Panel { Dock = DockStyle.Fill };
        body.Controls.Add(_grid);
        body.Controls.Add(_panel);
        body.Controls.Add(top);

        Controls.Add(body);

        btnNuevo.Click += (_, _) => Nuevo();
        btnGuardar.Click += (_, _) => Guardar();
        btnEliminar.Click += (_, _) => Eliminar();
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
            var data = ProveedorRepository.GetAll();
            _proveedores.Clear();
            _proveedores.AddRange(data);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error de base de datos");
            return;
        }
        _grid.DataSource = null;
        _grid.DataSource = _proveedores;
    }

    private void Nuevo()
    {
        _actual = null;
        _nombre.Text = "";
        _telefono.Text = "";
        _email.Text = "";
        _direccion.Text = "";
        _nombre.Focus();
    }

    private void Seleccionar()
    {
        if (_grid.CurrentRow?.DataBoundItem is not Proveedor p) return;
        _actual = p;
        _nombre.Text = p.Nombre;
        _telefono.Text = p.Telefono ?? "";
        _email.Text = p.Email ?? "";
        _direccion.Text = p.Direccion ?? "";
    }

    private void Guardar()
    {
        if (_nombre.Text.Trim().Length == 0)
        {
            MessageBox.Show("El nombre es obligatorio.", "Validacion");
            return;
        }
        var p = _actual ?? new Proveedor();
        p.Nombre = _nombre.Text.Trim();
        p.Telefono = string.IsNullOrWhiteSpace(_telefono.Text) ? null : _telefono.Text.Trim();
        p.Email = string.IsNullOrWhiteSpace(_email.Text) ? null : _email.Text.Trim();
        p.Direccion = string.IsNullOrWhiteSpace(_direccion.Text) ? null : _direccion.Text.Trim();
        try
        {
            if (_actual == null) ProveedorRepository.Insert(p);
            else ProveedorRepository.Update(p);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error de base de datos");
            return;
        }
        _actual = p;
        Reload();
    }

    private void Eliminar()
    {
        if (_actual == null)
        {
            MessageBox.Show("Seleccione un proveedor.", "Validacion");
            return;
        }
        var r = MessageBox.Show($"Eliminar proveedor '{_actual.Nombre}'?", "Confirmar", MessageBoxButtons.YesNo);
        if (r != DialogResult.Yes) return;
        try
        {
            ProveedorRepository.Delete(_actual.Id);
        }
        catch (MySqlException ex) when (ex.Number == 1451)
        {
            MessageBox.Show("No se puede eliminar: el proveedor tiene compras registradas.", "Validacion");
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error de base de datos");
            return;
        }
        Nuevo();
        Reload();
    }
}
