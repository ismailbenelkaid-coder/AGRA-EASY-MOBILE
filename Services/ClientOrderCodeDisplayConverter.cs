using System.Globalization;

namespace AGRA_EASY_MOBILE
{
    public class ClientOrderCodeDisplayConverter : IValueConverter
    {
        private const int DefaultMaxDisplayLength = 10;
        private const string Ellipsis = "...";

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var code = System.Convert.ToString(value, culture)?.Trim() ?? string.Empty;
            int maxDisplayLength = GetMaxDisplayLength(parameter);

            if (code.Length <= maxDisplayLength)
                return code;

            return code.Substring(0, maxDisplayLength - Ellipsis.Length) + Ellipsis;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static int GetMaxDisplayLength(object? parameter)
        {
            if (parameter != null && int.TryParse(parameter.ToString(), out int maxDisplayLength) && maxDisplayLength > Ellipsis.Length)
                return maxDisplayLength;

            return DefaultMaxDisplayLength;
        }
    }

    public class ClientOrderCodeInputTransparentConverter : IValueConverter
    {
        private const int DefaultMaxDisplayLength = 10;
        private const string Ellipsis = "...";

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var code = System.Convert.ToString(value, culture)?.Trim() ?? string.Empty;
            int maxDisplayLength = GetMaxDisplayLength(parameter);

            return code.Length <= maxDisplayLength;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static int GetMaxDisplayLength(object? parameter)
        {
            if (parameter != null && int.TryParse(parameter.ToString(), out int maxDisplayLength) && maxDisplayLength > Ellipsis.Length)
                return maxDisplayLength;

            return DefaultMaxDisplayLength;
        }
    }
}
