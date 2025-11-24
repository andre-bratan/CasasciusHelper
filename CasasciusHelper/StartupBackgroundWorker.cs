using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using B.DatabaseUtils.DuckDb;
using B.DatabaseUtils.DuckDb.Configuration;
using B.DiskUtils;
using CasasciusHelper.Core.Data;
using CasasciusHelper.Core.State;
using CasasciusHelper.Database;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CasasciusHelper;

/// <summary>
/// Background worker responsible for initial checks and startup logic.
/// </summary>
public class StartupBackgroundWorker : BackgroundService
{
    private readonly ApplicationState applicationState;
    private readonly IDuckDbConnectionProvider duckDbConnectionProvider;
    private readonly IDuckDbConnectionChecker duckDbConnectionChecker;
    private readonly IDuckDbAdoQueriesRunner duckDbAdoQueriesRunner;
    private readonly IOptions<DuckDbSettings> duckDbSettings;
    private readonly ICasasciusDataCache casasciusDataCache;
    private readonly IHostApplicationLifetime appLifetime;
    private readonly IHostEnvironment hostEnvironment;
    private readonly IPathUtils pathUtils;
    private readonly IDirectoryAccessChecker directoryAccessChecker;
    private readonly ILogger<StartupBackgroundWorker> logger;

    public StartupBackgroundWorker(
        ApplicationState applicationState,
        IDuckDbConnectionProvider duckDbConnectionProvider,
        IDuckDbConnectionChecker duckDbConnectionChecker,
        IDuckDbAdoQueriesRunner duckDbAdoQueriesRunner,
        IOptions<DuckDbSettings> duckDbSettings,
        ICasasciusDataCache casasciusDataCache,
        IHostApplicationLifetime appLifetime,
        IHostEnvironment hostEnvironment,
        IPathUtils pathUtils,
        IDirectoryAccessChecker directoryAccessChecker,
        ILogger<StartupBackgroundWorker> logger
    )
    {
        this.applicationState = applicationState;
        this.duckDbConnectionProvider = duckDbConnectionProvider;
        this.duckDbConnectionChecker = duckDbConnectionChecker;
        this.duckDbAdoQueriesRunner = duckDbAdoQueriesRunner;
        this.duckDbSettings = duckDbSettings;
        this.casasciusDataCache = casasciusDataCache;
        this.appLifetime = appLifetime;
        this.hostEnvironment = hostEnvironment;
        this.pathUtils = pathUtils;
        this.directoryAccessChecker = directoryAccessChecker;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            applicationState.StoppingToken = stoppingToken;

            SetApplicationVersion();

            await CheckDataLocationPermissions(stoppingToken);

            await CheckDuckDbConfiguration(stoppingToken);

            await MigrateDatabase(stoppingToken);

            await WarmupCaches(stoppingToken);

            logger.LogInformation("Startup checks are done");

            applicationState.StatusLine = "";
            applicationState.NotifyStateChanged();

            return;
        }
        catch (OperationCanceledException) { }
        catch (ApplicationException aex)
        {
            logger.LogError(aex, "Application exception");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception - startup logic has failed");
        }

