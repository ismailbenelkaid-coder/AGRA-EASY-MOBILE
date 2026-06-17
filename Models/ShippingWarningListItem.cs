using Services;

namespace AGRA_EASY_MOBILE.Models
{
    public class ShippingWarningListItem
    {
        public ShippingWarning Warning { get; set; }
        public bool IsNew { get; set; }

        public ShippingWarningListItem(ShippingWarning warning, bool isNew)
        {
            Warning = warning;
            IsNew = isNew;
        }

        public string DateText => Warning.CreationDate == default ? string.Empty : Warning.CreationDate.ToString("dd/MM/yyyy HH:mm");
        public string ShortDescription => string.IsNullOrWhiteSpace(Warning.ShortDescription) ? "Alerte" : Warning.ShortDescription;
        public string ClientText
        {
            get
            {
                var code = Warning.AccountCode?.Trim() ?? string.Empty;
                var name = Warning.AccountName?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(name)) return $"{code} - {name}";
                return code + name;
            }
        }
        public string WarehouseText => Warning.Warehouse?.Trim() ?? string.Empty;
        public string ContainerText => string.IsNullOrWhiteSpace(Warning.ContainerNo) ? string.Empty : $"Conteneur : {Warning.ContainerNo.Trim()}";
    }
}
