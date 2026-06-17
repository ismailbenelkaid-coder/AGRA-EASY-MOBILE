using System;

namespace AGRA_EASY_MOBILE.Models
{
    public class ExpeditionFilter
    {
        public DateTime? FirstDate { get; set; }
        public DateTime? LastDate { get; set; }
        public string ProductCode { get; set; }
        public string ClientSorderCode { get; set; }
        public string SorderType { get; set; }
        public string DeliveryType { get; set; }
        public string ContainerNo { get; set; }
        public string DeliveryNumber { get; set; }
        public string AccountCode { get; set; } // Déjà présent 
        public bool ShowDetails { get; set; } // Pour basculer entre vue détaillée et entête uniquement



        public bool IsDefined => FirstDate.HasValue
            || LastDate.HasValue
            || !string.IsNullOrWhiteSpace(ProductCode)
            || !string.IsNullOrWhiteSpace(ClientSorderCode)
            || !string.IsNullOrWhiteSpace(SorderType)
            || !string.IsNullOrWhiteSpace(DeliveryType)
            || !string.IsNullOrWhiteSpace(ContainerNo)
            || !string.IsNullOrWhiteSpace(DeliveryNumber)
            || !string.IsNullOrWhiteSpace(AccountCode);
    }
}