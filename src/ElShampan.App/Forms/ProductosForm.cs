using ElShampan.App.Data;
using ElShampan.App.Models;
using Krypton.Toolkit;
using MySqlConnector;

namespace ElShampan.App.Forms;

public class ProductosForm : Form
{
    private readonly KryptonDataGridView _grid = new();
    private readonly TextBox _busqueda = new();
    private readonly Panel _panel = new();
    private List<Producto> _productos = new();
    private Producto? _actual;

    private readonly KryptonTextBox _codigo = new();
    private readonly KryptonTextBox _nombre = new();
    private readonly KryptonTextBox _categoria = new();
    private readonly KryptonTextBox _descripcion = new();
    private readonly KryptonNumericUpDown _precioCompra = new();
    private readonly KryptonNumericUpDown _precioVenta = new();
    private readonly KryptonNumericUpDown _stock = new();
    private readonly KryptonNumericUpDown _stockMin = new();
    private readonly KryptonCheckBox _activo = new();

    public ProductosForm()
    {
        Text = "Productos";
        ClientSize = new Size(980, 560);
        StartPosition = FormStartPosition.CenterParent;

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 38, Padding = new Padding(6) };
        _busqueda.Width = 220;
        _busqueda.PlaceholderText = "Buscar por nombre o codigo";
        var btnBuscar = new KryptonButton { Text = "Buscar" };
        var btnNuevo = new KryptonButton { Text = "Nuevo" };
        var btnGuardar = new KryptonButton { Text = "Guardar" };
        var btnBaja = new KryptonButton { Text = "Dar de baja" };
        top.Controls.AddRange(new Control[] { _busqueda, btnBuscar, btnNuevo, btnGuardar, btnBaja });

        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AutoGenerateColumns = false;
        _grid.RowHeadersVisible = false;
        AddColumns();

        _panel.Dock = DockStyle.Right;
        _panel.Width = 300;
        _panel.AutoScroll = true;

