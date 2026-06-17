using System.Globalization;

namespace AGRA_EASY_MOBILE
{
    public class StringNotEmptyConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => !string.IsNullOrWhiteSpace(System.Convert.ToString(value, culture));

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class StringEmptyConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => string.IsNullOrWhiteSpace(System.Convert.ToString(value, culture));

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class SaturdayDeliveryConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => string.Equals(System.Convert.ToString(value, culture)?.Trim(), "Samedi", StringComparison.OrdinalIgnoreCase);

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class MasterContainerNoDisplayConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var text = System.Convert.ToString(value, culture)?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var trimmed = text.TrimStart('0');
            return string.IsNullOrEmpty(trimmed) ? "0" : trimmed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
