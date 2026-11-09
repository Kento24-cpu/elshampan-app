using ElShampan.App.Data;
using ElShampan.App.Models;
using ElShampan.App.Services;

namespace ElShampan.App.Forms;

public class ComprasForm : Form
{
    private readonly DataGridView _productosGrid = new();
    private readonly DataGridView _carritoGrid = new();
    private readonly TextBox _buscar = new();
    private readonly ComboBox _proveedor = new();
    private readonly NumericUpDown _cantidad = new();
    private readonly NumericUpDown _precioUnitario = new();
    private readonly Label _total = new();
    private readonly List<DetalleCompra> _carrito = new();

    public ComprasForm()
    {
        Text = "Compras a proveedores";
        ClientSize = new Size(980, 640);
        StartPosition = FormStartPosition.CenterParent;

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 38, Padding = new Padding(6) };
        top.Controls.Add(new Label { Text = "Proveedor:", AutoSize = true, Margin = new Padding(0, 8, 4, 0) });
        _proveedor.Width = 220;
        _proveedor.DropDownStyle = ComboBoxStyle.DropDownList;
        var btnGestionar = new Button { Text = "Gestionar proveedores" };
        btnGestionar.Click += (_, _) =>
        {
            new ProveedoresForm().ShowDialog(this);
            CargarProveedores();
        };
        _buscar.Width = 220;
        _buscar.PlaceholderText = "Buscar producto";
        top.Controls.AddRange(new Control[] { _proveedor, btnGestionar, _buscar });

        _productosGrid.Dock = DockStyle.Fill;
        _productosGrid.ReadOnly = true;
        _productosGrid.AllowUserToAddRows = false;
        _productosGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _productosGrid.MultiSelect = false;
        _productosGrid.AutoGenerateColumns = false;
        _productosGrid.RowHeadersVisible = false;
        _productosGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", Visible = false });
        _productosGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Codigo", DataPropertyName = "Codigo", Width = 90 });
        _productosGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", DataPropertyName = "Nombre", Width = 320 });
        _productosGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "P. compra", DataPropertyName = "PrecioCompra", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
        _productosGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock", DataPropertyName = "Stock", Width = 60 });
        _productosGrid.SelectionChanged += (_, _) => PrecioSugerido();

        var cartPanel = new Panel { Dock = DockStyle.Fill };
        InitCarritoGrid();
        cartPanel.Controls.Add(_carritoGrid);

