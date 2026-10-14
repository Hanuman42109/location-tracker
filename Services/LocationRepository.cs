using HeatMapTracker.Models;
using SQLite;

namespace HeatMapTracker.Services;

/// <summary>
/// SQLite implementation of <see cref="ILocationRepository"/> using sqlite-net.
/// The connection and table are created lazily on first use.
/// </summary>
public sealed class LocationRepository : ILocationRepository
{
    private const string DatabaseFileName = "locations.db3";

    private const SQLiteOpenFlags OpenFlags =
        SQLiteOpenFlags.ReadWrite |
        SQLiteOpenFlags.Create |
        SQLiteOpenFlags.SharedCache;

    private static readonly string DatabasePath =
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName);

    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private SQLiteAsyncConnection? _connection;

    /// <inheritdoc />
    public async Task InsertAsync(LocationRecord record)
    {
        var connection = await GetConnectionAsync();
        await connection.InsertAsync(record);
    }

    /// <inheritdoc />
    public async Task<List<LocationRecord>> GetAllAsync()
    {
        var connection = await GetConnectionAsync();
        return await connection.Table<LocationRecord>()
                               .OrderBy(record => record.TimestampUtc)
                               .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<int> DeleteAllAsync()
    {
        var connection = await GetConnectionAsync();
        return await connection.DeleteAllAsync<LocationRecord>();
    }

    /// <summary>
    /// Opens the database and creates the table on the first call (thread-safe).
    /// </summary>
    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null)
        {
            return _connection;
        }

        await _initializationLock.WaitAsync();
        try
        {
            if (_connection is null)
            {
                var connection = new SQLiteAsyncConnection(DatabasePath, OpenFlags);
                await connection.CreateTableAsync<LocationRecord>();
                _connection = connection;
            }

            return _connection;
        }
        finally
        {
            _initializationLock.Release();
        }
    }
}
