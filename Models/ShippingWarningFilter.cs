namespace AGRA_EASY_MOBILE.Models
{
    public class ShippingWarningFilter
    {
        public DateTime? FirstDate { get; set; }
        public DateTime? LastDate { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string ContainerNo { get; set; } = string.Empty;
        public string Warehouse { get; set; } = string.Empty;
    }
}
