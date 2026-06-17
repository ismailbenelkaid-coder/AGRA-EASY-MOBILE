using System;

namespace AGRA_EASY_MOBILE.Models
{
    public class ShippingCostFilter
    {
        public DateTime? FirstDate { get; set; }
        public DateTime? LastDate { get; set; }
        public string Warehouse { get; set; }
        public string AccountCode { get; set; }
        public string ShippingCostDeliveryNumber { get; set; }
        public string TurnoverDeliveryNumber { get; set; }
        public bool ShowDetails { get; set; }
        public bool WithBilledLines { get; set; }
        public bool WithExemptLines { get; set; }
        public bool WithWaitingLines { get; set; } = true;
        public bool HasRequiredSearchCriterion => !string.IsNullOrWhiteSpace(AccountCode) || !string.IsNullOrWhiteSpace(ShippingCostDeliveryNumber) || !string.IsNullOrWhiteSpace(TurnoverDeliveryNumber);
    }
}
