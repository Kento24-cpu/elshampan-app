namespace ElShampan.App.Models;

public class Compra
{
    public int Id { get; set; }
    public int ProveedorId { get; set; }
    public int UsuarioId { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public List<DetalleCompra> Detalles { get; set; } = new();
}

public class DetalleCompra
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
}
