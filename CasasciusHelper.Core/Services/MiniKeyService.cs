using System.Security.Cryptography;
using System.Text;
using NBitcoin.DataEncoders;

namespace CasasciusHelper.Core.Services;

// Mini Private Keys
// A key is well-formed if contains 30 Base58 characters starting with S and is passing SHA256(Key + "?") hash check.
// - https://en.bitcoin.it/wiki/Mini_private_key_format

// Uses "NBitcoin" NuGet (MIT)
// - https://github.com/MetacoSA/NBitcoin

// If you are interested to understand Base58 encoding - take a look at "Nokitakaze.Base58Check" before digging into "NBitcoin" implementation
// - https://github.com/nokitakaze/Base58Check.Standard
// - https://en.bitcoin.it/wiki/Base58Check_encoding
// - https://en.bitcoin.it/wiki/List_of_address_prefixes

/// <summary>
/// Service for working with mini keys
/// </summary>
public interface IMiniKeyService
{
    /// <inheritdoc cref="MiniKeyService.CheckMiniKey(byte[])"/>
    bool CheckMiniKey(byte[] miniKeyHashBytes);

    /// <inheritdoc cref="MiniKeyService.CheckMiniKey(string)"/>
    [Obsolete]
    bool CheckMiniKey(string miniKey);

    /// <inheritdoc cref="MiniKeyService.CheckMiniKeyAny(byte[])"/>
    bool CheckMiniKeyAny(byte[] miniKeyHashBytes);

    /// <inheritdoc cref="MiniKeyService.GetWifPrivateKey(string, bool)"/>
    string GetWifPrivateKey(string miniKey, bool skipCheck = false);

    /// <inheritdoc cref="MiniKeyService.GetAddressFromMiniKey(string)"/>
    string GetAddressFromMiniKey(string miniKey);

    /// <inheritdoc cref="MiniKeyService.DeconstructMiniKey(string)"/>
    (string WifPrivateKey, string Address) DeconstructMiniKey(string miniKey);
}

/// <remarks>Warning: This class is not thread-safe!</remarks>
/// <inheritdoc cref="IMiniKeyService"/>
public class MiniKeyService : IMiniKeyService
{
    // NOTE:
    // This class is not thread-safe - it was intentionally made so in favor of performance.
    // To make it thread-safe, we need to use a lock or put arrays into methods

    private readonly IWifPrivateKeyService wifPrivateKeyService;

    public MiniKeyService(IWifPrivateKeyService wifPrivateKeyService)
    {
        this.wifPrivateKeyService = wifPrivateKeyService;
    }

