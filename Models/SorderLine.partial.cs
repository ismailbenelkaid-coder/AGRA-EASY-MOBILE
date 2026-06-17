namespace Services
{
    public partial class SorderLine
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
    }
}
