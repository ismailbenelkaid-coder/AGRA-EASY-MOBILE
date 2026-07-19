namespace Services
{
    public partial class SupplierRefundLine
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

        public bool HasSupplierComment => !string.IsNullOrWhiteSpace(SupplierComment);

        public bool HasFilePath => !string.IsNullOrWhiteSpace(FilePath);

        public bool HasSupplierCommentOrFile => HasSupplierComment || HasFilePath;

        public bool HasLongReturnClientCode => (ReturnClientCode?.Trim().Length ?? 0) > 22;

        public bool HasSupplierResponse => !string.IsNullOrWhiteSpace(SupplierResponse);

        public bool IsSupplierRefundDocumentUnavailable
        {
            get
            {
                var status = SupplierStatusCode?.Trim();
                return string.IsNullOrWhiteSpace(status) || string.Equals(status, "ING", StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool CanOpenSupplierRefundDocument => HasSupplierResponse && !IsSupplierRefundDocumentUnavailable;

        public bool HasSupplierResponseButNoDocument => HasSupplierResponse && IsSupplierRefundDocumentUnavailable;

        public bool CanUploadDocumentForCurrentWarehouse
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

        public bool CanUploadDocumentForTreatedCurrentWarehouse => CanOpenSupplierRefundDocument && CanUploadDocumentForCurrentWarehouse;
    }
}
