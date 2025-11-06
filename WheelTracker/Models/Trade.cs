using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WheelTracker.Models
{
    public enum ActionType { STO, BTC, BuyShares, SellShares }
    public enum OptionStrategy { CSP, CC, Put, Call }

    public enum CloseType { BTC, STC, EXP, ASS, ROLL }

    [Table("Trades")]
    public class Trade
    {
        [Key] public int Id { get; set; }

        public string Ticker { get; set; } = string.Empty;
        public DateTime OpenDate { get; set; } = DateTime.Today;
        public ActionType Action { get; set; }
        public OptionStrategy? Strategy { get; set; }
        public string CreditDebit { get; set; } = "Credit";
        public double? PriceAtOpen { get; set; }
        public DateTime? Expiration { get; set; }
        public double? Strike { get; set; }
        public int Qty { get; set; } = 1;
        public double Premium { get; set; }
        public double Fees { get; set; }

        // These are CALCULATED at runtime → NOT in DB
        [NotMapped] public int InitialDTE { get; set; }
        [NotMapped] public double? Total { get; set; }
        [NotMapped] public double CalculatedReturn { get; set; }
        [NotMapped] public double AnnualReturn { get; set; }

        // These ARE in DB
        public int? DTE { get; set; }
        public double? Moneyness { get; set; }
        public string Status { get; set; } = "Open";
        public DateTime? CloseDate { get; set; }
        public CloseType? CloseTransType { get; set; }
        public int? CloseQty { get; set; }
        public double? ClosePrice { get; set; }
        public double? CloseFee { get; set; }
        public int? DaysHeld { get; set; }
        public double? RealizedGainLoss { get; set; }
        public double? UnrealizedGainLoss { get; set; }
        public double? CurrentSharePrice { get; set; }
    }
}