        // Stop the application in case of a critical error during startup
        appLifetime.StopApplication();
    }

    private void SetApplicationVersion()
    {
        logger.LogWarning("Application Name: {ApplicationName}", hostEnvironment.ApplicationName);
        logger.LogWarning("Startup Environment: {EnvironmentName}", hostEnvironment.EnvironmentName);

        var applicationVersion = Environment.GetEnvironmentVariable("VERSION");

        var commitHash = Environment.GetEnvironmentVariable("COMMIT_SHA");
        var commitHashString = $"Commit: {commitHash}";

        logger.LogWarning("Starting Version v{Version} ({CommitHash})", applicationVersion, commitHashString);

        applicationState.Version = applicationVersion ?? applicationState.Version;
        applicationState.CommitHash = commitHash ?? applicationState.CommitHash;
        applicationState.NotifyStateChanged();
    }

    private async Task CheckDuckDbConfiguration(CancellationToken cancellationToken)
    {
        applicationState.StatusLine = "Starting: Checking DuckDb...";
        applicationState.NotifyStateChanged();

        await using (var duckDbConnection = duckDbConnectionProvider.GetDatabaseConnection())
        {
            applicationState.DuckDbVersion = duckDbConnection.ServerVersion;
            applicationState.NotifyStateChanged();
        }

        var duckDbConnectionCheckResult = await duckDbConnectionChecker.CheckDatabaseConnection(cancellationToken);
        if (!duckDbConnectionCheckResult)
        {
            applicationState.DatabaseOk = false;
            applicationState.NotifyStateChanged();

            throw new ApplicationException("DuckDb connection check failed");
        }

        applicationState.DatabaseOk = true;
        applicationState.NotifyStateChanged();
    }

    private async Task MigrateDatabase(CancellationToken cancellationToken)
    {
        applicationState.StatusLine = "Starting: Migrating database...";
        applicationState.NotifyStateChanged();

        if (!await duckDbAdoQueriesRunner.EnsureDatabaseSchemaV1(cancellationToken))
        {
            applicationState.DatabaseMigrationsOk = false;
            applicationState.NotifyStateChanged();

            throw new ApplicationException("Database Migrations failed");
        }

        applicationState.DatabaseMigrationsOk = true;
        applicationState.NotifyStateChanged();
    }

    private async Task CheckDataLocationPermissions(CancellationToken cancellationToken)
    {
        applicationState.StatusLine = "Starting: Checking Data location permissions...";
        applicationState.NotifyStateChanged();

        // DuckDb home directory
        var homeDirectory = duckDbSettings.Value.HomeDirectory;
        if (string.IsNullOrWhiteSpace(homeDirectory))
            throw new ApplicationException($"{nameof(DuckDbSettings)}.{nameof(DuckDbSettings.HomeDirectory)} configuration value is not set");

        var homeDirectoryNormalized = pathUtils.NormalizePath(homeDirectory);
        if (!Directory.Exists(homeDirectoryNormalized))
            throw new ApplicationException($"{nameof(DuckDbSettings)}.{nameof(DuckDbSettings.HomeDirectory)} configuration value directory does not exist: {homeDirectory}");

        if (!await directoryAccessChecker.CanWriteAndReadFiles(homeDirectoryNormalized, cancellationToken))
            throw new ApplicationException($"{nameof(DuckDbSettings)}.{nameof(DuckDbSettings.HomeDirectory)} directory is not writable: {homeDirectoryNormalized}");

        // DuckDb file search path
        var fileSearchPath = duckDbSettings.Value.FileSearchPath;
        if (string.IsNullOrWhiteSpace(fileSearchPath))
            throw new ApplicationException($"{nameof(DuckDbSettings)}.{nameof(DuckDbSettings.FileSearchPath)} configuration value is not set");

        var fileSearchPathNormalized = pathUtils.NormalizePath(fileSearchPath);
        if (!Directory.Exists(fileSearchPathNormalized))
            throw new ApplicationException($"{nameof(DuckDbSettings)}.{nameof(DuckDbSettings.FileSearchPath)} configuration value directory does not exist: {fileSearchPath}");

        if (!await directoryAccessChecker.CanWriteAndReadFiles(fileSearchPathNormalized, cancellationToken))
            throw new ApplicationException($"{nameof(DuckDbSettings)}.{nameof(DuckDbSettings.FileSearchPath)} directory is not writable: {fileSearchPathNormalized}");
    }

    private async Task WarmupCaches(CancellationToken cancellationToken)
    {
        applicationState.StatusLine = "Starting: Warming up cache...";
        applicationState.NotifyStateChanged();

        await casasciusDataCache.Initialize(cancellationToken);

        applicationState.CachesWarmedUp = true;
        applicationState.HasData = casasciusDataCache.HasData;
        applicationState.LastDataUpdate = casasciusDataCache.LastUpdateTime;
        applicationState.NotifyStateChanged();
    }
}
