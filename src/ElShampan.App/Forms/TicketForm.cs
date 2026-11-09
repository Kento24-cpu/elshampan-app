using System.Drawing.Printing;

namespace ElShampan.App.Forms;

public class TicketForm : Form
{
    private readonly string _texto;

    public TicketForm(string texto)
    {
        _texto = texto;
        Text = "Ticket de venta";
        ClientSize = new Size(360, 480);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var txt = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            Font = new Font(FontFamily.GenericMonospace, 10),
            Text = texto,
            WordWrap = false,
        };

        var btnImprimir = new Button { Text = "Imprimir", Dock = DockStyle.Bottom, Height = 34 };
        btnImprimir.Click += (_, _) => Imprimir();

        Controls.Add(txt);
        Controls.Add(btnImprimir);
    }

    private void Imprimir()
    {
        var doc = new PrintDocument();
        doc.PrintPage += (_, e) =>
        {
            var font = new Font(FontFamily.GenericMonospace, 10);
            e.Graphics?.DrawString(_texto, font, Brushes.Black, new PointF(40, 40));
        };
        using var dlg = new PrintDialog { Document = doc, AllowSomePages = false };
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                doc.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error al imprimir");
            }
        }
    }
}
