using HeatMapTracker.Models;

namespace HeatMapTracker.Services;

/// <summary>
/// Persistence abstraction for location samples.
/// </summary>
public interface ILocationRepository
{
    /// <summary>Stores a new location sample.</summary>
    Task InsertAsync(LocationRecord record);

    /// <summary>Returns every stored sample ordered by time (oldest first).</summary>
    Task<List<LocationRecord>> GetAllAsync();

    /// <summary>Removes all stored samples and returns the number of deleted rows.</summary>
    Task<int> DeleteAllAsync();
}
