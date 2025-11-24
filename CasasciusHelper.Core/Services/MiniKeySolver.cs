using System.Text;
using CasasciusHelper.Core.Data;
using CasasciusHelper.Core.Utils;

namespace CasasciusHelper.Core.Services;

// Glossary:
// - Key - a Base58 string
// - Key (with some uncertainty) - a Base58 string in which some symbols were replaced with '?' characters
// - Mask - a string with '?' characters at positions of uncertain/unknown characters in the key. Always has the same length as the key
// - UncertaintyContext - an array with positions of uncertain/unknown characters of the key
// Example:
// - Input key: "SG64GZqySYwBm9Kx?3wJ2?"
// - Mask     : "                ?    ?"
// - UncertaintyContext: [ 16, 21 ]

/// <summary>
/// Mini key solution searcher
/// </summary>
/// <remarks>Enumerates through all possible Base58 symbols at unknown places of a given MiniKey</remarks>
public interface IMiniKeySolver
{
    /// <summary>
    /// Get a mask for the given key
    /// </summary>
    /// <remarks>This method is for debugging purposes convenience only</remarks>
    string GetKeyMask(string miniKey);

    /// <summary>
    /// Get an uncertainty context for the given key
    /// </summary>
    int[] GetUncertaintyContext(string miniKey);

    /// <summary>
    /// Search for a solution for the given uncertain key
    /// </summary>
    /// <returns><c>null</c> if there is no solution</returns>
    /// <remarks>This method performs checks of found minikey candidates against the list of known Casascius addresses</remarks>
    string? SearchKey(string miniKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for all possible solutions for the given uncertain key
    /// </summary>
    /// <returns>Empty list if there are no solutions</returns>
    /// <remarks>This method DOES NOT perform any checks of found minikey candidates against the list of known Casascius addresses</remarks>
    List<string> SearchKeys(string miniKey, CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="IMiniKeySolver"/>
/// <remarks>Warning: This class is not thread-safe!</remarks>
public class MiniKeySolver : IMiniKeySolver
{
    private readonly ICasasciusDataCache casasciusDataCache;
    private readonly IMiniKeyService miniKeyService;

    public MiniKeySolver(
        ICasasciusDataCache casasciusDataCache,
        IMiniKeyService miniKeyService)
    {
        this.casasciusDataCache = casasciusDataCache;
        this.miniKeyService = miniKeyService;
    }

    public string GetKeyMask(string miniKey)
    {
        var keyMaskChars = new char[miniKey.Length];
        for (var i = 0; i < miniKey.Length; i++)
        {
            if (!Base58Encoding.AlphabetChars.Contains(miniKey[i]))
                keyMaskChars[i] = '?';
            else
                keyMaskChars[i] = ' ';
        }

        var result = new string(keyMaskChars);

        return result;
    }

    public int[] GetUncertaintyContext(string miniKey)
    {
        var result = new List<int>();
        for (var i = 0; i < miniKey.Length; i++)
        {
            if (!Base58Encoding.AlphabetChars.Contains(miniKey[i]))
                result.Add(i);
        }

        return result.ToArray();
    }

    public string? SearchKey(string miniKey, CancellationToken cancellationToken = default)
    {
        var context = GetUncertaintyContext(miniKey);
        if (context.Length == 0)
        {
            if (CheckMiniKey(miniKey, onlyKnownAddresses: true))
                return miniKey;

            return null;
        }

        variantKey = miniKey.ToCharArray();
        uncertaintyContext = context;

        var results = SearchKeysInternal(0, cancellationToken, stopOnFirst: true);
        var result = results.FirstOrDefault();

        return result;
    }

    public List<string> SearchKeys(string miniKey, CancellationToken cancellationToken = default)
    {
        var context = GetUncertaintyContext(miniKey);
        if (context.Length == 0)
        {
            if (CheckMiniKey(miniKey, onlyKnownAddresses: false))
                return new List<string> { miniKey };

            return new List<string>();
        }

        variantKey = miniKey.ToCharArray();
        uncertaintyContext = context;

        var results = SearchKeysInternal(0, cancellationToken, stopOnFirst: false);
        return results;
    }

    private char[] variantKey = Array.Empty<char>();
    private int[] uncertaintyContext = Array.Empty<int>();
    /// <remarks>Note: <paramref name="stopOnFirst"/> set to <c>true</c> also means additional check against the list of known Casascius addresses will be performed for found minikey candidates!</remarks>
    private List<string> SearchKeysInternal(int currentKeyMaskPosition, CancellationToken cancellationToken, bool stopOnFirst = true)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var nextKeyMaskPosition = currentKeyMaskPosition + 1;
        var keySymbolPosition = uncertaintyContext[currentKeyMaskPosition];

        var result = new List<string>();
        for (var j = 0; j < Base58Encoding.AlphabetChars.Length; j++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            variantKey[keySymbolPosition] = Base58Encoding.AlphabetChars[j];

            if (nextKeyMaskPosition < uncertaintyContext.Length)
            {
                var subResults = SearchKeysInternal(nextKeyMaskPosition, cancellationToken, stopOnFirst); // recursive call
                result.AddRange(subResults);
            }
            else
            {
                var variantKeyString = new string(variantKey);
                if (CheckMiniKey(variantKeyString, onlyKnownAddresses: stopOnFirst))
                    result.Add(variantKeyString);
            }

            if (result.Count > 0 && stopOnFirst)
                break;
        }

        return result;
    }

    private bool CheckMiniKey(string miniKey, bool onlyKnownAddresses = true)
    {
        var variantKeyHashBytes = Encoding.UTF8.GetBytes(miniKey);

        if (miniKeyService.CheckMiniKey(variantKeyHashBytes))
        {
            if (!onlyKnownAddresses)
                return true;

            var variantKeyAddress = miniKeyService.GetAddressFromMiniKey(miniKey);
            if (casasciusDataCache.IsCasasciusAddress(variantKeyAddress))
                return true;
        }

        return false;
    }
}
