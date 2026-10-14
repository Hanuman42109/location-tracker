using System.Collections.Specialized;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

// "Map" also exists in Microsoft.Maui.ApplicationModel, so the alias removes the ambiguity.
using Map = Microsoft.Maui.Controls.Maps.Map;

namespace HeatMapTracker.Controls;

/// <summary>
/// Extends the .NET MAUI <see cref="Map"/> with heat-map rendering.
/// </summary>
/// <remarks>
/// Algorithm:
/// 1. The world is divided into a square grid (<see cref="CellSizeMeters"/> per side).
/// 2. Every point is assigned to a grid cell; each cell counts its points ("visits").
/// 3. Each cell is drawn as a semi-transparent <see cref="Circle"/> whose colour goes from
///    blue (cold) to red (hot) according to its count relative to the busiest cell.
/// Using the built-in map elements keeps the control cross-platform (Google Maps on Android,
/// Apple Maps on iOS) without any custom native renderer.
/// </remarks>
public class HeatMap : Map
{
    /// <summary>Approximate length of one degree of latitude in meters.</summary>
    private const double MetersPerDegreeLatitude = 111_320.0;

    /// <summary>Upper bound of drawn circles, to keep the map responsive.</summary>
    private const int MaxRenderedCells = 1500;

    /// <summary>
    /// A cell with this many points (or the busiest cell, if higher) is drawn fully red.
    /// Prevents a handful of samples from looking "hot".
    /// </summary>
    private const int MinSaturationCount = 5;

    private const float CircleFillAlpha = 0.6f;

    /// <summary>Half-width of the map view when it first jumps to the latest point.</summary>
    private const double InitialViewRadiusMeters = 400;

    /// <summary>Colour ramp: cold (blue) -> cyan -> green -> yellow -> hot (red).</summary>
    private static readonly Color[] GradientStops =
    [
        new Color(0f, 0f, 1f),
        new Color(0f, 1f, 1f),
        new Color(0f, 1f, 0f),
        new Color(1f, 1f, 0f),
        new Color(1f, 0f, 0f),
    ];

    /// <summary>The points to aggregate. Supports <see cref="INotifyCollectionChanged"/> collections.</summary>
    public static readonly BindableProperty HeatPointsProperty = BindableProperty.Create(
        nameof(HeatPoints),
        typeof(IEnumerable<Location>),
        typeof(HeatMap),
        defaultValue: null,
        propertyChanged: OnHeatPointsChanged);

    /// <summary>Side length of one grid cell, in meters.</summary>
    public static readonly BindableProperty CellSizeMetersProperty = BindableProperty.Create(
        nameof(CellSizeMeters),
        typeof(double),
        typeof(HeatMap),
        defaultValue: 25.0,
        propertyChanged: (bindable, _, _) => ((HeatMap)bindable).Render());

    /// <summary>When true, the camera moves to the newest point on every update (zoom is preserved).</summary>
    public static readonly BindableProperty FollowLatestPointProperty = BindableProperty.Create(
        nameof(FollowLatestPoint),
        typeof(bool),
        typeof(HeatMap),
        defaultValue: true);

    private bool _hasInitialZoom;

    public IEnumerable<Location>? HeatPoints
    {
        get => (IEnumerable<Location>?)GetValue(HeatPointsProperty);
        set => SetValue(HeatPointsProperty, value);
    }

    public bool FollowLatestPoint
    {
        get => (bool)GetValue(FollowLatestPointProperty);
        set => SetValue(FollowLatestPointProperty, value);
    }

    public double CellSizeMeters
    {
        get => (double)GetValue(CellSizeMetersProperty);
        set => SetValue(CellSizeMetersProperty, value);
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        // The first render may happen before the native map exists; retry the centering now.
        if (Handler is not null && FollowLatestPoint)
        {
            CenterOnLatestPoint();
        }
    }

    private static void OnHeatPointsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var heatMap = (HeatMap)bindable;

        if (oldValue is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -= heatMap.OnCollectionChanged;
        }

        if (newValue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += heatMap.OnCollectionChanged;
        }

