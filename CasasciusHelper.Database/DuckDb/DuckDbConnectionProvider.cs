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

using System.Text;
using DuckDB.NET.Data;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace B.DatabaseUtils.DuckDb;

public interface IDuckDbConnectionProvider : IDatabaseConnectionProvider<DuckDBConnection>
{
    /// <summary>
    /// Returns a new fully configured database connection
    /// </summary>
    [MustDisposeResource]
    DuckDBConnection GetConfiguredOpenConnection();
}

public class DuckDbConnectionProvider : IDuckDbConnectionProvider
{
    private readonly IDuckDbConnectionStringProvider connectionStringProvider;

    public DuckDbConnectionProvider(
        IDuckDbConnectionStringProvider connectionStringProvider
        )
    {
        this.connectionStringProvider = connectionStringProvider;
    }

    /// <summary>
    /// Returns a new database connection without any configuration done
    /// </summary>
    /// <remarks>
    /// Warning: You'll get a non-opened (and thus not fully configured) connection! <br/>
    /// Better use <see cref="GetConfiguredOpenConnection"/> method
    /// </remarks>
    public DuckDBConnection GetDatabaseConnection()
    {
        var connectionString = connectionStringProvider.GetDatabaseConnectionString();

        var result = new DuckDBConnection(connectionString);
        return result;
    }

    public DuckDBConnection GetConfiguredOpenConnection()
    {
        const string QUERY_PARAMETER_HOME = "home";
        const string QUERY_PARAMETER_SEARCH = "search";
        const string QUERY_PARAMETER_TZ = "timezone";

        var (options, connectionString) = connectionStringProvider.GetDatabaseConnectionStringWithOptions();

        var result = new DuckDBConnection(connectionString);
        result.Open();

        if (!options.Keys.Intersect(DuckDbConnectionOptions.OptionsToConfigureAfterOpen).Any())
            return result; // nothing to configure

        using var command = result.CreateCommand();

        var commandTextStringBuilder = new StringBuilder();
        if (options.TryGetValue(DuckDbConnectionOptions.OPTION_HOME_DIRECTORY, out var option))
        {
            commandTextStringBuilder.AppendLine($"SET {DuckDbConnectionOptions.OPTION_HOME_DIRECTORY} = ${QUERY_PARAMETER_HOME};");
            command.Parameters.Add(new DuckDBParameter(QUERY_PARAMETER_HOME, option)); // Note: "$x" or "@x" notation in parameter name are not acceptable
        }
        if (options.TryGetValue(DuckDbConnectionOptions.OPTION_FILE_SEARCH_PATH, out var option2))
        {
            commandTextStringBuilder.AppendLine($"SET {DuckDbConnectionOptions.OPTION_FILE_SEARCH_PATH} = ${QUERY_PARAMETER_SEARCH};");
            command.Parameters.Add(new DuckDBParameter(QUERY_PARAMETER_SEARCH, option2)); // Note: "$x" or "@x" notation in parameter name are not acceptable
        }

        if (options.TryGetValue(DuckDbConnectionOptions.OPTION_TIMEZONE, out var option3))
        {
            commandTextStringBuilder.AppendLine($"SET {DuckDbConnectionOptions.OPTION_TIMEZONE} = ${QUERY_PARAMETER_TZ};");
            command.Parameters.Add(new DuckDBParameter(QUERY_PARAMETER_TZ, option3)); // Note: "$x" or "@x" notation in parameter name are not acceptable
        }

        command.CommandText = commandTextStringBuilder.ToString(); // Note: DuckDb uses $ symbol as a query parameter marker instead of traditional @
        command.ExecuteNonQuery();

        return result;
    }
}
