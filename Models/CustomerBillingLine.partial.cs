using System.Globalization;

namespace Services
{
    public partial class CustomerBillingLine
    {
        public string DisplayCustomerBillingId => CustomerBillingId ?? string.Empty;

        public string DisplayCreationDate => CreationDate == default
            ? string.Empty
            : CreationDate.ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("fr-FR"));

        public string DisplayInvoiceDate => InvoiceDate.HasValue
            ? InvoiceDate.Value.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("fr-FR"))
            : string.Empty;

        public string DisplayQuantity => Quantity.HasValue
            ? Quantity.Value.ToString("0.####", CultureInfo.GetCultureInfo("fr-FR"))
            : string.Empty;

        public string DisplayTotalHTBase => TotalHTBaseLine.ToString("0.00", CultureInfo.GetCultureInfo("fr-FR"));

        public string DisplayTotalTVA => TotalTVALine.ToString("0.00", CultureInfo.GetCultureInfo("fr-FR"));

        public string DisplayTotalTTC => TotalTTCLine.ToString("0.00", CultureInfo.GetCultureInfo("fr-FR"));

        public string DisplayDeliveredAccount
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

        public string DisplayInvoicedAccount
        {
            get
            {
                if (string.IsNullOrWhiteSpace(InvoicedAccountCode))
                    return InvoicedAccountName ?? string.Empty;

                if (string.IsNullOrWhiteSpace(InvoicedAccountName))
                    return InvoicedAccountCode;

                return $"{InvoicedAccountCode} - {InvoicedAccountName}";
            }
        }

        public bool HasDifferentInvoicedAccount
        {
            get
            {
                var delivered = (AccountCode ?? string.Empty).Trim();
                var invoiced = (InvoicedAccountCode ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(invoiced))
                    return false;

                if (string.IsNullOrWhiteSpace(delivered))
                    return true;

                return !string.Equals(delivered, invoiced, StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool ShouldShowCustomerBlock => global::AGRA_EASY_MOBILE.Services.EasySession.IsAdministrator || HasDifferentInvoicedAccount;

        public bool HasClientNetworkCode => !string.IsNullOrWhiteSpace(ClientNetworkCode);

        public bool HasSorderClientCode => !string.IsNullOrWhiteSpace(SorderClientCode);
    }
}
