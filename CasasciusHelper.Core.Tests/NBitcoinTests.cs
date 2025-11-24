using Casascius.Bitcoin;
using CasasciusHelper.Core.Services;
using FluentAssertions;
using NBitcoin;
using NBitcoin.BIP322;
using Xunit;

namespace CasasciusHelper.Core.Tests;

//
// These tests demonstrate usage of NBitcoin library, for example: Message Signing and Signature Verification according to BIP322
// - https://bips.xyz/322
// - https://github.com/bitcoin/bips/blob/master/bip-0322.mediawiki
// - https://github.com/bitcoin/bips/blob/master/bip-0137.mediawiki
//
// Discussions on NBitcoin dropping the previous signing implementation and implementing BIP322 instead:
// - https://github.com/MetacoSA/NBitcoin/issues/1187
// - https://github.com/MetacoSA/NBitcoin/issues/1094
// - https://github.com/MetacoSA/NBitcoin/pull/1224

public class NBitcoinTests
{
    private readonly IMiniKeyService miniKeyService;

    public NBitcoinTests()
    {
        IWifPrivateKeyService wifPrivateKeyService = new WifPrivateKeyService();
        miniKeyService = new MiniKeyService(wifPrivateKeyService);
    }

    [Fact]
    public void Test_PrivateKeyToWif()
    {
        var network = Network.Main;

        var privateKeyHex = "7865551dbcbf82e783320c1a6eaf17d95aab32e227a6430f1b4a9611f42896bf"; // some random PK
        var privateKeyBytes = Util.HexStringToBytes(privateKeyHex, testingForValidHex: false);
        var privateKeyCompressed = new Key(privateKeyBytes, fCompressedIn: true);
        var privateKeyUncompressed = new Key(privateKeyBytes, fCompressedIn: false);
        var wifCompressed = privateKeyCompressed.GetWif(network);
        var wifUncompressed = privateKeyUncompressed.GetWif(network);
        var wifCompressedString = wifCompressed.ToWif();

        var wifUncompressedString = wifUncompressed.ToWif();

        wifUncompressedString.Should().Be("5JjJvyUjq8XgwUHEyR2M4f4jeminVHDoQr5YJCeHWyznUExEq1c");
        wifCompressedString.Should().Be("L1Fk9EHS3SETHRQgg12wZ91VFiPRmpbVrFd38vPZuLa3h44S2QcS");
    }

    [Fact]
    public void Test_GenerateAddressFromWifPrivateKey()
    {
        var network = Network.Main;
        var scriptPubKeyType = ScriptPubKeyType.Legacy;

        var miniKey = "SMgKFRQ6n64w8gV2dUwYte";
        var wifKey = miniKeyService.GetWifPrivateKey(miniKey);

        var addressOwnImplementation = miniKeyService.GetAddressFromMiniKey(miniKey);

        // Use NBitcoin to get the same Address
        var bitcoinSecret = new BitcoinSecret(wifKey, network);
        var addressNBitcoinPubKey = bitcoinSecret.PubKey.GetAddress(scriptPubKeyType, network); // it is possible to generate other types of addresses (like Segwit) from imported WIF private key
        var addressNBitcoinPrivateKey = bitcoinSecret.PrivateKey.GetAddress(scriptPubKeyType, network);

        addressNBitcoinPubKey.Should().Be(addressNBitcoinPrivateKey); // it doesn't matter if Public or Private key was used to get the Address
        addressOwnImplementation.Should().Be(addressNBitcoinPubKey.ToString());
    }

    /// <summary>
    /// This test is a predecessor of MessageSigningService
    /// </summary>
    /// <remarks>Based on: <a href="https://programmingblockchain.gitbook.io/programmingblockchain/bitcoin_transfer/proof_of_ownership_as_an_authentication_method">Proof of ownership as an authentication method</a></remarks>
    [Fact]
    public void Test_SignMessage()
    {
        var network = Network.Main;
        var miniKey = "SMgKFRQ6n64w8gV2dUwYte";
        var wifPrivateKey = miniKeyService.GetWifPrivateKey(miniKey);

        var message = "I am Craig Wright";

        // Signing
        var bitcoinSecret = new BitcoinSecret(wifPrivateKey, network);
        var address = bitcoinSecret.PubKey.GetAddress(ScriptPubKeyType.Legacy, network); // 1148CztCNPQJdfXt5PGPxffYWrA1CjSoiw
        var signature = bitcoinSecret.PrivateKey.SignBIP322(address, message, SignatureType.Full);
        var signatureString = signature.ToBase64();

        // Verifying
        var address2 = BitcoinAddress.Create("1148CztCNPQJdfXt5PGPxffYWrA1CjSoiw", network);
        var parsedSignature = BIP322Signature.Parse(signatureString, network);

        var isCraigWrigthSatoshi1 = address2.VerifyBIP322(message, signatureString); // string signature parameter
        var isCraigWrigthSatoshi2 = address2.VerifyBIP322(message, parsedSignature); // BIP322Signature parameter

        isCraigWrigthSatoshi1.Should().BeTrue();
        isCraigWrigthSatoshi2.Should().BeTrue();
    }

    [Fact]
    public void Test_MnemonicKey()
    {
        // See more examples of how to work with mnemonics:
        // - https://github.com/MetacoSA/NBitcoin/blob/master/NBitcoin.Tests/MnemonicTests.cs
        // - https://github.com/jackfreemancoder/BitcoinCoreCsharp/blob/main/examples.md

        var network = Network.Main;
        var scriptPubKeyType = ScriptPubKeyType.Legacy;
        //var mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve); // create new random mnemonic
        var mnemonic = new Mnemonic( "dash target inner rubber often roast omit drink payment march palm lobster", Wordlist.English);
        string? passphrase = null;

        var masterExtendedKey = mnemonic.DeriveExtKey(passphrase);
        // OR
        // var seed = mnemonic.DeriveSeed(passphrase);
        // var seedHex = seed.ToHexString();
        // var masterExtendedKey = new ExtKey(seedHex);

        var privateKey = masterExtendedKey.PrivateKey;
        var wifPrivateKey = privateKey.GetWif(network).ToWif(); // Note: extended private key ("extKey.GetWif(network).ToWif()" resulting in "xprv...") will not work here
        var address1 = privateKey.GetAddress(scriptPubKeyType, network);

        var bitcoinSecret = new BitcoinSecret(wifPrivateKey, network);
        var address2 = bitcoinSecret.PrivateKey.GetAddress(scriptPubKeyType, network);

        // Derive the first account external chain key using BIP44 path: m/44'/0'/0'/0/0
        // m / purpose' / coin_type' / account' / change / address_index
        // m - master node
        // 44' - BIP44 (https://github.com/bitcoin/bips/blob/master/bip-0044.mediawiki)
        // 0' - Bitcoin (https://github.com/satoshilabs/slips/blob/master/slip-0044.md)
        // 0' - External (receiving); 1 - Internal (change)
        // 0 - address index in the chain
        var derivedKey = masterExtendedKey.Derive(new KeyPath("44'/0'/0'/0/0"));
        var address3 = derivedKey.PrivateKey.GetAddress(scriptPubKeyType, network).ToString();

        wifPrivateKey.Should().Be("KzwU2WBUu4Qv31nRFcavcQswEmW9dE3JWPovc5HUQZWg6bxdMhbW");
        address1.ToString().Should().Be("17qkvZXTigpx2NbWaXSS7FuP4g8F4hXBvF");
        address1.Should().Be(address2);
        address3.Should().Be("19qqo3P4zibVTAEkGFWxDZ6R8uDn95jX9S");
    }
}
