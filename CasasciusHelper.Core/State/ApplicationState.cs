using System.Text.Json.Serialization;

namespace CasasciusHelper.Core.State;

/// <summary>
/// General application state
/// </summary>
public class ApplicationState : StateBase
{
    /// <summary>
    /// Application version
    /// </summary>
    public string Version { get; set; } = "x.x.x";

    /// <summary>
    /// Application version commit hash
    /// </summary>
    public string CommitHash { get; set; } = "unknown";

    /// <summary>
    /// Application stopping token
    /// </summary>
    [JsonIgnore]
    public CancellationToken StoppingToken { get; set; }

    /// <summary>
    /// General status line
    /// </summary>
    public string StatusLine { get; set; } = "";

    /// <summary>
    /// Database connectivity check result
    /// </summary>
    public bool DatabaseOk { get; set; }

    /// <summary>
    /// Database migrations result
    /// </summary>
    public bool DatabaseMigrationsOk { get; set; }

    /// <summary>
    /// Duck Db version
    /// </summary>
    public string? DuckDbVersion { get; set; }

    /// <summary>
    /// Caches warm up result
    /// </summary>
    public bool CachesWarmedUp { get; set; }

    /// <summary>
    /// Application database has data
    /// </summary>
    public bool HasData { get; set; }

    /// <summary>
    /// Last data update timestamp
    /// </summary>
    public DateTime? LastDataUpdate { get; set; }

    /// <summary>
    /// Set when the application is ready to display its UI and receive API calls
    /// </summary>
    public bool IsHealthy => DatabaseOk && DatabaseMigrationsOk && CachesWarmedUp;

    /// <summary>
    /// Set when the application is able to process data
    /// </summary>
    public bool IsReady => IsHealthy && HasData;
}
