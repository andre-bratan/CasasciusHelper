using CasasciusHelper.Core.Services;
using FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Xunit;

namespace CasasciusHelper.Core.Tests.Services;

// Note: These tests are intentionally made integration-like

[TestSubject(typeof(MessageSigningService))]
public class MessageSigningServiceTests
{
    private readonly IMessageSigningService sut;

    public MessageSigningServiceTests()
    {
        var wifPrivateKeyService = Substitute.For<IWifPrivateKeyService>(); // mock
        var miniKeyService = new MiniKeyService(wifPrivateKeyService); // real

        sut = new MessageSigningService(miniKeyService);
    }

    [Fact]
    public void Test_SignMessage()
    {
        var miniKey = "S6c56bnXQiBjk9mqSYE7ykVQ7NzrRy"; // corresponding Legacy address is 1CciesT23BNionJeXrbxmjc7ywfiyM4oLW
        var message = "I own a Casascius coin";

        var result = sut.SignMessage(miniKey, message);

        result.Should().Be("AAAAAAFKC6fI/JzdV4TR5FilMyLi6JycCwZJT98ktg4+CiJ7mQAAAACKRzBEAiAO3nFqeVVc0cqg4K4MxThVBTk9Ll" +
                           "WN7GRaNRlJXySCCgIgBDM/NXMkNle0QW6V+4KvcWlqeutqgWCJ1F7DVRO+NsIBQQT7T9WHL/L4pGwtSWOD/MxQPAJg" +
                           "7xJv+6xhQH9r04Tl264kK6VUxgeic7Si4LeimPslBa/6fN8AIiyrihz9frvXAAAAAAEAAAAAAAAAAAFqAAAAAA==");
    }

    [Fact]
    public void Test_VerifyMessage()
    {
        var address = "1CciesT23BNionJeXrbxmjc7ywfiyM4oLW"; // corresponding MiniKey is S6c56bnXQiBjk9mqSYE7ykVQ7NzrRy
        var message1 = "I own a Casascius coin";
        var message2 = "I don't own a Casascius coin";
        var signature = "AAAAAAFKC6fI/JzdV4TR5FilMyLi6JycCwZJT98ktg4+CiJ7mQAAAACKRzBEAiAO3nFqeVVc0cqg4K4MxThVBTk9Ll" +
                        "WN7GRaNRlJXySCCgIgBDM/NXMkNle0QW6V+4KvcWlqeutqgWCJ1F7DVRO+NsIBQQT7T9WHL/L4pGwtSWOD/MxQPAJg" +
                        "7xJv+6xhQH9r04Tl264kK6VUxgeic7Si4LeimPslBa/6fN8AIiyrihz9frvXAAAAAAEAAAAAAAAAAAFqAAAAAA==";

        var result1 = sut.VerifyMessage(address, message1, signature);
        var result2 = sut.VerifyMessage(address, message2, signature);

        result1.Should().BeTrue();
        result2.Should().BeFalse();
    }
}
