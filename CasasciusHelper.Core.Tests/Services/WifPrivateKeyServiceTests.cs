using CasasciusHelper.Core.Services;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace CasasciusHelper.Core.Tests.Services;

[TestSubject(typeof(WifPrivateKeyService))]
public class WifPrivateKeyServiceTests
{
    private readonly IWifPrivateKeyService sut = new WifPrivateKeyService();

    [Fact]
    public void Test_WifToAddress()
    {
        var wifKey1 = "5JPy8Zg7z4P7RSLsiqcqyeAF1935zjNUdMxcDeVrtU1oarrgnB7";
        var wifKey2 = "5HueCGU8rMjxEXxiPuD5BDku4MkFqeZyd4dZ1jvhTVqvbTLvyTJ";

        var addressString1 = sut.GetAddressFromWifPrivateKey(wifKey1);
        var addressString2 = sut.GetAddressFromWifPrivateKey(wifKey2);

        addressString1.Should().Be("1CciesT23BNionJeXrbxmjc7ywfiyM4oLW");
        addressString2.Should().Be("1GAehh7TsJAHuUAeKZcXf5CnwuGuGgyX2S");
    }
}
