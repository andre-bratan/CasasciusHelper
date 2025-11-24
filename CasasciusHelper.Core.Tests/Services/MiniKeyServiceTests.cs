using System.Text;
using CasasciusHelper.Core.Services;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace CasasciusHelper.Core.Tests.Services;

// Note: These tests are intentionally made integration-like

[TestSubject(typeof(MiniKeyService))]
public class MiniKeyServiceTests
{
    private readonly IMiniKeyService sut = new MiniKeyService(new WifPrivateKeyService());

    [Fact]
    public void Test_CheckMiniKey_Bytes()
    {
        var correctMiniKeyLong = "S6c56bnXQiBjk9mqSYE7ykVQ7NzrRy";
        var wrongMiniKeyLong = "S6c56bnXQiBjk9mqSYE7ykVQ7NzrRa"; // last character was changed
        var correctMiniKeyShort = "SG64GZqySYwBm9KxE3wJ29";
        var wrongMiniKeyShort = "SMgKPPQ6n64wSgV2dUwYte"; // random key

        var correctMiniKeyLongHashBytes = Encoding.UTF8.GetBytes(correctMiniKeyLong);
        var correctMiniKeyShortHashBytes = Encoding.UTF8.GetBytes(correctMiniKeyShort);
        var wrongMiniKeyLongHashBytes = Encoding.UTF8.GetBytes(wrongMiniKeyLong);
        var wrongMiniKeyShortHashBytes = Encoding.UTF8.GetBytes(wrongMiniKeyShort);

        var result1 = sut.CheckMiniKey(correctMiniKeyLongHashBytes);
        var result2 = sut.CheckMiniKey(correctMiniKeyShortHashBytes);
        var result3 = sut.CheckMiniKey(wrongMiniKeyLongHashBytes);
        var result4 = sut.CheckMiniKey(wrongMiniKeyShortHashBytes);

        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeFalse();
        result4.Should().BeFalse();
    }

    [Fact]
    public void Test_CheckMiniKey_Text()
    {
        var correctMiniKeyLong = "S6c56bnXQiBjk9mqSYE7ykVQ7NzrRy";
        var wrongMiniKeyLong = "S6c56bnXQiBjk9mqSYE7ykVQ7NzrRa"; // last character was changed
        var correctMiniKeyShort = "SG64GZqySYwBm9KxE3wJ29";
        var wrongMiniKeyShort = "SMgKPPQ6n64wSgV2dUwYte"; // random key

#pragma warning disable CS0612 // Type or member is obsolete
        var result1 = sut.CheckMiniKey(correctMiniKeyLong);
        var result2 = sut.CheckMiniKey(correctMiniKeyShort);
        var result3 = sut.CheckMiniKey(wrongMiniKeyLong);
        var result4 = sut.CheckMiniKey(wrongMiniKeyShort);
#pragma warning restore CS0612 // Type or member is obsolete

        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeFalse();
        result4.Should().BeFalse();
    }

    [Fact]
    public void Test_CheckMiniKeyAny()
    {
        var correctMiniKeyLong = "S6c56bnXQiBjk9mqSYE7ykVQ7NzrRy";
        var wrongMiniKeyLong = "S6c56bnXQiBjk9mqSYE7ykVQ7NzrRa"; // last character was changed
        var correctMiniKeyShort = "SG64GZqySYwBm9KxE3wJ29";
        var wrongMiniKeyShort = "SMgKPPQ6n64wSgV2dUwYte"; // random key

        var correctMiniKeyLongHashBytes = Encoding.UTF8.GetBytes(correctMiniKeyLong);
        var correctMiniKeyShortHashBytes = Encoding.UTF8.GetBytes(correctMiniKeyShort);
        var wrongMiniKeyLongHashBytes = Encoding.UTF8.GetBytes(wrongMiniKeyLong);
        var wrongMiniKeyShortHashBytes = Encoding.UTF8.GetBytes(wrongMiniKeyShort);

        var result1 = sut.CheckMiniKeyAny(correctMiniKeyLongHashBytes);
        var result2 = sut.CheckMiniKeyAny(correctMiniKeyShortHashBytes);
        var result3 = sut.CheckMiniKeyAny(wrongMiniKeyLongHashBytes);
        var result4 = sut.CheckMiniKeyAny(wrongMiniKeyShortHashBytes);

        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeFalse();
        result4.Should().BeFalse();
    }

    [Fact]
    public void Test_CalculatePrivateKeyWifFromMiniKey()
    {
        var correctMiniKeyLong = "S6c56bnXQiBjk9mqSYE7ykVQ7NzrRy";
        var correctMiniKeyShort = "SG64GZqySYwBm9KxE3wJ29";

        var result1 = sut.GetWifPrivateKey(correctMiniKeyLong);
        var result2 = sut.GetWifPrivateKey(correctMiniKeyShort);

        result1.Should().Be("5JPy8Zg7z4P7RSLsiqcqyeAF1935zjNUdMxcDeVrtU1oarrgnB7");
        result2.Should().Be("5Hv4GsRSzCNQPC1wrkGi2ZbsNtCFc79w2UDMakY9d3YacxiqLLt");
    }

    [Fact]
    public void Test_MiniKeyToAddress()
    {
        var correctMiniKeyLong = "S6c56bnXQiBjk9mqSYE7ykVQ7NzrRy";
        var correctMiniKeyShort = "SG64GZqySYwBm9KxE3wJ29";

        var correctMiniKeyLongAddress = sut.GetAddressFromMiniKey(correctMiniKeyLong);
        var correctMiniKeyShortAddress = sut.GetAddressFromMiniKey(correctMiniKeyShort);

        correctMiniKeyLongAddress.Should().Be("1CciesT23BNionJeXrbxmjc7ywfiyM4oLW");
        correctMiniKeyShortAddress.Should().Be("15azScMmHvFPAQfQafrKr48E9MqRRXSnVv");
    }
}
