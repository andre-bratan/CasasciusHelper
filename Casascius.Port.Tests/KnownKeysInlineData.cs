using System.Collections;
using Casascius.Coins;

namespace Casascius.Port.Tests;

/// <summary>
/// Adapts <see cref="CasasciusKnownCoins.Collection"/> to be used as inline data for xUnit tests.
/// </summary>
public class KnownKeysInlineData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        foreach (var knownCoin in CasasciusKnownCoins.Collection)
        {
            yield return [ knownCoin.Value ];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
