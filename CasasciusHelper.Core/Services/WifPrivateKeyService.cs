using NBitcoin;

namespace CasasciusHelper.Core.Services;

// Wallet Import Format (WIF)
// - https://en.bitcoin.it/wiki/Wallet_import_format

// Uses "NBitcoin" NuGet (MIT)
// - https://github.com/MetacoSA/NBitcoin

/// <summary>
/// Service for working with WIF private keys
/// </summary>
public interface IWifPrivateKeyService
{
    /// <inheritdoc cref="WifPrivateKeyService.GetAddressFromWifPrivateKey(string)"/>
    string GetAddressFromWifPrivateKey(string wifPrivateKey);
}

/// <inheritdoc cref="IMiniKeyService"/>
public class WifPrivateKeyService : IWifPrivateKeyService
{
    /// <summary>
    /// Get address from WIF private key
    /// </summary>
    public string GetAddressFromWifPrivateKey(string wifPrivateKey)
    {
        var bitcoinSecret = new BitcoinSecret(base58: wifPrivateKey, Network.Main);
        var key = bitcoinSecret.PrivateKey;

        //var privateKeyHex = key.ToBytes().ToHexString();
        //var publicKeyHex = bitcoinSecret.PubKey.ToHex();
        //var publicKeyHash = bitcoinSecret.PubKeyHash.ToBytes().ToHexString();

        var bitcoinAddress = key.GetAddress(scriptPubKeyType: ScriptPubKeyType.Legacy, network: Network.Main);
        var result = bitcoinAddress.ToString();

        return result;
    }
}
