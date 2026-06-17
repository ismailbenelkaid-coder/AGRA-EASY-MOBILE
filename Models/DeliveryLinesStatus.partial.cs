namespace Services
{
    public partial class DeliveryLinesStatus
    {
        public string DisplayAbattement
        {
            get
            {
                var text = (Abattement ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(text))
                    return string.Empty;

                if (string.Equals(text, "REC", System.StringComparison.OrdinalIgnoreCase))
                    return "sans abattement";

                return text;
            }
        }

        public bool HasDisplayAbattement => !string.IsNullOrWhiteSpace(DisplayAbattement);
    }
}
