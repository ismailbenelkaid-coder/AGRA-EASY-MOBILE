using System.Globalization;

namespace Services
{
    public partial class RefundWatingLine
    {
        private static readonly CultureInfo FrenchCulture = CultureInfo.GetCultureInfo("fr-FR");

        public string DisplayRefundWaitingCode => string.IsNullOrWhiteSpace(RefundWaitingCode) ? string.Empty : $"N° {RefundWaitingCode}";
        public string DisplayHeaderDate { get { var date = InvoiceDate ?? CreationDate; return date == default ? string.Empty : date.ToString("dd/MM/yyyy", FrenchCulture); } }
        public string DisplayCreationDate => CreationDate == default ? string.Empty : CreationDate.ToString("dd/MM/yyyy", FrenchCulture);
        public DateTime DeliveryDateForSort => InvoiceDate ?? CreationDate;
        public string DisplayWarehouseReturnType { get { var warehouse = Warehouse?.Trim() ?? string.Empty; var returnType = ReturnType?.Trim() ?? string.Empty; if (string.IsNullOrWhiteSpace(warehouse)) return returnType; if (string.IsNullOrWhiteSpace(returnType)) return warehouse; return $"{warehouse} / {returnType}"; } }
        public string DisplayDeliveryNetPrice => DeliveryNetPrice.ToString("0.00", FrenchCulture);
        public bool HasAdditionalDepression => AdditionalDepression.HasValue && AdditionalDepression.Value != 0;
        public bool HasDepression => Depression != 0 || HasAdditionalDepression;
        public string DisplayDepression => Depression.ToString("0", FrenchCulture) + " %";
        public string DisplayAdditionalDepression => HasAdditionalDepression ? AdditionalDepression.Value.ToString("0", FrenchCulture) + " %" : string.Empty;
        public string DisplayDepressions
        {
            get
            {
                var parts = new List<string>();
                if (Depression != 0) parts.Add("Dépr. : " + DisplayDepression);
                if (HasAdditionalDepression) parts.Add("Dépr.+ : " + DisplayAdditionalDepression);
                return string.Join(" / ", parts);
            }
        }
        public string DisplayTTC => TTC.ToString("0.00", FrenchCulture);
        public string DisplayQuantity => Quantity.ToString("0.####", FrenchCulture);
        public string DisplayTVA => TVA.ToString("0.##", FrenchCulture) + " %";
        public string DisplayLineTotal => LineTotal.ToString("0.00", FrenchCulture);
        public bool ShouldShowCustomerBlock => global::AGRA_EASY_MOBILE.Services.EasySession.IsAdministrator;
    }
}
