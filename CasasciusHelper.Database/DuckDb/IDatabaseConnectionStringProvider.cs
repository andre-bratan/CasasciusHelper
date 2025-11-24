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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace B.DatabaseUtils;

/// <summary>
/// Provides a connection string to the main Application Database
/// </summary>
/// <remarks>
/// Warning: It is wrong to assume there is only one connection string provider in an Application
/// </remarks>
public interface IDatabaseConnectionStringProvider
{
    string GetDatabaseConnectionString(int? customCommandTimeoutInSeconds = null);
}

/// <inheritdoc />
public abstract class DatabaseConnectionStringProviderBase : IDatabaseConnectionStringProvider
{
    // Connection string name can be set by an environment variable. For example (can be included in launchSettings.json):
    // "CONNECTIONSTRING_NAME": "DuckDb_Tests"
    protected const string CONNECTION_STRING_NAME_ENVIRONMENT_VARIABLE = "CONNECTIONSTRING_NAME";

    protected virtual string DefaultConnectionStringName { get; } = "Default";

    private readonly IServiceScopeFactory serviceScopeFactory;

    public DatabaseConnectionStringProviderBase(IServiceScopeFactory serviceScopeFactory)
    {
        this.serviceScopeFactory = serviceScopeFactory;
    }

    public string GetDatabaseConnectionString(int? customCommandTimeoutInSeconds = null)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var result = GetDatabaseConnectionString(serviceProvider, customCommandTimeoutInSeconds);
        return result;
    }

    protected abstract string GetDatabaseConnectionString(
        IServiceProvider serviceProvider,
        int? customCommandTimeoutInSeconds = null
    );

    /// <summary>
    /// Gets a configured connection string name or default
    /// </summary>
    /// <remarks>Environment variable has higher priority than the default value</remarks>
    protected virtual string GetConnectionStringName(IConfiguration configuration)
    {
        var result = configuration.GetValue<string>(CONNECTION_STRING_NAME_ENVIRONMENT_VARIABLE);
        if (string.IsNullOrWhiteSpace(result))
            result = DefaultConnectionStringName;

        return result;
    }

    /// <summary>
    /// Get a connection string from Configuration
    /// </summary>
    protected static string GetConnectionString(IConfiguration configuration, string connectionStringName)
    {
        var result = configuration.GetConnectionString(connectionStringName);
        if (string.IsNullOrWhiteSpace(result))
            throw new Exception($"Database connection string '{connectionStringName}' is missing/misconfigured");

        return result;
    }
}
