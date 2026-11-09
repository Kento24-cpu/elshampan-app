using System.Text;
using ElShampan.App.Data;
using ElShampan.App.Models;
using ElShampan.App.Services;
using Krypton.Toolkit;

namespace ElShampan.App.Forms;

public class PuntoVentaForm : Form
{
    private readonly KryptonDataGridView _productosGrid = new();
    private readonly KryptonDataGridView _carritoGrid = new();
    private readonly TextBox _buscar = new();
    private readonly KryptonNumericUpDown _cantidad = new();
    private readonly KryptonComboBox _pago = new();
    private readonly Label _total = new();
    private readonly List<DetalleVenta> _carrito = new();

    public PuntoVentaForm()
    {
        Text = "Punto de venta";
        ClientSize = new Size(980, 640);
        StartPosition = FormStartPosition.CenterParent;

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 38, Padding = new Padding(6) };
        _buscar.Width = 240;
        _buscar.PlaceholderText = "Buscar producto (nombre o codigo)";
        top.Controls.Add(_buscar);

        _productosGrid.Dock = DockStyle.Fill;
        _productosGrid.ReadOnly = true;
        _productosGrid.AllowUserToAddRows = false;
        _productosGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _productosGrid.MultiSelect = false;
        _productosGrid.AutoGenerateColumns = false;
        _productosGrid.RowHeadersVisible = false;
        _productosGrid.DoubleClick += (_, _) => Agregar();

        _productosGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", Visible = false });
        _productosGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Codigo", DataPropertyName = "Codigo", Width = 90 });
        _productosGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", DataPropertyName = "Nombre", Width = 300 });
        _productosGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Precio", DataPropertyName = "PrecioVenta", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
        _productosGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock", DataPropertyName = "Stock", Width = 60 });

        var cartPanel = new Panel { Dock = DockStyle.Fill };
        InitCarritoGrid();
        cartPanel.Controls.Add(_carritoGrid);

        var controls = new Panel { Dock = DockStyle.Right, Width = 260, AutoScroll = true };
        int y = 12;
        controls.Controls.Add(new Label { Text = "Cantidad:", AutoSize = true, Location = new Point(12, y + 3) });
        _cantidad.SetBounds(110, y, 80, 23);
        _cantidad.Minimum = 1;
        _cantidad.Maximum = 9999;
        _cantidad.Value = 1;
        controls.Controls.Add(_cantidad);
        y += 34;

        var btnAgregar = new KryptonButton { Text = "Agregar al carrito", Location = new Point(12, y), Width = 230 };
        controls.Controls.Add(btnAgregar);
        y += 36;
        var btnQuitar = new KryptonButton { Text = "Quitar del carrito", Location = new Point(12, y), Width = 230 };
        controls.Controls.Add(btnQuitar);
        y += 44;

        controls.Controls.Add(new Label { Text = "Pago:", AutoSize = true, Location = new Point(12, y + 3) });
        _pago.SetBounds(110, y, 130, 23);
        _pago.DropDownStyle = ComboBoxStyle.DropDownList;
        _pago.Items.AddRange(new object[] { "efectivo", "tarjeta" });
        _pago.SelectedIndex = 0;
        controls.Controls.Add(_pago);
        y += 44;

        controls.Controls.Add(new Label { Text = "TOTAL", AutoSize = true, Location = new Point(12, y), Font = new Font(Font, FontStyle.Bold) });
        y += 26;
        _total.AutoSize = true;
        _total.Location = new Point(12, y);
        _total.Font = new Font(Font, FontStyle.Bold);
        _total.ForeColor = Color.DarkGreen;
        controls.Controls.Add(_total);
        y += 44;

