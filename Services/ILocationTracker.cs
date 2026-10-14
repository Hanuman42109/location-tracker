using Microsoft.Maui.Devices.Sensors;

namespace HeatMapTracker.Services;

/// <summary>
/// Abstraction over the device's location sensor.
/// </summary>
public interface ILocationTracker
{
    /// <summary>True while the tracker is listening for location updates.</summary>
    bool IsTracking { get; }

    /// <summary>Raised for every accurate-enough location sample (may fire on a background thread).</summary>
    event EventHandler<Location>? LocationUpdated;

    /// <summary>Raised when the platform stops delivering updates; the argument is a readable reason.</summary>
    event EventHandler<string>? TrackingFailed;

    /// <summary>Checks, and if necessary requests, the "location when in use" permission.</summary>
    Task<bool> RequestPermissionAsync();

    /// <summary>Starts listening. Returns false if permission was denied or the sensor could not start.</summary>
    Task<bool> StartAsync();

    /// <summary>Stops listening.</summary>
    void Stop();
}
