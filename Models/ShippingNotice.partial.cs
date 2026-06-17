namespace Services
{
    public partial class ShippingNotice
    {
        public string DisplayAccount
        {
            get
            {
                if (string.IsNullOrWhiteSpace(AccountCode))
                    return Account ?? string.Empty;

                if (string.IsNullOrWhiteSpace(Account))
                    return AccountCode;

                return $"{AccountCode} - {Account}";
            }
        }

        public string DisplayContainerNo => TrimLeadingZeros(ContainerNo);

        public string DisplayMasterContainerNo => TrimLeadingZeros(MasterContainerNo);

        public bool HasMasterContainerNo => !string.IsNullOrWhiteSpace(MasterContainerNo);

        public bool IsSaturdayDelivery => string.Equals(WeekendDelivery?.Trim(), "Samedi", StringComparison.OrdinalIgnoreCase);

        public bool HasCarrierLabel => !string.IsNullOrWhiteSpace(CarrierLabel);

        public bool HasTrackingLink => !string.IsNullOrWhiteSpace(TrackingLink);

        public bool HasCarrierLabelWithoutLink => HasCarrierLabel && !HasTrackingLink;

        public bool HasThirdDetailLine => HasMasterContainerNo || IsSaturdayDelivery || HasCarrierLabel;

        private static string TrimLeadingZeros(string? value)
        {
            var text = value?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var trimmed = text.TrimStart('0');
            return string.IsNullOrEmpty(trimmed) ? "0" : trimmed;
        }
    }
}
