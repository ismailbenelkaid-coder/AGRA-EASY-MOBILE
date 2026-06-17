using System;

namespace AGRA_EASY_MOBILE.Models
{
    public class ReturnFilter
    {
        public DateTime? FirstDate { get; set; }
        public DateTime? LastDate { get; set; }
        public string ProductCode { get; set; }
        public string AccountCode { get; set; }
        public string ReturnStatus { get; set; }
        public string ProductStatus { get; set; }
        public string ReturnCode { get; set; }
        public string ReturnClientCode { get; set; }
        public bool ShowDetails { get; set; }
        public bool WithTreated { get; set; }

        public bool IsDefined => FirstDate.HasValue
            || LastDate.HasValue
            || !string.IsNullOrWhiteSpace(ProductCode)
            || !string.IsNullOrWhiteSpace(AccountCode)
            || !string.IsNullOrWhiteSpace(ReturnStatus)
            || !string.IsNullOrWhiteSpace(ProductStatus)
            || !string.IsNullOrWhiteSpace(ReturnCode)
            || !string.IsNullOrWhiteSpace(ReturnClientCode);
    }
}