        var controls = new Panel { Dock = DockStyle.Right, Width = 260, AutoScroll = true };
        int y = 12;
        controls.Controls.Add(new Label { Text = "Cantidad:", AutoSize = true, Location = new Point(12, y + 3) });
        _cantidad.SetBounds(120, y, 70, 23);
        _cantidad.Minimum = 1;
        _cantidad.Maximum = 9999;
        controls.Controls.Add(_cantidad);
        y += 32;
        controls.Controls.Add(new Label { Text = "P. unitario:", AutoSize = true, Location = new Point(12, y + 3) });
        _precioUnitario.SetBounds(120, y, 110, 23);
        _precioUnitario.DecimalPlaces = 2;
        _precioUnitario.Maximum = 10_000_000;
        controls.Controls.Add(_precioUnitario);
        y += 34;
        var btnAgregar = new Button { Text = "Agregar al carrito", Location = new Point(12, y), Width = 230 };
        controls.Controls.Add(btnAgregar);
        y += 36;
        var btnQuitar = new Button { Text = "Quitar del carrito", Location = new Point(12, y), Width = 230 };
        controls.Controls.Add(btnQuitar);
        y += 44;
        controls.Controls.Add(new Label { Text = "TOTAL", AutoSize = true, Location = new Point(12, y), Font = new Font(Font, FontStyle.Bold) });
        y += 26;
        _total.AutoSize = true;
        _total.Location = new Point(12, y);
        _total.Font = new Font(Font, FontStyle.Bold);
        _total.ForeColor = Color.DarkGreen;
        controls.Controls.Add(_total);
        y += 44;
        var btnLimpiar = new Button { Text = "Limpiar carrito", Location = new Point(12, y), Width = 230 };
        controls.Controls.Add(btnLimpiar);
        y += 40;
        var btnRegistrar = new Button { Text = "REGISTRAR COMPRA", Location = new Point(12, y), Width = 230, Height = 40 };
        btnRegistrar.Font = new Font(btnRegistrar.Font, FontStyle.Bold);
        controls.Controls.Add(btnRegistrar);

        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 340 };
        bottom.Controls.Add(cartPanel);
        bottom.Controls.Add(controls);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 240,
        };
        split.Panel1.Controls.Add(_productosGrid);
        split.Panel1.Controls.Add(top);
        split.Panel2.Controls.Add(bottom);

        Controls.Add(split);

        _buscar.TextChanged += (_, _) => CargarProductos();
        btnAgregar.Click += (_, _) => Agregar();
        btnQuitar.Click += (_, _) => Quitar();
        btnLimpiar.Click += (_, _) => { _carrito.Clear(); RefrescarCarrito(); };
        btnRegistrar.Click += (_, _) => Registrar();

        CargarProveedores();
        CargarProductos();
        RefrescarCarrito();
    }

    private void InitCarritoGrid()
    {
        _carritoGrid.Dock = DockStyle.Fill;
        _carritoGrid.ReadOnly = true;
        _carritoGrid.AllowUserToAddRows = false;
        _carritoGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _carritoGrid.MultiSelect = false;
        _carritoGrid.AutoGenerateColumns = false;
        _carritoGrid.RowHeadersVisible = false;
        _carritoGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductoNombre", HeaderText = "Producto", Width = 300 });
        _carritoGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Cantidad", HeaderText = "Cant.", Width = 60 });
        _carritoGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "P. unitario", DataPropertyName = "PrecioUnitario", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
        _carritoGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Subtotal", DataPropertyName = "Subtotal", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
    }

    private void CargarProveedores()
    {
        try
        {
            _proveedor.DataSource = null;
            _proveedor.DataSource = ProveedorRepository.GetAll();
            _proveedor.DisplayMember = "Nombre";
            _proveedor.ValueMember = "Id";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error de base de datos");
        }
    }

    private void CargarProductos()
    {
        try
        {
            _productosGrid.DataSource = null;
            _productosGrid.DataSource = ProductoRepository.Search(_buscar.Text.Trim());
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error de base de datos");
        }
    }

    private void PrecioSugerido()
    {
        if (_productosGrid.CurrentRow?.DataBoundItem is Producto p)
            _precioUnitario.Value = p.PrecioCompra;
    }

    private void Agregar()
    {
        if (_productosGrid.CurrentRow?.DataBoundItem is not Producto p) return;
        if (_precioUnitario.Value <= 0)
        {
            MessageBox.Show("Ingrese un precio de compra mayor a cero.", "Validacion");
            return;
        }
        var qty = (int)_cantidad.Value;
        var detalle = _carrito.FirstOrDefault(d => d.ProductoId == p.Id);
        if (detalle != null)
        {
            detalle.Cantidad += qty;
            detalle.PrecioUnitario = _precioUnitario.Value;
        }
        else
        {
            _carrito.Add(new DetalleCompra
            {
                ProductoId = p.Id,
                ProductoNombre = p.Nombre,
                Cantidad = qty,
                PrecioUnitario = _precioUnitario.Value,
            });
        }
        RefrescarCarrito();
    }

    private void Quitar()
    {
        if (_carritoGrid.CurrentRow?.DataBoundItem is not DetalleCompra d) return;
        _carrito.Remove(d);
        RefrescarCarrito();
    }

    private void RefrescarCarrito()
    {
        _carritoGrid.DataSource = null;
        _carritoGrid.DataSource = _carrito;
        _total.Text = _carrito.Sum(d => d.Subtotal).ToString("N2");
    }

    private void Registrar()
    {
        if (_proveedor.SelectedItem is not Proveedor prov)
        {
            MessageBox.Show("Seleccione un proveedor.", "Validacion");
            return;
        }
        if (_carrito.Count == 0)
        {
            MessageBox.Show("El carrito esta vacio.", "Validacion");
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
            MessageBox.Show(ex.Message, "No se pudo registrar la compra");
            CargarProductos();
            return;
        }
        MessageBox.Show($"Compra #{compra.Id} registrada. Stock actualizado.", "Exito");
        _carrito.Clear();
        RefrescarCarrito();
        CargarProductos();
    }
}
