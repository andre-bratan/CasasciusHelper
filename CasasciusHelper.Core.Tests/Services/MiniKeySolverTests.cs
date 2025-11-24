using CasasciusHelper.Core.Services;
using CasasciusHelper.Core.Tests.Data;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace CasasciusHelper.Core.Tests.Services;

// Note: These tests are intentionally made integration-like

[TestSubject(typeof(MiniKeySolver))]
public class MiniKeySolverTests
{
    private readonly IMiniKeySolver sut = new MiniKeySolver(new CasasciusDataCacheForTests(), new MiniKeyService(new WifPrivateKeyService()));

    [Fact]
    public void Test_GetKeyMask()
    {
        var miniKey1 = "SG64GZqySYwBm9KxE3wJ29";
        var miniKey2 = "SG64GZqySYwBm9KxE3wJ20"; // last character was changed
        var miniKey3 = "SG64GZqySYwBm9Kx?3wJ2?"; // some characters were changed

        var result1 = sut.GetKeyMask(miniKey1);
        var result2 = sut.GetKeyMask(miniKey2);
        var reuslt3 = sut.GetKeyMask(miniKey3);

        result1.Should().Be("                      ");
        result2.Should().Be("                     ?");
        reuslt3.Should().Be("                ?    ?");
    }

    [Fact]
    public void Test_GetUncertaintyContext()
    {
        var miniKey1 = "SG64GZqySYwBm9KxE3wJ29";
        var miniKey2 = "SG64GZqySYwBm9KxE3wJ20"; // last character was changed
        var miniKey3 = "SG64GZqySYwBm9Kx?3wJ2?"; // some characters were changed

        var result1 = sut.GetUncertaintyContext(miniKey1);
        var result2 = sut.GetUncertaintyContext(miniKey2);
        var result3 = sut.GetUncertaintyContext(miniKey3);

        result1.Should().BeEmpty();

        result2.Should().HaveCount(1);
        result2.Should().BeEquivalentTo(new int[] { 21 });

        result3.Should().HaveCount(2);
        result3.Should().BeEquivalentTo(new int[] { 16, 21 });
    }

    [Fact]
    public void Test_SearchKey_ValidKey()
    {
        var miniKey = "SG64GZqySYwBm9KxE3wJ29";

        var result = sut.SearchKey(miniKey);

        result.Should().Be("SG64GZqySYwBm9KxE3wJ29");
    }

    [Fact]
    public void Test_SearchKey_ValidUnknownKey()
    {
        var miniKey = "SMgKFRQ6n64w8gV2dUwYuT";

        var result = sut.SearchKey(miniKey);

        result.Should().BeNull();
    }

    [Fact]
    public void Test_SearchKey_InvalidSymbol()
    {
        var miniKey = "SG64GZqySYwBm9KxE3wJ2?"; // last character was changed

        var result = sut.SearchKey(miniKey);

        result.Should().NotBeNull();
        result.Should().Be("SG64GZqySYwBm9KxE3wJ29");
    }

    [Fact]
    public void Test_SearchKey_UncertainSymbols()
    {
        var miniKey = "SMgKF?Q6n64w?gV2dUwYte"; // some characters were changed (the more the longer it will take to find the solution)

        var result = sut.SearchKey(miniKey);

        result.Should().NotBeNull();
        result.Should().Be("SMgKFRQ6n64w8gV2dUwYte");
    }

    [Fact]
    public void Test_SearchKeys_ValidKey()
    {
        var miniKey = "SG64GZqySYwBm9KxE3wJ29";

        var result = sut.SearchKeys(miniKey);

        result.Should().BeEquivalentTo("SG64GZqySYwBm9KxE3wJ29");
    }

    [Fact]
    public void Test_SearchKeys_InvalidSymbol()
    {
        var miniKey = "SG64GZqySYwBm9KxE3wJ2?"; // last character was changed

        var result = sut.SearchKeys(miniKey);

        result.Should().BeEquivalentTo("SG64GZqySYwBm9KxE3wJ29");
    }

    [Fact]
    public void Test_SearchKeys_UncertainSymbols()
    {
        var miniKey = "SMgKFRQ6n64w8gV2dUwY??"; // some characters were changed (the more the longer it will take to find the solution)

        var result = sut.SearchKeys(miniKey);

        result.Should().HaveCountGreaterThan(1);
        result.Should().HaveCount(14);
        result.Should()
              .BeEquivalentTo(
                   "SMgKFRQ6n64w8gV2dUwY5A",
                   "SMgKFRQ6n64w8gV2dUwY5o",
                   "SMgKFRQ6n64w8gV2dUwYAQ",
                   "SMgKFRQ6n64w8gV2dUwYAz",
                   "SMgKFRQ6n64w8gV2dUwYBi",
                   "SMgKFRQ6n64w8gV2dUwYPv",
                   "SMgKFRQ6n64w8gV2dUwYWa",
                   "SMgKFRQ6n64w8gV2dUwYay",
                   "SMgKFRQ6n64w8gV2dUwYte",
                   "SMgKFRQ6n64w8gV2dUwYuT",
                   "SMgKFRQ6n64w8gV2dUwYwX",
                   "SMgKFRQ6n64w8gV2dUwYwb",
                   "SMgKFRQ6n64w8gV2dUwYwt",
                   "SMgKFRQ6n64w8gV2dUwYxV"
               );
    }
}
