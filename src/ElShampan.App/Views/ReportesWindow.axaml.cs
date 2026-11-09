using Avalonia.Controls;
using Avalonia.Interactivity;
using ElShampan.App.Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace ElShampan.App.Views;

public partial class ReportesWindow : Window
{
    public ReportesWindow()
    {
        InitializeComponent();
        Desde.SelectedDate = new DateTimeOffset(DateTime.Today.AddDays(-30));
        Hasta.SelectedDate = new DateTimeOffset(DateTime.Today);
        Consultar();
    }

    private void OnConsultar(object? sender, RoutedEventArgs e) => Consultar();

    private void Consultar()
    {
        if (Desde.SelectedDate is not { } d || Hasta.SelectedDate is not { } h)
        {
            _ = MessageBoxManager.GetMessageBoxStandard("Validación", "Seleccione ambas fechas.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning).ShowWindowAsync();
            return;
        }
        if (d.Date > h.Date)
        {
            _ = MessageBoxManager.GetMessageBoxStandard("Validación", "La fecha 'desde' no puede ser posterior a 'hasta'.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning).ShowWindowAsync();
            return;
        }
        var desde = d.Date;
        var hasta = h.Date.AddDays(1);
        try
        {
            var ventas = ReporteRepository.Ventas(desde, hasta);
            VentasGrid.ItemsSource = null;
            VentasGrid.ItemsSource = ventas;
            TotalGeneral.Text = ventas.Sum(v => v.Total).ToString("N2");
            TotalEfectivo.Text = ventas.Where(v => v.MetodoPago == "efectivo").Sum(v => v.Total).ToString("N2");
            TotalTarjeta.Text = ventas.Where(v => v.MetodoPago == "tarjeta").Sum(v => v.Total).ToString("N2");

            TopGrid.ItemsSource = null;
            TopGrid.ItemsSource = ReporteRepository.TopProductos(desde, hasta);

            StockGrid.ItemsSource = null;
            StockGrid.ItemsSource = ProductoRepository.GetLowStock();

            var porDia = ventas.GroupBy(v => v.Fecha.Date).OrderBy(g => g.Key).ToList();
            VentasChart.Series = new ISeries[]
            {
                new ColumnSeries<decimal> { Values = porDia.Select(g => g.Sum(v => v.Total)).ToArray() }
            };
            VentasChart.XAxes = new Axis[]
            {
                new Axis { Labels = porDia.Select(g => g.Key.ToString("dd/MM")).ToArray(), LabelsRotation = 45 }
            };

            PagoChart.Series = ventas.GroupBy(v => v.MetodoPago)
                .Select(g => (ISeries)new PieSeries<decimal>
                {
                    Name = g.Key,
                    Values = new[] { g.Sum(v => v.Total) },
                }).ToArray();
        }
        catch (Exception ex)
        {
            _ = MessageBoxManager.GetMessageBoxStandard("Error de base de datos", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowWindowAsync();
        }
    }
}