        var btnLimpiar = new KryptonButton { Text = "Limpiar carrito", Location = new Point(12, y), Width = 230 };
        controls.Controls.Add(btnLimpiar);
        y += 40;
        var btnCobrar = new KryptonButton { Text = "COBRAR", Location = new Point(12, y), Width = 230, Height = 40 };
        btnCobrar.Font = new Font(btnCobrar.Font, FontStyle.Bold);
        controls.Controls.Add(btnCobrar);

        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 360 };
        bottom.Controls.Add(cartPanel);
        bottom.Controls.Add(controls);
        cartPanel.Dock = DockStyle.Fill;
        controls.Dock = DockStyle.Right;

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 250,
        };
        split.Panel1.Controls.Add(_productosGrid);
        split.Panel1.Controls.Add(top);
        split.Panel2.Controls.Add(bottom);

        Controls.Add(split);

        _buscar.TextChanged += (_, _) => CargarProductos();
        btnAgregar.Click += (_, _) => Agregar();
        btnQuitar.Click += (_, _) => Quitar();
        btnLimpiar.Click += (_, _) => { _carrito.Clear(); RefrescarCarrito(); };
        btnCobrar.Click += (_, _) => Cobrar();

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
        _carritoGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Precio unit.", DataPropertyName = "PrecioUnitario", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
        _carritoGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Subtotal", DataPropertyName = "Subtotal", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
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

    private void Agregar()
    {
        if (_productosGrid.CurrentRow?.DataBoundItem is not Producto p) return;
        var qty = (int)_cantidad.Value;
        var enCarrito = _carrito.Where(d => d.ProductoId == p.Id).Sum(d => d.Cantidad);
        if (enCarrito + qty > p.Stock)
        {
            MessageBox.Show($"Stock insuficiente. Disponible: {p.Stock}, en carrito: {enCarrito}.", "Validacion");
            return;
        }
        var detalle = _carrito.FirstOrDefault(d => d.ProductoId == p.Id);
        if (detalle != null) detalle.Cantidad += qty;
        else _carrito.Add(new DetalleVenta { ProductoId = p.Id, ProductoNombre = p.Nombre, Cantidad = qty, PrecioUnitario = p.PrecioVenta });
        RefrescarCarrito();
    }

    private void Quitar()
    {
        if (_carritoGrid.CurrentRow?.DataBoundItem is not DetalleVenta d) return;
        _carrito.Remove(d);
        RefrescarCarrito();
    }

    private void RefrescarCarrito()
    {
        _carritoGrid.DataSource = null;
        _carritoGrid.DataSource = _carrito;
        _total.Text = _carrito.Sum(d => d.Subtotal).ToString("N2");
    }

    private void Cobrar()
    {
        if (_carrito.Count == 0)
        {
            MessageBox.Show("El carrito esta vacio.", "Validacion");
            return;
        }
        var venta = new Venta
        {
            UsuarioId = Sesion.Actual!.Id,
            MetodoPago = _pago.SelectedItem!.ToString()!,
            Subtotal = _carrito.Sum(d => d.Subtotal),
            Total = _carrito.Sum(d => d.Subtotal),
            Detalles = new List<DetalleVenta>(_carrito),
        };
        try
        {
            VentaRepository.Registrar(venta);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "No se pudo registrar la venta");
            CargarProductos();
            return;
        }
        venta.Fecha = DateTime.Now;
        var texto = TicketTexto(venta);
        _carrito.Clear();
        RefrescarCarrito();
        CargarProductos();
        new TicketForm(texto).ShowDialog(this);
    }

    private static string TicketTexto(Venta v)
    {
        var sb = new StringBuilder();
        sb.AppendLine("       ELSHAMSPAN");
        sb.AppendLine("    Ticket de venta");
        sb.AppendLine("--------------------------");
        sb.AppendLine($"Venta #{v.Id}  {v.Fecha:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"Vendedor: {Sesion.Actual!.NombreCompleto}");
        sb.AppendLine("--------------------------");
        foreach (var d in v.Detalles)
            sb.AppendLine($"{d.Cantidad,3} x {d.ProductoNombre,-22}{d.Subtotal,10:N2}");
        sb.AppendLine("--------------------------");
        sb.AppendLine($"TOTAL: {v.Total,28:N2}");
        sb.AppendLine($"Pago: {v.MetodoPago}");
        sb.AppendLine();
        sb.AppendLine("     Gracias por su compra");
        return sb.ToString();
    }
}
