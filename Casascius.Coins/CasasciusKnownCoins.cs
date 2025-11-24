using System.Collections.Generic;

namespace Casascius.Coins;

//
// WARNING: This file contains sample addresses and private keys. Do not send bitcoins to or import any sample keys - you will lose your money.
//

public static class CasasciusKnownCoins
{
    public static readonly IReadOnlyDictionary<string, CasasciusKnownCoin> Collection = new Dictionary<string, CasasciusKnownCoin> // The dictionary Key is a Wallet Address
    {
        // Wiki
        { "1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj", new CasasciusKnownCoin { MiniKey = "SzavMBLoXU6kDrqtUVmffv", PrivateKey = "5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF", Address = "1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj", Comment = "https://en.bitcoin.it/wiki/Private_key" } },
        { "1CciesT23BNionJeXrbxmjc7ywfiyM4oLW", new CasasciusKnownCoin { MiniKey = "S6c56bnXQiBjk9mqSYE7ykVQ7NzrRy", PrivateKey = "5JPy8Zg7z4P7RSLsiqcqyeAF1935zjNUdMxcDeVrtU1oarrgnB7", Address = "1CciesT23BNionJeXrbxmjc7ywfiyM4oLW", Comment = "https://en.bitcoin.it/wiki/Mini_private_key_format" } },
        { "1148CztCNPQJdfXt5PGPxffYWrA1CjSoiw", new CasasciusKnownCoin { MiniKey = "SMgKFRQ6n64w8gV2dUwYte", PrivateKey = "5Jk1ByG5Gjy3fcyhTa4Dap9NK4eCJUcApuYeSyYYPB7N3b1ms71", Address = "1148CztCNPQJdfXt5PGPxffYWrA1CjSoiw", Comment = "https://en.bitcoin.it/wiki/Casascius_physical_bitcoins" } },

        // YouTube
        { "15azScMmHvFPAQfQafrKr48E9MqRRXSnVv", new CasasciusKnownCoin { MiniKey = "SG64GZqySYwBm9KxE3wJ29", PrivateKey = "5Hv4GsRSzCNQPC1wrkGi2ZbsNtCFc79w2UDMakY9d3YacxiqLLt", Address = "15azScMmHvFPAQfQafrKr48E9MqRRXSnVv", Comment = "https://www.youtube.com/watch?v=bA4eetGvQXE&ab_channel=15azScMm" } },
        { "1CCJ6B7XZRsT2VqKeWGsstZSgcYxg6PQJB", new CasasciusKnownCoin { MiniKey = "SECUN34uVQKk3UkMBAiTLSoLTUWfwS", PrivateKey = "5JLDLEBE1oNfyNqyBsqknFK1zwjwnqHEUStXNhocW3tJho9g8fr", Address = "1CCJ6B7XZRsT2VqKeWGsstZSgcYxg6PQJB", Comment = "https://www.youtube.com/watch?v=y1yydfYJFwA&ab_channel=LinusDunkers" } },
        { "125fEtzGewLHGNgM8kpH8gFfh14MmfLhSR", new CasasciusKnownCoin { MiniKey = "SvX26tQus497UABggmwN3HXmJa4jTt", PrivateKey = "5KMAuR9V1DH5ay7oW4pksZLVPkj5qvDdCjoup26dK9ykCiFhBjt", Address = "125fEtzGewLHGNgM8kpH8gFfh14MmfLhSR", Comment = "https://www.youtube.com/watch?v=y1yydfYJFwA&ab_channel=LinusDunkers" } },
    };
}

public class CasasciusKnownCoin
{
    public string? MiniKey { get; set; }

    public string? PrivateKey { get; set; }

    public string? Address { get; set; }

    public string? Comment { get; set; }

    // Note: This override is needed for xUnit to better name InlineData-generated tests
    public override string? ToString() => Address ?? base.ToString();
}
