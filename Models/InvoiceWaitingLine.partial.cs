using System.Globalization;

namespace Services
{
    public partial class InvoiceWaitingLine
    {
        private static readonly CultureInfo FrenchCulture = CultureInfo.GetCultureInfo("fr-FR");

        public string DisplayInvoiceWaitingCode => string.IsNullOrWhiteSpace(InvoiceWaitingCode) ? string.Empty : $"N° {InvoiceWaitingCode}";
        public string DisplayHeaderDate { get { var date = InvoiceDate ?? CreationDate; return date == default ? string.Empty : date.ToString("dd/MM/yyyy", FrenchCulture); } }
        public string DisplayCreationDate => CreationDate == default ? string.Empty : CreationDate.ToString("dd/MM/yyyy", FrenchCulture);
        public DateTime DeliveryDateForSort => InvoiceDate ?? CreationDate;
        public string DisplayWarehouseDeliveryType { get { var warehouse = Warehouse?.Trim() ?? string.Empty; var deliveryType = DeliveryType?.Trim() ?? string.Empty; if (string.IsNullOrWhiteSpace(warehouse)) return deliveryType; if (string.IsNullOrWhiteSpace(deliveryType)) return warehouse; return $"{warehouse} / {deliveryType}"; } }
        public string DisplayPrice => Price.ToString("0.00", FrenchCulture);
        public bool HasDiscount => Discount != 0;
        public string DisplayDiscountPercent => Discount.ToString("0.##", FrenchCulture) + " %";
        public decimal ConsigneEcotaxeValue => (ConsigneValue ?? 0) + (EcotaxeValue ?? 0);
        public bool HasConsigneEcotaxe => ConsigneEcotaxeValue != 0;
        public string DisplayConsigneEcotaxe => ConsigneEcotaxeValue.ToString("0.00", FrenchCulture);
        public string DisplayTTC => TTC.ToString("0.00", FrenchCulture);
        public string DisplayQuantity => Quantity.ToString("0.####", FrenchCulture);
        public string DisplayTVA => TVA.ToString("0.##", FrenchCulture) + " %";
        public string DisplayLineTotal => LineTotal.ToString("0.00", FrenchCulture);
        public bool ShouldShowCustomerBlock => global::AGRA_EASY_MOBILE.Services.EasySession.IsAdministrator;
    }
}
