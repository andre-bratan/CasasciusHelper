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

using DuckDB.NET.Data;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace B.DatabaseUtils.DuckDb;

public interface IDuckDbConnectionChecker : IDatabaseConnectionChecker;

/// <inheritdoc />
public class DuckDbConnectionChecker : IDuckDbConnectionChecker
{
    private readonly IDuckDbConnectionProvider connectionProvider;
    private readonly ILogger<DuckDbConnectionChecker> logger;

    public DuckDbConnectionChecker(
        IDuckDbConnectionProvider connectionProvider,
        ILogger<DuckDbConnectionChecker> logger)
    {
        this.connectionProvider = connectionProvider;
        this.logger = logger;
    }

    public async Task<bool> CheckDatabaseConnection(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var dbConnection = connectionProvider.GetConfiguredOpenConnection();

            await using var command = new DuckDBCommand("SELECT current_setting('TIMEZONE');", dbConnection);
            var result = await command.ExecuteScalarAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database connection check error");
            return false;
        }
    }
}
