using Microsoft.Maui.Devices.Sensors;

namespace HeatMapTracker.Services;

/// <summary>
/// <see cref="ILocationTracker"/> implementation based on .NET MAUI Essentials
/// <see cref="Geolocation"/> foreground listening.
/// </summary>
/// <remarks>
/// Foreground listening only delivers updates while the app is visible. Tracking while the app is
/// in the background requires a platform foreground service (Android) or the "location" background
/// mode (iOS) - see README.md, "Next steps".
/// </remarks>
public sealed class LocationTracker : ILocationTracker
{
    /// <summary>Samples less accurate than this (in meters) are discarded.</summary>
    private const double MaxAcceptableAccuracyMeters = 50;

    /// <summary>Requested interval between samples.</summary>
    private static readonly TimeSpan SamplingInterval = TimeSpan.FromSeconds(5);

    /// <inheritdoc />
    public bool IsTracking { get; private set; }

    /// <inheritdoc />
    public event EventHandler<Location>? LocationUpdated;

    /// <inheritdoc />
    public event EventHandler<string>? TrackingFailed;

    /// <inheritdoc />
    public async Task<bool> RequestPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status == PermissionStatus.Granted)
        {
            return true;
        }

        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        return status == PermissionStatus.Granted;
    }

    /// <inheritdoc />
    public async Task<bool> StartAsync()
    {
        if (IsTracking)
        {
            return true;
        }

        if (!await RequestPermissionAsync())
        {
            return false;
        }

        Geolocation.Default.LocationChanged += OnLocationChanged;
        Geolocation.Default.ListeningFailed += OnListeningFailed;

        var request = new GeolocationListeningRequest(GeolocationAccuracy.Best, SamplingInterval);
        IsTracking = await Geolocation.Default.StartListeningForegroundAsync(request);

        if (!IsTracking)
        {
            DetachHandlers();
        }

        return IsTracking;
    }

    /// <inheritdoc />
    public void Stop()
    {
        if (!IsTracking)
        {
            return;
        }

        Geolocation.Default.StopListeningForeground();
        DetachHandlers();
        IsTracking = false;
    }

    private void OnLocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
    {
        // Ignore poor fixes (e.g. cell-tower based) so they do not pollute the heat map.
        if (e.Location.Accuracy is double accuracy && accuracy > MaxAcceptableAccuracyMeters)
        {
            return;
        }

        LocationUpdated?.Invoke(this, e.Location);
    }

    private void OnListeningFailed(object? sender, GeolocationListeningFailedEventArgs e)
    {
        Stop();
        TrackingFailed?.Invoke(this, $"Location updates stopped: {e.Error}");
    }

    private void DetachHandlers()
    {
        Geolocation.Default.LocationChanged -= OnLocationChanged;
        Geolocation.Default.ListeningFailed -= OnListeningFailed;
    }
}
