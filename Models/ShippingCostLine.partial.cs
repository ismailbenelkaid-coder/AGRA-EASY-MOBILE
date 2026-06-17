using System.Globalization;

namespace Services
{
    public partial class ShippingCostLine
    {
        private static readonly CultureInfo FrenchCulture = CultureInfo.GetCultureInfo("fr-FR");
        public string DisplayShippingCostCode => string.IsNullOrWhiteSpace(ShippingCostCode) ? string.Empty : $"N° {ShippingCostCode}";
        public string DisplayHeaderDate => ShippingDate == default ? string.Empty : ShippingDate.ToString("dd/MM/yyyy", FrenchCulture);
        public string DisplayWarehouseShippingCostDeliveryNumber { get { var warehouse = Warehouse?.Trim() ?? string.Empty; var delivery = ShippingCostDeliveryNumber?.Trim() ?? string.Empty; if (string.IsNullOrWhiteSpace(warehouse)) return delivery; if (string.IsNullOrWhiteSpace(delivery)) return warehouse; return $"{warehouse} / {delivery}"; } }
        public string DisplayCost => Cost.ToString("0.00", FrenchCulture) + " €";
        public string DisplayTotalWeightAndContainers { get { var parts = new List<string>(); if (TotalWeight.HasValue) parts.Add(TotalWeight.Value.ToString("0.##", FrenchCulture) + " KG"); if (DeliveredContainerCount != 0) parts.Add(DeliveredContainerCount.ToString("0.####", FrenchCulture) + " colis"); return string.Join(" / ", parts); } }
        public string DisplayAddress { get { var parts = new[] { AddressLine1, AddressLine2 }.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()); return string.Join(" ", parts); } }
        public bool HasAddress => !string.IsNullOrWhiteSpace(DisplayAddress);
        public bool HasLongAddress => DisplayAddress.Length > 42;
        public string DisplayCityZip { get { var city = City?.Trim() ?? string.Empty; var zip = Zip?.Trim() ?? string.Empty; if (string.IsNullOrWhiteSpace(city)) return zip; if (string.IsNullOrWhiteSpace(zip)) return city; return $"{city} {zip}"; } }
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
        public bool HasLongDescription => (Description?.Trim().Length ?? 0) > 80;
        public string DisplayAccountCode => string.IsNullOrWhiteSpace(AccountCode) ? string.Empty : $"Client : {AccountCode}";
        public string DisplayContainerNo => string.IsNullOrWhiteSpace(ContainerNo) ? string.Empty : $"Colis : {ContainerNo.Trim()}";
        public bool ShouldShowCustomerBlock => global::AGRA_EASY_MOBILE.Services.EasySession.IsAdministrator;
        public string DisplayWeight => Weight.HasValue ? Weight.Value.ToString("0.##", FrenchCulture) + " KG" : string.Empty;
    }
}
