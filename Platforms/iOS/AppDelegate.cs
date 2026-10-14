using Foundation;

namespace HeatMapTracker;

/// <summary>iOS application delegate; bootstraps the MAUI app.</summary>
[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
