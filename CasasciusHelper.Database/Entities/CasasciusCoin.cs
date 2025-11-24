using Casascius.Coins;

namespace CasasciusHelper.Database.Entities;

public class CasasciusCoin
{
    public int Id { get; set; }

    public string Address { get; set; } = "";

    public int? Series { get; set; }

    //public CasasciusCoinSeries? SeriesValue { get; set; }

    public string? Type { get; set; }

    //public CasasciusCoinType? TypeValue { get; set; }

    public string Status { get; set; } = "";

    public CasasciusStatuses StatusValue { get; set; }

    public decimal? Value { get; set; }

    public decimal Balance { get; set; }

    public int? CreateBlock { get; set; }

    public int? RedeemBlock { get; set; }

    public DateTime? CreateTime { get; set; }

    public DateTime? RedeemTime { get; set; }

    public DateTime UpdateTime { get; set; }
}