        int y = 12;
        y = AddField("Codigo:", _codigo, y);
        y = AddField("Nombre:", _nombre, y);
        y = AddField("Categoria:", _categoria, y);
        y = AddField("Descripcion:", _descripcion, y);
        _precioCompra.SetBounds(120, y, 150, 23);
        _precioCompra.DecimalPlaces = 2;
        _precioCompra.Maximum = 10_000_000;
        _panel.Controls.Add(new Label { Text = "Precio compra:", AutoSize = true, Location = new Point(12, y + 3) });
        y += 32;
        _precioVenta.SetBounds(120, y, 150, 23);
        _precioVenta.DecimalPlaces = 2;
        _precioVenta.Maximum = 10_000_000;
        _panel.Controls.Add(new Label { Text = "Precio venta:", AutoSize = true, Location = new Point(12, y + 3) });
        y += 32;
        _stock.SetBounds(120, y, 150, 23);
        _stock.Maximum = 1_000_000;
        _panel.Controls.Add(new Label { Text = "Stock:", AutoSize = true, Location = new Point(12, y + 3) });
        y += 32;
        _stockMin.SetBounds(120, y, 150, 23);
        _stockMin.Maximum = 1_000_000;
        _panel.Controls.Add(new Label { Text = "Stock minimo:", AutoSize = true, Location = new Point(12, y + 3) });
        y += 32;
        _activo.Text = "Activo";
        _activo.SetBounds(120, y, 100, 23);
        _activo.Checked = true;
        _panel.Controls.Add(_activo);
        y += 36;
        _panel.Controls.AddRange(new Control[] { _codigo, _nombre, _categoria, _descripcion });

        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 640 };
        split.Panel1.Controls.Add(_grid);
        split.Panel1.Controls.Add(top);
        split.Panel2.Controls.Add(_panel);

        Controls.Add(split);

        btnBuscar.Click += (_, _) => Reload();
        _busqueda.TextChanged += (_, _) => Reload();
        btnNuevo.Click += (_, _) => Nuevo();
        btnGuardar.Click += (_, _) => Guardar();
        btnBaja.Click += (_, _) => DarDeBaja();
        _grid.SelectionChanged += (_, _) => Seleccionar();

        Reload();
    }

    private int AddField(string label, KryptonTextBox box, int y)
    {
        _panel.Controls.Add(new Label { Text = label, AutoSize = true, Location = new Point(12, y + 3) });
        box.SetBounds(120, y, 150, 23);
        return y + 32;
    }

    private void AddColumns()
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", Visible = false });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Codigo", DataPropertyName = "Codigo", Width = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", DataPropertyName = "Nombre", Width = 240 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Categoria", DataPropertyName = "Categoria", Width = 110 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Precio venta", DataPropertyName = "PrecioVenta", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock", DataPropertyName = "Stock", Width = 60 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock min.", DataPropertyName = "StockMinimo", Width = 70 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Activo", DataPropertyName = "Activo", Width = 60 });
        _grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0 || e.RowIndex >= _grid.Rows.Count) return;
            if (_grid.Rows[e.RowIndex].DataBoundItem is not Producto p) return;
            if (!p.Activo) e.CellStyle!.BackColor = Color.LightGray;
            else if (p.Stock <= p.StockMinimo) e.CellStyle!.BackColor = Color.Orange;
        };
    }

    private void Reload()
    {
        try
        {
            _productos = ProductoRepository.GetAll();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error de base de datos");
            return;
        }
        var term = _busqueda.Text.Trim();
        var lista = term.Length == 0
            ? _productos
            : _productos.Where(p => p.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase)
                                 || p.Codigo.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        _grid.DataSource = null;
        _grid.DataSource = lista;
    }

    private void Nuevo()
    {
        _actual = null;
        _codigo.Text = "";
        _nombre.Text = "";
        _categoria.Text = "";
        _descripcion.Text = "";
        _precioCompra.Value = 0;
        _precioVenta.Value = 0;
        _stock.Value = 0;
        _stockMin.Value = 5;
        _activo.Checked = true;
        _codigo.Focus();
    }

    private void Seleccionar()
    {
        if (_grid.CurrentRow?.DataBoundItem is not Producto p) return;
        _actual = p;
        _codigo.Text = p.Codigo;
        _nombre.Text = p.Nombre;
        _categoria.Text = p.Categoria;
        _descripcion.Text = p.Descripcion ?? "";
        _precioCompra.Value = p.PrecioCompra;
        _precioVenta.Value = p.PrecioVenta;
        _stock.Value = p.Stock;
        _stockMin.Value = p.StockMinimo;
        _activo.Checked = p.Activo;
    }

    private void Guardar()
    {
        if (_codigo.Text.Trim().Length == 0 || _nombre.Text.Trim().Length == 0 || _categoria.Text.Trim().Length == 0)
        {
            MessageBox.Show("Codigo, nombre y categoria son obligatorios.", "Validacion");
            return;
        }
        if (_precioVenta.Value <= 0)
        {
            MessageBox.Show("El precio de venta debe ser mayor a cero.", "Validacion");
            return;
        }
        var p = _actual ?? new Producto();
        p.Codigo = _codigo.Text.Trim();
        p.Nombre = _nombre.Text.Trim();
        p.Categoria = _categoria.Text.Trim();
        p.Descripcion = string.IsNullOrWhiteSpace(_descripcion.Text) ? null : _descripcion.Text.Trim();
        p.PrecioCompra = _precioCompra.Value;
        p.PrecioVenta = _precioVenta.Value;
        p.Stock = (int)_stock.Value;
        p.StockMinimo = (int)_stockMin.Value;
        p.Activo = _activo.Checked;
        try
        {
            if (_actual == null) ProductoRepository.Insert(p);
            else ProductoRepository.Update(p);
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            MessageBox.Show("Ya existe un producto con ese codigo.", "Validacion");
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error de base de datos");
            return;
        }
        _actual = p;
        Reload();
    }

    private void DarDeBaja()
    {
        if (_actual == null || !_actual.Activo)
        {
            MessageBox.Show("Seleccione un producto activo.", "Validacion");
            return;
        }
        var r = MessageBox.Show($"Dar de baja '{_actual.Nombre}'?", "Confirmar", MessageBoxButtons.YesNo);
        if (r != DialogResult.Yes) return;
        _actual.Activo = false;
        try
        {
            ProductoRepository.Update(_actual);
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
