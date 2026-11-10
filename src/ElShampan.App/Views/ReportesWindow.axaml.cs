using Avalonia.Controls;
using Avalonia.Interactivity;
using ElShampan.App.Data;
using ElShampan.App.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using static ElShampan.App.Services.Dialogos;

namespace ElShampan.App.Views;

public partial class ReportesWindow : Window
{
    public ReportesWindow()
    {
        InitializeComponent();
        Desde.SelectedDate = DateTime.Today.AddDays(-30);
        Hasta.SelectedDate = DateTime.Today;
        SoloMisVentas.IsChecked = !Sesion.Actual!.EsAdmin;
        Consultar();
    }

    private void OnConsultar(object? sender, RoutedEventArgs e) => Consultar();

    private void Consultar()
    {
        if (Desde.SelectedDate is not { } d || Hasta.SelectedDate is not { } h)
        {
            _ = Aviso("Seleccione ambas fechas.");
            return;
        }
        if (d.Date > h.Date)
        {
            _ = Aviso("La fecha 'desde' no puede ser posterior a 'hasta'.");
            return;
        }
        var desde = d.Date;
        var hasta = h.Date.AddDays(1);
        int? usuarioId = SoloMisVentas.IsChecked == true ? Sesion.Actual!.Id : null;
        try
        {
            var ventas = ReporteRepository.Ventas(desde, hasta, usuarioId);
            VentasGrid.ItemsSource = null;
            VentasGrid.ItemsSource = ventas;
            TotalGeneral.Text = ventas.Sum(v => v.Total).ToString("N2");
            TotalEfectivo.Text = ventas.Where(v => v.MetodoPago == "efectivo").Sum(v => v.Total).ToString("N2");
            TotalTarjeta.Text = ventas.Where(v => v.MetodoPago == "tarjeta").Sum(v => v.Total).ToString("N2");

            TopGrid.ItemsSource = null;
            TopGrid.ItemsSource = ReporteRepository.TopProductos(desde, hasta, 5, usuarioId);

            StockGrid.ItemsSource = null;
            StockGrid.ItemsSource = ProductoRepository.GetLowStock();

            var porDia = ventas.GroupBy(v => v.Fecha.Date).OrderBy(g => g.Key).ToList();
            VentasChart.Series = new ISeries[]
            {
                new ColumnSeries<decimal> { Values = porDia.Select(g => g.Sum(v => v.Total)).ToArray() }
            };
            var ejeTexto = new SolidColorPaint(SKColors.Black);
            VentasChart.XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = porDia.Select(g => g.Key.ToString("dd/MM")).ToArray(),
                    LabelsRotation = 45,
                    LabelsPaint = ejeTexto,
                }
            };
            VentasChart.YAxes = new Axis[] { new Axis { LabelsPaint = ejeTexto } };

            PagoChart.Series = ventas.GroupBy(v => v.MetodoPago)
                .Select(g => (ISeries)new PieSeries<decimal>
                {
                    Name = g.Key,
                    Values = new[] { g.Sum(v => v.Total) },
                }).ToArray();
        }
        catch (Exception ex)
        {
            _ = ErrorDb(ex);
        }
    }
}
