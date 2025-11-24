using System.Text;
using Casascius.Bitcoin;
using Casascius.Coins;
using CasasciusHelper.Core;
using CasasciusHelper.Core.Services;
using FluentAssertions;
using Xunit;

namespace Casascius.Port.Tests;

public class CasasciusPortTests
{
    private readonly MiniKeyService miniKeyService;
    private readonly WifPrivateKeyService wifPrivateKeyService;

    public CasasciusPortTests()
    {
        wifPrivateKeyService = new WifPrivateKeyService();
        miniKeyService = new MiniKeyService(wifPrivateKeyService);
    }

    [Fact]
    public void Test_PortQuality_GenerateRandom()
    {
        var randomMiniKeyPair = MiniKeyPair.CreateRandom(ExtraEntropy.GetEntropy());
        var randomMiniKey = randomMiniKeyPair.MiniKey;
        var miniKeyBytes = Encoding.UTF8.GetBytes(randomMiniKey);

        var miniKeyCheckByPortedCode = MiniKeyPair.IsValidMiniKey(randomMiniKey) == 1;
        var miniKeyCheckByRewrittenCode = miniKeyService.CheckMiniKey(miniKeyBytes);

        miniKeyCheckByPortedCode.Should().BeTrue();
        miniKeyCheckByRewrittenCode.Should().BeTrue();
    }

    [Theory]
    [ClassData(typeof(KnownKeysInlineData))]
    public void Test_PortQuality_BasicKeyOperations(CasasciusKnownCoin knownCoin)
    {
        var miniKeyPair = new MiniKeyPair(knownCoin.MiniKey);

        //var privateKeyHex = miniKeyPair.PrivateKeyHex;
        //var publicKeyHex = miniKeyPair.PublicKeyHex;
        //var publicKeyHash = miniKeyPair.Hash160Hex;

        var keyPair = new KeyPair(miniKeyPair.PrivateKeyBytes);
        //var wifPrivateKey = new Bip38KeyPair(keyPair, txtPassphrase.Text).EncryptedPrivateKey;

        miniKeyPair.PrivateKey.Should().Be(knownCoin.MiniKey);
        miniKeyPair.PrivateKeyBase58.Should().Be(keyPair.PrivateKey); // keyPair.PrivateKey = keyPair.PrivateKeyBase58
        miniKeyPair.PrivateKeyBase58.Should().Be(keyPair.PrivateKeyBase58);
        miniKeyPair.PublicKeyBytes.Should().BeEquivalentTo(keyPair.PublicKeyBytes);
        miniKeyPair.PublicKeyHex.Should().Be(keyPair.PublicKeyHex);
        miniKeyPair.PrivateKeyBytes.Should().BeEquivalentTo(keyPair.PrivateKeyBytes);
        miniKeyPair.PrivateKeyHex.Should().Be(keyPair.PrivateKeyHex);
        miniKeyPair.Hash160Hex.Should().Be(keyPair.Hash160Hex);
        miniKeyPair.AddressBase58.Should().Be(keyPair.AddressBase58);

        miniKeyPair.MiniKey.Should().Be(knownCoin.MiniKey);
        miniKeyPair.PrivateKeyBase58.Should().Be(knownCoin.PrivateKey);
        miniKeyPair.AddressBase58.Should().Be(knownCoin.Address);
    }
}
