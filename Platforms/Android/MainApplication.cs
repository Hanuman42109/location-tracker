using Android.App;
using Android.Runtime;

namespace HeatMapTracker;

/// <summary>Android application class; bootstraps the MAUI app.</summary>
[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
