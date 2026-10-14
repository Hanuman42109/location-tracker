namespace HeatMapTracker;

/// <summary>
/// Application root. Creates the single window that hosts the <see cref="AppShell"/>.
/// </summary>
public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new(new AppShell());
}
