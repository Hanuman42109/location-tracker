using HeatMapTracker.Services;
using HeatMapTracker.ViewModels;
using HeatMapTracker.Views;
using Microsoft.Maui.Controls.Hosting;

namespace HeatMapTracker;

/// <summary>
/// Application entry point: configures the MAUI host and the dependency-injection container.
/// </summary>
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiMaps(); // Registers the handlers for the MAUI Maps control.

#if ANDROID
        // Makes the purple title bar reach the top of the screen, behind the status bar.
        builder.ConfigureMauiHandlers(handlers =>
            handlers.AddHandler<Shell, StatusBarShellRenderer>());
#endif

        // Services (singletons: one database connection and one GPS listener per app run).
        builder.Services.AddSingleton<ILocationRepository, LocationRepository>();
        builder.Services.AddSingleton<ILocationTracker, LocationTracker>();

        // View models and views.
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainView>();

        return builder.Build();
    }
}
