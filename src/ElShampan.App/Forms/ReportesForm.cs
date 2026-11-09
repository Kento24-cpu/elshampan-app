using ElShampan.App.Data;
using Krypton.Toolkit;

namespace ElShampan.App.Forms;

public class ReportesForm : Form
{
    private readonly KryptonDateTimePicker _desde = new();
    private readonly KryptonDateTimePicker _hasta = new();
    private readonly KryptonDataGridView _ventasGrid = new();
    private readonly KryptonDataGridView _topGrid = new();
    private readonly KryptonDataGridView _stockGrid = new();
    private readonly Label _totalGeneral = new();
    private readonly Label _totalEfectivo = new();
    private readonly Label _totalTarjeta = new();

    public ReportesForm()
    {
        Text = "Reportes de ventas";
        ClientSize = new Size(980, 560);
        StartPosition = FormStartPosition.CenterParent;

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 38, Padding = new Padding(6) };
        top.Controls.Add(new Label { Text = "Desde:", AutoSize = true, Margin = new Padding(0, 8, 4, 0) });
        _desde.Format = DateTimePickerFormat.Short;
        _desde.Width = 110;
        _desde.Value = DateTime.Today.AddDays(-30);
        top.Controls.Add(_desde);
        top.Controls.Add(new Label { Text = "Hasta:", AutoSize = true, Margin = new Padding(10, 8, 4, 0) });
        _hasta.Format = DateTimePickerFormat.Short;
        _hasta.Width = 110;
        _hasta.Value = DateTime.Today;
        top.Controls.Add(_hasta);
        var btnConsultar = new KryptonButton { Text = "Consultar" };
        top.Controls.Add(btnConsultar);

        _ventasGrid.Dock = DockStyle.Fill;
        _ventasGrid.ReadOnly = true;
        _ventasGrid.AllowUserToAddRows = false;
        _ventasGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _ventasGrid.MultiSelect = false;
        _ventasGrid.AutoGenerateColumns = false;
        _ventasGrid.RowHeadersVisible = false;
        _ventasGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "#", DataPropertyName = "Id", Width = 50 });
        _ventasGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fecha", DataPropertyName = "Fecha", Width = 130, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm" } });
        _ventasGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Vendedor", DataPropertyName = "Vendedor", Width = 180 });
        _ventasGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Pago", DataPropertyName = "MetodoPago", Width = 80 });
        _ventasGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", DataPropertyName = "Total", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });

        var side = new Panel { Dock = DockStyle.Right, Width = 340, AutoScroll = true };
        int y = 12;
        side.Controls.Add(new Label { Text = "Resumen del periodo", AutoSize = true, Location = new Point(12, y), Font = new Font(Font, FontStyle.Bold) });
        y += 26;
        y = AddValor("Total general:", _totalGeneral, y, side);
        y = AddValor("Efectivo:", _totalEfectivo, y, side);
        y = AddValor("Tarjeta:", _totalTarjeta, y, side);
        y += 10;

        side.Controls.Add(new Label { Text = "Top 5 productos", AutoSize = true, Location = new Point(12, y), Font = new Font(Font, FontStyle.Bold) });
        y += 24;
        _topGrid.SetBounds(12, y, 310, 130);
        _topGrid.ReadOnly = true;
        _topGrid.AllowUserToAddRows = false;
        _topGrid.AutoGenerateColumns = false;
        _topGrid.RowHeadersVisible = false;
        _topGrid.AllowUserToResizeRows = false;
        _topGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Producto", DataPropertyName = "Nombre", Width = 170 });
        _topGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cant.", DataPropertyName = "Cantidad", Width = 50 });
        _topGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", DataPropertyName = "Total", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
        side.Controls.Add(_topGrid);
        y += 142;

        side.Controls.Add(new Label { Text = "Stock bajo (alerta)", AutoSize = true, Location = new Point(12, y), Font = new Font(Font, FontStyle.Bold) });
        y += 24;
        _stockGrid.SetBounds(12, y, 310, 150);
        _stockGrid.ReadOnly = true;
        _stockGrid.AllowUserToAddRows = false;
        _stockGrid.AutoGenerateColumns = false;
        _stockGrid.RowHeadersVisible = false;
        _stockGrid.AllowUserToResizeRows = false;
        _stockGrid.DefaultCellStyle.BackColor = Color.Orange;
        _stockGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Producto", DataPropertyName = "Nombre", Width = 180 });
        _stockGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock", DataPropertyName = "Stock", Width = 50 });
        _stockGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Min.", DataPropertyName = "StockMinimo", Width = 60 });
        side.Controls.Add(_stockGrid);

        Controls.Add(_ventasGrid);
        Controls.Add(side);
        Controls.Add(top);

        btnConsultar.Click += (_, _) => Consultar();

        Consultar();
    }

    private static int AddValor(string label, Label valor, int y, Panel side)
    {
        side.Controls.Add(new Label { Text = label, AutoSize = true, Location = new Point(12, y + 3) });
        valor.AutoSize = true;
        valor.Location = new Point(170, y + 3);
        valor.Font = new Font(valor.Font, FontStyle.Bold);
        valor.ForeColor = Color.DarkGreen;
        side.Controls.Add(valor);
        return y + 28;
    }

    private void Consultar()
    {
        if (_desde.Value.Date > _hasta.Value.Date)
        {
            MessageBox.Show("La fecha 'desde' no puede ser posterior a 'hasta'.", "Validacion");
            return;
        }
        var desde = _desde.Value.Date;
        var hasta = _hasta.Value.Date.AddDays(1);
        try
        {
            var ventas = ReporteRepository.Ventas(desde, hasta);
            _ventasGrid.DataSource = null;
            _ventasGrid.DataSource = ventas;
            _totalGeneral.Text = ventas.Sum(v => v.Total).ToString("N2");
            _totalEfectivo.Text = ventas.Where(v => v.MetodoPago == "efectivo").Sum(v => v.Total).ToString("N2");
            _totalTarjeta.Text = ventas.Where(v => v.MetodoPago == "tarjeta").Sum(v => v.Total).ToString("N2");

            _topGrid.DataSource = null;
            _topGrid.DataSource = ReporteRepository.TopProductos(desde, hasta);

            _stockGrid.DataSource = null;
            _stockGrid.DataSource = ProductoRepository.GetLowStock();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error de base de datos");
        }
    }
}
