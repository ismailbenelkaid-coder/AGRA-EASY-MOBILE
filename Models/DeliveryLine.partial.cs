namespace Services
{
    public partial class DeliveryLine
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

        public string DisplayTypeCompact => string.IsNullOrWhiteSpace(Type) ? "Type ?" : Type;

        public string DisplayWarehouseCompact => string.IsNullOrWhiteSpace(Warehouse) ? "Entrepôt ?" : Warehouse;

        public string DisplayCreationDateTime => CreationDate == default
            ? string.Empty
            : $"{CreationDate:dd/MM/yyyy HH:mm}";
    }
}
