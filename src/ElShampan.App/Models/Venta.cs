namespace ElShampan.App.Models;

public class Venta
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public string MetodoPago { get; set; } = "efectivo";
    public List<DetalleVenta> Detalles { get; set; } = new();
}

public class DetalleVenta
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
}
