namespace Services
{
    public partial class ReturnLine
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
    }
}