        heatMap.Render();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Render();

    /// <summary>Rebuilds all heat circles from the current points.</summary>
    private void Render()
    {
        MapElements.Clear();

        var points = HeatPoints?.ToList();
        if (points is null || points.Count == 0 || CellSizeMeters <= 0)
        {
            return;
        }

        var cells = BuildCells(points);
        int maxCount = cells.Max(cell => cell.Count);
        double saturation = Math.Max(maxCount, MinSaturationCount);

        // Keep only the hottest cells and draw cold ones first so hot ones end up on top.
        var cellsToDraw = cells
            .OrderByDescending(cell => cell.Count)
            .Take(MaxRenderedCells)
            .OrderBy(cell => cell.Count);

        foreach (var cell in cellsToDraw)
        {
            MapElements.Add(new Circle
            {
                Center = new Location(cell.SumLatitude / cell.Count, cell.SumLongitude / cell.Count),
                Radius = Distance.FromMeters(CellSizeMeters),
                FillColor = GetHeatColor(cell.Count / saturation).WithAlpha(CircleFillAlpha),
                StrokeColor = GetHeatColor(cell.Count / saturation).WithAlpha(0.9f),
                StrokeWidth = 2,
            });
        }

        if (FollowLatestPoint)
        {
            CenterOnLatestPoint();
        }
    }

    /// <summary>Groups points into grid cells and counts them.</summary>
    private List<HeatCell> BuildCells(IReadOnlyList<Location> points)
    {
        // Longitude degrees shrink with latitude, so scale the column step using the first point.
        double referenceLatitude = points[0].Latitude * Math.PI / 180.0;
        double latitudeStep = CellSizeMeters / MetersPerDegreeLatitude;
        double longitudeStep = CellSizeMeters / (MetersPerDegreeLatitude * Math.Cos(referenceLatitude));

        var cells = new Dictionary<(long Row, long Column), HeatCell>();

        foreach (var point in points)
        {
            var key = ((long)Math.Floor(point.Latitude / latitudeStep),
                       (long)Math.Floor(point.Longitude / longitudeStep));

            if (!cells.TryGetValue(key, out var cell))
            {
                cell = new HeatCell();
                cells[key] = cell;
            }

            cell.Add(point);
        }

        return [.. cells.Values];
    }

    /// <summary>
    /// Moves the camera to the most recent point. The first move zooms to a street-level view;
    /// later moves keep whatever zoom the user chose.
    /// </summary>
    private void CenterOnLatestPoint()
    {
        var last = HeatPoints?.LastOrDefault();
        if (last is null || Handler is null)
        {
            return;
        }

        var region = VisibleRegion;
        var span = _hasInitialZoom && region is not null
            ? new MapSpan(last, region.LatitudeDegrees, region.LongitudeDegrees)
            : MapSpan.FromCenterAndRadius(last, Distance.FromMeters(InitialViewRadiusMeters));

        MoveToRegion(span);
        _hasInitialZoom = true;
    }

    /// <summary>Maps an intensity in [0,1] onto the colour ramp.</summary>
    private static Color GetHeatColor(double intensity)
    {
        double scaled = Math.Clamp(intensity, 0, 1) * (GradientStops.Length - 1);
        int lowerIndex = (int)Math.Floor(scaled);
        int upperIndex = Math.Min(lowerIndex + 1, GradientStops.Length - 1);
        float t = (float)(scaled - lowerIndex);

        Color from = GradientStops[lowerIndex];
        Color to = GradientStops[upperIndex];

        return new Color(
            from.Red + (to.Red - from.Red) * t,
            from.Green + (to.Green - from.Green) * t,
            from.Blue + (to.Blue - from.Blue) * t);
    }

    /// <summary>Accumulator for one grid cell.</summary>
    private sealed class HeatCell
    {
        public int Count { get; private set; }
        public double SumLatitude { get; private set; }
        public double SumLongitude { get; private set; }

        public void Add(Location point)
        {
            Count++;
            SumLatitude += point.Latitude;
            SumLongitude += point.Longitude;
        }
    }
}
