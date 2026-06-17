namespace Services
{
    public partial class RefusedReturnLine
    {
        public string DisplayAccount
        {
            get
            {
                if (string.IsNullOrWhiteSpace(AccountCode))
                    return AccountName ?? string.Empty;

                if (string.IsNullOrWhiteSpace(AccountName))
                    return AccountCode;

                return $"{AccountCode} - {AccountName}";
            }
        }

        public string DisplayQuantity => Quantity.ToString("0.####");

        public bool HasReason => !string.IsNullOrWhiteSpace(Reason);

        public bool HasLongReason
        {
            get
            {
                var text = Reason?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(text))
                    return false;

                return text.Length > 120 || text.Contains('\n') || text.Contains('\r');
            }
        }

        public bool HasPicturePath => !string.IsNullOrWhiteSpace(PicturePath);

        public bool HasReasonOrPicture => HasReason || HasPicturePath;

        public bool CanUploadPictureForCurrentWarehouse
        {
            get
            {
                if (!AGRA_EASY_MOBILE.Services.EasySession.IsAdministrator)
                    return false;

                var lineWarehouse = (Warehouse ?? string.Empty).Trim();
                var currentWarehouse = (AGRA_EASY_MOBILE.Services.EasySession.CurrentAccount?.Warehouse ?? string.Empty).Trim();

                return !string.IsNullOrWhiteSpace(lineWarehouse)
                    && string.Equals(lineWarehouse, currentWarehouse, StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool HasLongReturnClientCode => (ReturnClientCode?.Trim().Length ?? 0) > 22;
    }
}
