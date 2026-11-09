using ElShampan.App.Forms;

namespace ElShampan.App;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        while (true)
        {
            using var login = new LoginForm();
            if (login.ShowDialog() != DialogResult.OK) return;
            using var main = new MainForm();
            Application.Run(main);
        }
    }
}
