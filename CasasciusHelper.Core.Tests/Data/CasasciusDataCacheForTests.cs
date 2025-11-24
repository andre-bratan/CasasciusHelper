using Casascius.Coins;
using CasasciusHelper.Core.Data;
using CasasciusHelper.Database.Entities;

namespace CasasciusHelper.Core.Tests.Data;

public class CasasciusDataCacheForTests : ICasasciusDataCache
{
    private static readonly List<CasasciusCoin> CasasciusCoins = new();
    private static readonly DateTime? LastDataFileUpdateTime;

    public bool HasData => CasasciusCoins.Any();

    public bool HasAllData => CasasciusCoins.Count == CasasciusFacts.TotalNumberOfCoins;

    public DateTime? LastUpdateTime => LastDataFileUpdateTime;

    static CasasciusDataCacheForTests()
    {
        var addressesFileLocation = Path.Combine("Data", "CasasciusKnownAddresses.txt");
        var addresses = File.ReadAllLines(addressesFileLocation);

        var counter = 1;
        foreach (var address in addresses)
        {
            CasasciusCoins.Add(new CasasciusCoin
            {
                Id = counter,
                Address = address
            });

            counter++;
        }

        LastDataFileUpdateTime = File.GetLastWriteTimeUtc(addressesFileLocation);
    }

    public Task Initialize(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public bool IsCasasciusAddress(string address) => CasasciusCoins.FirstOrDefault(x => x.Address == address) != null;

    public List<CasasciusCoin> SearchCasasciusAddresses(string addressFilter)
    {
        var result = string.IsNullOrWhiteSpace(addressFilter)
            ? CasasciusCoins.ToList()
            : CasasciusCoins.Where(x => x.Address.IndexOf(addressFilter, StringComparison.InvariantCulture) != -1).ToList();

        return result;
    }
}
