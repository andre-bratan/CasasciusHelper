using System.Runtime.Serialization;

namespace Casascius.Coins;

public enum CasasciusTypes
{
    [EnumMember(Value = "S1-BAR-100")]
    Series1Bar100,

    [EnumMember(Value = "S1-BAR-500")]
    Series1Bar500,

    [EnumMember(Value = "S1-BAR-1000")]
    Series1Bar1000,

    [EnumMember(Value = "S1-COIN-1")]
    Series1Coin1,

    [EnumMember(Value = "S1-COIN-5")]
    Series1Coin5,

    [EnumMember(Value = "S1-COIN-25")]
    Series1Coin25,

    [EnumMember(Value = "S1-COIN-1000")]
    Series1Coin1000,

    [EnumMember(Value = "S2-BAR-100")]
    Series2Bar100,

    [EnumMember(Value = "S2-BAR-500")]
    Series2Bar500,

    [EnumMember(Value = "S2-BAR-DIY")]
    Series2BarDiy,

    [EnumMember(Value = "S2-COIN-05")]
    Series2Coin05,

    [EnumMember(Value = "S2-COIN-1-2011")]
    Series2Coin1In2011,

    [EnumMember(Value = "S2-COIN-1-2012")]
    Series2Coin1In2012,

    [EnumMember(Value = "S2-COIN-1-2013")]
    Series2Coin1In2013,

    [EnumMember(Value = "S2-COIN-5")]
    Series2Coin5,

    [EnumMember(Value = "S2-COIN-10")]
    Series2Coin10,

    [EnumMember(Value = "S2-COIN-25")]
    Series2Coin25,

    [EnumMember(Value = "S3-COIN-0.1-AG")]
    Series3Coin01Ag,

    [EnumMember(Value = "S3-COIN-0.5-AG")]
    Series3Coin05Ag,

    [EnumMember(Value = "S3-COIN-1-AG")]
    Series3Coin1Ag
}
