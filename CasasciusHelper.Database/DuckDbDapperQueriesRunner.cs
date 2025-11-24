using B.DatabaseUtils.DuckDb;
using Casascius.Coins;
using CasasciusHelper.Database.Entities;
using Dapper;
using Microsoft.Extensions.Logging;

namespace CasasciusHelper.Database;

public interface IDuckDbDapperQueriesRunner
{
    Task<bool> HasData(CancellationToken cancellationToken = default);

    Task<DateTime?> GetLastUpdateTime(CancellationToken cancellationToken = default);

    /// <remarks>Prefer to use <see cref="DuckDbAdoAdoQueriesRunner.ImportCasasciusCoinsFromFile(string,System.Threading.CancellationToken)"/> instead as it is much faster</remarks>
    [Obsolete]
    Task ImportCasasciusCoins(IEnumerable<CasasciusCoin> coins, CancellationToken cancellationToken = default);

    Task<List<CasasciusCoin>> GetAllCasasciusCoins(CancellationToken cancellationToken = default);
}

public class DuckDbDapperQueriesRunner : IDuckDbDapperQueriesRunner
{
    private readonly IDuckDbConnectionProvider dbConnectionProvider;
    private readonly ILogger<DuckDbDapperQueriesRunner> logger;

    public DuckDbDapperQueriesRunner(
        IDuckDbConnectionProvider dbConnectionProvider,
        ILogger<DuckDbDapperQueriesRunner> logger)
    {
        this.dbConnectionProvider = dbConnectionProvider;
        this.logger = logger;
    }

    /// <summary>
    /// Check if CasasciusCoins table has data
    /// </summary>
    public async Task<bool> HasData(CancellationToken cancellationToken = default)
    {
        await using var connection = dbConnectionProvider.GetConfiguredOpenConnection();
        var sql = """
                  SELECT count(*) FROM "CasasciusCoins";
                  """;
        try
        {
            var result = await connection.QuerySingleAsync<int>(sql);
            return result > 0;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check data presence");
            throw;
        }
    }

    /// <summary>
    /// Gets last update time of CasasciusCoins table
    /// </summary>
    public async Task<DateTime?> GetLastUpdateTime(CancellationToken cancellationToken = default)
    {
        await using var connection = dbConnectionProvider.GetConfiguredOpenConnection();
        var sql = """
                  SELECT max("UpdateTime") FROM "CasasciusCoins";
                  """;
        try
        {
            var result = await connection.QuerySingleAsync<DateTime?>(sql);
            return result;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check data presence");
            throw;
        }
    }

    [Obsolete]
    public async Task ImportCasasciusCoins(IEnumerable<CasasciusCoin> coins, CancellationToken cancellationToken = default)
    {
        await using var connection = dbConnectionProvider.GetConfiguredOpenConnection();
        var sql = """
                  UPDATE "CasasciusCoins" AS t
                  SET
                      --"Address"     = s."Address", -- Alternative key. WARNING: commented because it doesn't work with FOREIGN KEY constraints
                      "Series"      = $Series,
                      "Type"        = $Type,
                      "Status"      = $Status,
                      "Value"       = $Value,
                      "Balance"     = $Balance,
                      "CreateBlock" = $CreateBlock,
                      "RedeemBlock" = $RedeemBlock,
                      "CreateTime"  = $CreateTime,
                      "RedeemTime"  = $RedeemTime,
                      "UpdateTime"  = $UpdateTime
                  WHERE t."Id" = $Id;

                  INSERT INTO "CasasciusCoins" (
                      "Id",
                      "Address",
                      "Series",
                      "Type",
                      "Status",
                      "Value",
                      "Balance",
                      "CreateBlock",
                      "RedeemBlock",
                      "CreateTime",
                      "RedeemTime",
                      "UpdateTime"
                  )
                  SELECT * FROM ((SELECT
                      $Id as "Id",
                      $Address as "Address",
                      $Series as "Series",
                      $Type as "Type",
                      $Status as "Status",
                      $Value as "Value",
                      $Balance as "Balance",
                      $CreateBlock as "CreateBlock",
                      $RedeemBlock as "RedeemBlock",
                      $CreateTime as "CreateTime",
                      $RedeemTime as "RedeemTime",
                      $UpdateTime as "UpdateTime"
                  ))
                  AS s
                  WHERE NOT EXISTS (
                      SELECT 1
                      FROM "CasasciusCoins" AS t
                      WHERE t."Id" = s."Id"
                  );
                  """;

        try
        {
            await connection.ExecuteAsync(sql, coins);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to import Casascius coins");
            throw;
        }
    }

    /// <summary>
    /// Fetch all CasasciusCoins from database
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<CasasciusCoin>> GetAllCasasciusCoins(CancellationToken cancellationToken = default)
    {
        await using var connection = dbConnectionProvider.GetConfiguredOpenConnection();
        var sql = """
                  SELECT * FROM "CasasciusCoins";
                  """;
        try
        {
            var databaseResult = await connection.QueryAsync<CasasciusCoin>(sql);
            var result = databaseResult.ToList();

            // Populate missing values
            foreach (var casasciusCoin in result)
            {
                // Skipped enum transformations:
                // - CasasciusCoin.Series
                // - CasasciusCoin.Type

                if (Enum.TryParse(typeof(CasasciusStatuses), casasciusCoin.Status, true, out var statusParsingResult))
                    casasciusCoin.StatusValue = (CasasciusStatuses)statusParsingResult;
            }

            return result;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to import Casascius coins");
            throw;
        }
    }
}
