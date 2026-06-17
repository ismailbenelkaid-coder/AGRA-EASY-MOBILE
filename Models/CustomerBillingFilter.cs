using System;

namespace AGRA_EASY_MOBILE.Models
{
    public class CustomerBillingFilter
    {
        public DateTime? FirstDate { get; set; }
        public DateTime? LastDate { get; set; }
        public string ProductCode { get; set; }
        public string AccountCode { get; set; }
        public string InvoicedAccountCode { get; set; }
        public string ClientSorderCode { get; set; }
        public string DeliveryNumber { get; set; }
        public bool ShowDetails { get; set; }

        public bool IsDefined => FirstDate.HasValue
            || LastDate.HasValue
            || !string.IsNullOrWhiteSpace(ProductCode)
            || !string.IsNullOrWhiteSpace(AccountCode)
            || !string.IsNullOrWhiteSpace(InvoicedAccountCode)
            || !string.IsNullOrWhiteSpace(ClientSorderCode)
            || !string.IsNullOrWhiteSpace(DeliveryNumber);
    }
}
