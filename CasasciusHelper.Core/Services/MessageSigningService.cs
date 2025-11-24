using NBitcoin;
using NBitcoin.BIP322;

namespace CasasciusHelper.Core.Services;

/// <summary>
/// Signs or verifies messages according to BIP322
/// </summary>
public interface IMessageSigningService
{
    string SignMessage(string miniKey, string message);

    bool VerifyMessage(string address, string message, string signature);
}

/// <inheritdoc cref="IMessageSigningService"/>
public class MessageSigningService : IMessageSigningService
{
    private readonly IMiniKeyService miniKeyService;

    public MessageSigningService(
        IMiniKeyService miniKeyService)
    {
        this.miniKeyService = miniKeyService;
    }

    public string SignMessage(string miniKey, string message)
    {
        var wifPrivateKey = miniKeyService.GetWifPrivateKey(miniKey);
        var result = SignMessageInternal(wifPrivateKey, message);

        return result;
    }

    public bool VerifyMessage(string address, string message, string signature)
    {
        var address2 = BitcoinAddress.Create(address, Network.Main);
        var parsedSignature = BIP322Signature.Parse(signature, Network.Main);

        var result = address2.VerifyBIP322(message, parsedSignature);

        return result;
    }

    private string SignMessageInternal(string wifPrivateKey, string message)
    {
        var bitcoinSecret = new BitcoinSecret(wifPrivateKey, Network.Main);
        var address = bitcoinSecret.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
        var signature = bitcoinSecret.PrivateKey.SignBIP322(address, message, SignatureType.Full);
        var result = signature.ToBase64();

        return result;
    }
}
