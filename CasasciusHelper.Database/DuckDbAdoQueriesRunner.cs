using B.DatabaseUtils.DuckDb;
using Dapper;
using DuckDB.NET.Data;
using Microsoft.Extensions.Logging;

namespace CasasciusHelper.Database;

public interface IDuckDbAdoQueriesRunner
{
    Task<bool> EnsureDatabaseSchemaV1(CancellationToken cancellationToken = default);

    Task<bool> ImportCasasciusCoinsFromFile(string filename, CancellationToken cancellationToken = default);
}

public class DuckDbAdoAdoQueriesRunner : IDuckDbAdoQueriesRunner
{
    private readonly IDuckDbConnectionProvider dbConnectionProvider;
    private readonly ILogger<DuckDbAdoAdoQueriesRunner> logger;

    public DuckDbAdoAdoQueriesRunner(
        IDuckDbConnectionProvider dbConnectionProvider,
        ILogger<DuckDbAdoAdoQueriesRunner> logger)
    {
        this.dbConnectionProvider = dbConnectionProvider;
        this.logger = logger;
    }

    public async Task<bool> EnsureDatabaseSchemaV1(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = dbConnectionProvider.GetConfiguredOpenConnection();

            await RunCreateCasasciusCoinsTable(connection, cancellationToken: cancellationToken);

            return true;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute DuckDB migration");
            return false;
        }
    }

    public async Task<bool> ImportCasasciusCoinsFromFile(string filename, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = dbConnectionProvider.GetConfiguredOpenConnection();

            await ImportCasasciusCoinsFromFile(filename, connection, cancellationToken: cancellationToken);

            return true;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to import Casascius coins");
            return false;
        }
    }

    private async Task RunCreateCasasciusCoinsTable(DuckDBConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();

        command.CommandText = """
                              CREATE TABLE IF NOT EXISTS "CasasciusCoins"(
                                  "Id" INTEGER NOT NULL PRIMARY KEY,
                                  "Address" VARCHAR(34) NOT NULL UNIQUE, -- actually DuckDb doesn't respect VARCHAR lengths
                                  "Series" TINYINT,
                                  "Type" VARCHAR(14),
                                  "Status" VARCHAR(8) NOT NULL,
                                  "Value" DECIMAL(12, 8),
                                  "Balance" DECIMAL(12, 8) NOT NULL,
                                  "CreateBlock" INTEGER,
                                  "RedeemBlock" INTEGER,
                                  "CreateTime" TIMESTAMP,
                                  "RedeemTime" TIMESTAMP,
                                  "UpdateTime" TIMESTAMP NOT NULL
                              );
                              """;

        await command.ExecuteScalarAsync(cancellationToken);
    }

    /// <summary>
    /// Import CasasciusCoins table data directly from CSV file
    /// </summary>
    /// <remarks>This method simulates MERGE operation (not supported by DuckDb at the moment) by running INSERT and UPDATE</remarks>
    public async Task ImportCasasciusCoinsFromFile(string filename, DuckDBConnection connection, CancellationToken cancellationToken = default)
    {
        var sqlInsert = $"""
                   WITH source AS (
                       SELECT
                           "Index"                     AS "Id",
                           "Address",
                           "Series",
                           "Type",
                           "Status",
                           "Value",
                           "Balance",
                           "Create Block"              AS "CreateBlock",
                           "Redeem Block"              AS "RedeemBlock",
                           TO_TIMESTAMP("Create Time") AS "CreateTime",
                           TO_TIMESTAMP("Redeem Time") AS "RedeemTime",
                           TO_TIMESTAMP("Update Time") AS "UpdateTime"
                       FROM read_csv('{filename}')
                   )

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
                   SELECT
                       s."Id",
                       s."Address",
                       s."Series",
                       s."Type",
                       s."Status",
                       s."Value",
                       s."Balance",
                       s."CreateBlock",
                       s."RedeemBlock",
                       s."CreateTime",
                       s."RedeemTime",
                       s."UpdateTime"
                   FROM source AS s
                   WHERE NOT EXISTS (
                       SELECT 1
                       FROM "CasasciusCoins" AS t
                       WHERE t."Id" = s."Id"
                   );
                   """;

        var sqlUpdate = $"""
                         WITH source AS (
                             SELECT
                                 "Index"                     AS "Id",
                                 "Address",
                                 "Series",
                                 "Type",
                                 "Status",
                                 "Value",
                                 "Balance",
                                 "Create Block"              AS "CreateBlock",
                                 "Redeem Block"              AS "RedeemBlock",
                                 TO_TIMESTAMP("Create Time") AS "CreateTime",
                                 TO_TIMESTAMP("Redeem Time") AS "RedeemTime",
                                 TO_TIMESTAMP("Update Time") AS "UpdateTime"
                             FROM read_csv('{filename}')
                         )

                         UPDATE "CasasciusCoins" AS t
                         SET
                             --"Id", -- Primary key
                             --"Address"     = s."Address", -- Alternative key. Note: it is better to not include alternative keys in MERGE/UPDATE operations
                                                            -- as they potentially may be used in FOREIGN KEY constraints thus making such operations "suddenly start to fail" in the future
                             "Series"      = s."Series",
                             "Type"        = s."Type",
                             "Status"      = s."Status",
                             "Value"       = s."Value",
                             "Balance"     = s."Balance",
                             "CreateBlock" = s."CreateBlock",
                             "RedeemBlock" = s."RedeemBlock",
                             "CreateTime"  = s."CreateTime",
                             "RedeemTime"  = s."RedeemTime",
                             "UpdateTime"  = s."UpdateTime"
                         FROM source AS s
                         WHERE t."Id" = s."Id";
                         """;

        try
        {
            await connection.ExecuteAsync(sqlUpdate); // the more data are in the table, the longer this operation will take
            await connection.ExecuteAsync(sqlInsert);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to import Casascius coins");
            throw;
        }
    }
}
