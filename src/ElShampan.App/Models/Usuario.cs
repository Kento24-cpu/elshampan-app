namespace ElShampan.App.Models;

public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string NombreCompleto { get; set; } = "";
    public string Rol { get; set; } = "vendedor";
    public bool Activo { get; set; } = true;
    public bool EsAdmin => Rol == "admin";
}
