using ElShampan.App.Services;

namespace ElShampan.App.Forms;

public class MainForm : Form
{
    private readonly ToolStripStatusLabel _status = new();

    public MainForm()
    {
        var usuario = Sesion.Actual!;
        Text = $"ElShampan - Sistema de venta de licor [{usuario.Rol}]";
        ClientSize = new Size(1000, 640);
        StartPosition = FormStartPosition.CenterScreen;

        var menu = new MenuStrip();
        if (usuario.EsAdmin)
        {
            var inventario = new ToolStripMenuItem("Inventario");
            inventario.DropDownItems.Add("Productos", null, (_, _) => new ProductosForm().ShowDialog(this));
            inventario.DropDownItems.Add("Compras", null, (_, _) => new ComprasForm().ShowDialog(this));
            menu.Items.Add(inventario);
        }
        var ventas = new ToolStripMenuItem("Ventas");
        ventas.DropDownItems.Add("Punto de venta", null, (_, _) => new PuntoVentaForm().ShowDialog(this));
        menu.Items.Add(ventas);

        var sistema = new ToolStripMenuItem("Sistema");
        sistema.DropDownItems.Add("Cerrar sesion", null, (_, _) => Close());
        menu.Items.Add(sistema);

        var status = new StatusStrip();
        status.Items.Add(_status);
        _status.Text = $"{usuario.NombreCompleto} ({usuario.Rol})";

        Controls.Add(menu);
        Controls.Add(status);
        MainMenuStrip = menu;
    }
}
