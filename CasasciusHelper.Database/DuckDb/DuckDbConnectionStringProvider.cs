// -----------------------------------------------------------------------------
//  "B.*" - Utility libraries to speed up proof-of-concepts creation
//  Copyright (c) 2022-2025 Andre Bratan
// -----------------------------------------------------------------------------
//
// This file is part of the "B.*" libraries - a set of utilities originally
// created for internal and personal use. It is provided here for use in the
// CasasciusHelper project only, "as is", free of charge as long as this notice
// stays in the codebase.
//
// DISCLAIMER:
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE, OR NON-INFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES, OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//
// Use of this code in CasasciusHelper constitutes your acceptance of the terms
// stated above. If you wish to use parts of the "B.*" libraries in a different
// context or another project, please contact the author to obtain written
// consent.
// -----------------------------------------------------------------------------

using B.DatabaseUtils.DuckDb.Configuration;
using DuckDB.NET.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace B.DatabaseUtils.DuckDb;

public interface IDuckDbConnectionStringProvider : IDatabaseConnectionStringProvider
{
    (Dictionary<string, string> Options, string ConnectionString) GetDatabaseConnectionStringWithOptions();
}

public class DuckDbConnectionStringProvider : DatabaseConnectionStringProviderBase, IDuckDbConnectionStringProvider
{
    // https://duckdb.net/docs/connection-string.html
    // https://duckdb.org/docs/configuration/overview.html - connection string parameters
    // Note: there is also a special "ConnectionStringBuilder" with methods:
    // - DuckDBConnectionStringBuilder.InMemoryConnectionString
    // - DuckDBConnectionStringBuilder.InMemorySharedConnectionString
    // - DuckDBConnectionStringBuilder.InMemorySharedDataSource
    // - DuckDBConnectionStringBuilder.InMemoryDataSource

    private static IReadOnlyDictionary<string, string> DefaultOptions => new Dictionary<string, string>()
    {
        { DuckDbConnectionOptions.OPTION_TIMEZONE, "UTC" },
        //{ DuckDbConnectionOptions.OPTION_ACCESS_MODE, "READ_ONLY"},
        { DuckDbConnectionOptions.OPTION_MEMORY_LIMIT, "1Gb"},
        { DuckDbConnectionOptions.OPTION_TEMP_DIRECTORY, "" }, // empty string (or NULL) disables the temporary directory usage
        //{ DuckDbConnectionOptions.OPTION_FILE_SEARCH_PATH, "" },
        //{ DuckDbConnectionOptions.OPTION_HOME_DIRECTORY, "" },
        //{ DuckDbConnectionOptions.OPTION_SCHEMA, "main" }, // default value is "main"
        //{ DuckDbConnectionOptions.OPTION_SEARCH_PATH, "" }
    };

    private readonly DuckDbSettings configuration;

    public DuckDbConnectionStringProvider(
        IOptions<DuckDbSettings> settings,
        //ILatestConfigurationOptions<DuckDbConfiguration> settings
        IServiceScopeFactory serviceScopeFactory
    ) : base(serviceScopeFactory)
    {
        configuration = settings.Value;
    }

    protected override string GetDatabaseConnectionString(
        IServiceProvider serviceProvider,
        int? customCommandTimeoutInSeconds = null
    )
    {
        if (customCommandTimeoutInSeconds is not null)
            throw new NotSupportedException($"{nameof(customCommandTimeoutInSeconds)} is not supported - use custom Connection String Provider");

        var (options, result) = GetConnectionString();

        return result;
    }

    public (Dictionary<string, string> Options, string ConnectionString) GetDatabaseConnectionStringWithOptions()
    {
        var result = GetConnectionString();

        return result;
    }

    /// <summary>
    /// Constructs a connection string from configuration
    /// </summary>
    private (Dictionary<string, string> Options, string ConnectionString) GetConnectionString()
    {
        var options = GetMergedOptions();

        var dataSource = configuration.DataSource;
        if (string.IsNullOrWhiteSpace(dataSource))
            throw new ArgumentException($"{nameof(dataSource)} is not set");

        string databaseLocation;
        if (dataSource.StartsWith(DuckDBConnectionStringBuilder.InMemoryDataSource, StringComparison.InvariantCultureIgnoreCase)) // "DataSource=:memory:"
            databaseLocation = dataSource; // in-memory database connection
        else
        {
            // supposing "DataSource" is only a filename, taking path from the "HomeDirectory" option
            var homeDirectoryOption = options[DuckDbConnectionOptions.OPTION_HOME_DIRECTORY];
            var filenameWithPath = Path.Combine(homeDirectoryOption, dataSource);
            databaseLocation = filenameWithPath;
        }

        var connectionStringBuilder = new DuckDBConnectionStringBuilder();
        connectionStringBuilder["DataSource"] = databaseLocation; // DuckDBConnectionStringBuilder.DataSourceKey - unfortunatelly is declared as private
        foreach (var (key, value) in options)
            if (!DuckDbConnectionOptions.OptionsToConfigureAfterOpen.Contains(key))
                connectionStringBuilder[key] = value;

        var result = connectionStringBuilder.ConnectionString;

        return (options, result);
    }

    /// <summary>
    /// Merges default options with options from configuration
    /// </summary>
    private Dictionary<string, string> GetMergedOptions()
    {
        var result = new Dictionary<string, string>(DefaultOptions);
        if (!string.IsNullOrWhiteSpace(configuration.FileSearchPath))
            result[DuckDbConnectionOptions.OPTION_FILE_SEARCH_PATH] = configuration.FileSearchPath;
        if (!string.IsNullOrWhiteSpace(configuration.HomeDirectory))
            result[DuckDbConnectionOptions.OPTION_HOME_DIRECTORY] = configuration.HomeDirectory;
        if (!string.IsNullOrWhiteSpace(configuration.MemoryLimit))
            result[DuckDbConnectionOptions.OPTION_MEMORY_LIMIT] = configuration.MemoryLimit;
        if (!string.IsNullOrWhiteSpace(configuration.TempDirectory))
            result[DuckDbConnectionOptions.OPTION_TEMP_DIRECTORY] = configuration.TempDirectory;
        if (!string.IsNullOrWhiteSpace(configuration.TimeZone))
            result[DuckDbConnectionOptions.OPTION_TIMEZONE] = configuration.TimeZone;

        return result;
    }
}
