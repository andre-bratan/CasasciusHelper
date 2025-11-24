using Casascius.Coins;
using CasasciusHelper.Database;
using CasasciusHelper.Database.Entities;

namespace CasasciusHelper.Core.Data;

/// <summary>
/// Caches CasasciusCoins-related data from database
/// </summary>
public interface ICasasciusDataCache
{
    /// <summary>
    /// Set if the application has data on Casascius coins
    /// </summary>
    bool HasData { get; }

    /// <summary>
    /// Set if the application has all (supposed) data on Casascius coins
    /// </summary>
    bool HasAllData { get; }

    /// <summary>
    /// Last Casascius coins data update timestamp
    /// </summary>
    DateTime? LastUpdateTime { get; }

    /// <summary>
    /// Initialize the cache
    /// </summary>
    Task Initialize(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the given address is a known Casascius coin address
    /// </summary>
    bool IsCasasciusAddress(string address);

    /// <summary>
    /// Search for Casascius coins addresses containing the given substring
    /// </summary>
    List<CasasciusCoin> SearchCasasciusAddresses(string addressFilter);
}

/// <inheritdoc cref="ICasasciusDataCache"/>
public class CasasciusDataCache : ICasasciusDataCache
{
    private static Dictionary<string, CasasciusCoin> casasciusCoins = new();

    private readonly IDuckDbDapperQueriesRunner database;

    public CasasciusDataCache(
        IDuckDbDapperQueriesRunner database
    )
    {
        this.database = database;
    }

    public bool HasData => casasciusCoins.Any();

    public bool HasAllData => casasciusCoins.Count >= CasasciusFacts.TotalNumberOfCoins;

    public DateTime? LastUpdateTime { get; private set; }

    public async Task Initialize(CancellationToken cancellationToken = default)
    {
        // You may also check if the database is empty:
        //var databaseHasData = await database.HasData(cancellationToken);

        var databaseResult = await database.GetAllCasasciusCoins(cancellationToken);
        casasciusCoins = databaseResult.ToDictionary(x => x.Address, x => x);
        LastUpdateTime = await database.GetLastUpdateTime(cancellationToken);
    }

    public bool IsCasasciusAddress(string address) => casasciusCoins.ContainsKey(address);

    public List<CasasciusCoin> SearchCasasciusAddresses(string addressFilter)
    {
        var unsortedResult = string.IsNullOrWhiteSpace(addressFilter)
            ? casasciusCoins.Values
            : casasciusCoins.Values.Where(x => x.Address.IndexOf(addressFilter, StringComparison.InvariantCulture) != -1);
        var result = unsortedResult.OrderBy(x => x.StatusValue).ThenBy(x => x.Address).ToList();

        return result;
    }
}