    // ReSharper disable once RedundantExplicitArraySize
    private readonly byte[] miniKeyBytesForCheckLong = new byte[31] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x3F }; // "0x3F" is "?"
    // ReSharper disable once RedundantExplicitArraySize
    private readonly byte[] miniKeyBytesForCheckShort = new byte[23] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x3F }; // "0x3F" is "?"

    /// <summary>
    /// Checks if the mini key is well-formed.
    /// </summary>
    /// <remarks>Both short and long mini keys (22 and 30 bytes long) are supported</remarks>
    public bool CheckMiniKey(byte[] miniKeyHashBytes)
    {
        // Check algorithm: add "?" to the end, compute SHA256 hash, check first byte (it must be 0x00)

        byte[] miniKeyHashForCheck;
        if (miniKeyHashBytes.Length == 30)
        {
            //var miniKeyBytesForCheckLong = new byte[31] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x3F }; // "0x3F" is "?"

            Array.Copy(miniKeyHashBytes, 0, miniKeyBytesForCheckLong, 0, miniKeyHashBytes.Length);

            using var algorithm = SHA256.Create();
            miniKeyHashForCheck = algorithm.ComputeHash(miniKeyBytesForCheckLong);
        }
        else if (miniKeyHashBytes.Length == 22)
        {
            //var miniKeyBytesForCheckShort = new byte[23] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x3F }; // "0x3F" is "?"

            Array.Copy(miniKeyHashBytes, 0, miniKeyBytesForCheckShort, 0, miniKeyHashBytes.Length);

            using var algorithm = SHA256.Create();
            miniKeyHashForCheck = algorithm.ComputeHash(miniKeyBytesForCheckShort);
        }
        else
            return false;

        if (miniKeyHashForCheck[0] != 0)
            return false;

        return true;
    }

    /// <summary>
    /// Checks if the mini key is well-formed.
    /// </summary>
    /// <remarks>This method works with mini keys of any length, but is the slowest</remarks>
    [Obsolete]
    public bool CheckMiniKey(string miniKey)
    {
        var miniKeyForCheck = miniKey + "?";
        var miniKeyCheckHashString = GetHashString(miniKeyForCheck);
        if (!miniKeyCheckHashString.StartsWith("00", StringComparison.InvariantCulture))
            return false;

        return true;
    }

    /// <summary>
    /// Checks if the given mini key is well-formed
    /// </summary>
    /// <remarks>Both short and long mini keys (22 and 30 bytes long) are supported</remarks>
    public bool CheckMiniKeyAny(byte[] miniKeyHashBytes)
    {
        var miniKeyBytesForCheck = miniKeyHashBytes.Concat(new byte[] { 0x3F }).ToArray(); // "0x3F" is "?"

        using var algorithm = SHA256.Create();
        var miniKeyHashForCheck = algorithm.ComputeHash(miniKeyBytesForCheck);
        if (miniKeyHashForCheck[0] != 0)
            return false;

        return true;
    }

    // ReSharper disable once RedundantExplicitArraySize
    private readonly byte[] miniKeyHashExtended = new byte[33] { 0x80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    /// <summary>
    /// Gets a private key in WIF format from the given mini key
    /// </summary>
    /// <remarks>Both short and long mini keys (22 and 30 bytes long) are supported</remarks>
    public string GetWifPrivateKey(string miniKey, bool skipCheck = false)
    {
        var miniKeyHashBytes = Encoding.UTF8.GetBytes(miniKey);

        if (!skipCheck && !CheckMiniKey(miniKeyHashBytes))
            throw new ArgumentException($"Invalid mini key: {miniKey}");

        using var algorithm = SHA256.Create();
        var miniKeyHash = algorithm.ComputeHash(miniKeyHashBytes);

        //var miniKeyHashExtended = new byte[33] { 0x80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        Array.Copy(miniKeyHash, 0,
                   miniKeyHashExtended, 1, // skip first byte (0x80)
                   32);

        var result = Encoders.Base58Check.EncodeData(miniKeyHashExtended);

        return result;
    }

    /// <summary>
    /// Get address from the given mini key
    /// </summary>
    /// <remarks>Both short and long mini keys (22 and 30 bytes long) are supported</remarks>
    public string GetAddressFromMiniKey(string miniKey)
    {
        var result = DeconstructMiniKey(miniKey).Address;

        return result;
    }

    /// <summary>
    /// Get Wif private key and Address from the given mini key
    /// </summary>
    /// <remarks>Both short and long mini keys (22 and 30 bytes long) are supported</remarks>
    public (string WifPrivateKey, string Address) DeconstructMiniKey(string miniKey)
    {
        var wifKey = GetWifPrivateKey(miniKey);
        var address = wifPrivateKeyService.GetAddressFromWifPrivateKey(wifKey);

        return (wifKey, address);
    }

    private string GetHashString(string data)
    {
        var hash = GetHash(data);
        var result = ToHexString(hash);

        return result;
    }

    private byte[] GetHash(string data)
    {
        var dataBytes = Encoding.UTF8.GetBytes(data);

        using var algorithm = SHA256.Create(); // Warning: SHA256 is not thread-safe and its instances must not be cached ever!
        var result = algorithm.ComputeHash(dataBytes);

        return result;
    }

    private static string ToHexString(byte[] bytes)
    {
        var result = BitConverter.ToString(bytes);
        result = result.Replace("-", "");

        return result;
    }
}
