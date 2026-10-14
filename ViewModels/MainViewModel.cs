using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HeatMapTracker.Models;
using HeatMapTracker.Services;
using Microsoft.Maui.Devices.Sensors;

namespace HeatMapTracker.ViewModels;

/// <summary>
/// View model of the main page: connects the location tracker, the database and the heat map.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ILocationRepository _repository;
    private readonly ILocationTracker _tracker;
    private readonly List<Location> _points = [];
    private bool _isInitialized;

    /// <summary>All stored points; a new array instance is published on every change.</summary>
    [ObservableProperty]
    private IReadOnlyList<Location> _heatPoints = Array.Empty<Location>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TrackingButtonText))]
    private bool _isTracking;

    /// <summary>Drives <c>Map.IsShowingUser</c>; enabling it without permission can crash Android.</summary>
    [ObservableProperty]
    private bool _isLocationPermissionGranted;

    [ObservableProperty]
    private string _statusText = "Ready";

    public MainViewModel(ILocationRepository repository, ILocationTracker tracker)
    {
        _repository = repository;
        _tracker = tracker;

        _tracker.LocationUpdated += OnLocationUpdated;
        _tracker.TrackingFailed += OnTrackingFailed;
    }

    public string TrackingButtonText => IsTracking ? "Stop tracking" : "Start tracking";

    /// <summary>Requests permission and loads the stored history (runs once).</summary>
    [RelayCommand]
    private async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        IsLocationPermissionGranted = await _tracker.RequestPermissionAsync();

        var records = await _repository.GetAllAsync();
        _points.Clear();
        _points.AddRange(records.Select(r => new Location(r.Latitude, r.Longitude)));
        PublishPoints();
    }

    [RelayCommand]
    private async Task ToggleTrackingAsync()
    {
        if (IsTracking)
        {
            _tracker.Stop();
            IsTracking = false;
            StatusText = $"Stopped - {_points.Count} points stored";
            return;
        }

        IsTracking = await _tracker.StartAsync();
        if (IsTracking)
        {
            IsLocationPermissionGranted = true;
            StatusText = "Tracking...";
        }
        else
        {
            StatusText = "Could not start tracking. Check the location permission and GPS.";
        }
    }

    [RelayCommand]
    private async Task ClearAsync()
    {
        await _repository.DeleteAllAsync();
        _points.Clear();
        PublishPoints();
    }

    /// <summary>Location events may arrive on a worker thread; marshal to the UI thread.</summary>
    private void OnLocationUpdated(object? sender, Location location)
        => _ = MainThread.InvokeOnMainThreadAsync(() => SaveLocationAsync(location));

    private void OnTrackingFailed(object? sender, string message)
        => MainThread.BeginInvokeOnMainThread(() =>
        {
            IsTracking = false;
            StatusText = message;
        });

    private async Task SaveLocationAsync(Location location)
    {
        try
        {
            await _repository.InsertAsync(new LocationRecord
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Accuracy = location.Accuracy,
                TimestampUtc = DateTime.UtcNow,
            });

            _points.Add(new Location(location.Latitude, location.Longitude));
            PublishPoints();
        }
        catch (Exception ex)
        {
            StatusText = $"Could not save location: {ex.Message}";
        }
    }

    private void PublishPoints()
    {
        HeatPoints = _points.ToArray();
        StatusText = $"{_points.Count} points stored";
    }
}
