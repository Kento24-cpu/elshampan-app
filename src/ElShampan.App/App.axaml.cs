using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace ElShampan.App;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        // tema Fluent deja el watermark a 0.4 de opacidad: sobre crema queda en ~2:1
        Resources["TextControlPlaceholderOpacity"] = 1d;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new Views.LoginWindow();
        base.OnFrameworkInitializationCompleted();
    }
}
