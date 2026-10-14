using SQLite;

namespace HeatMapTracker.Models;

/// <summary>
/// A single GPS sample persisted in the SQLite database.
/// </summary>
[Table("LocationRecords")]
public class LocationRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Latitude in decimal degrees (WGS 84).</summary>
    public double Latitude { get; set; }

    /// <summary>Longitude in decimal degrees (WGS 84).</summary>
    public double Longitude { get; set; }

    /// <summary>Horizontal accuracy in meters, if the platform reported it.</summary>
    public double? Accuracy { get; set; }

    /// <summary>UTC time at which the sample was stored.</summary>
    [Indexed]
    public DateTime TimestampUtc { get; set; }
}
