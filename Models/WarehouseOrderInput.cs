using Services;
using System.ComponentModel;
using System.Globalization;

namespace AGRA_EASY_MOBILE.Models
{
    public class WarehouseOrderInput : INotifyPropertyChanged
    {
        private string _quantityText = "0";

        public WarehouseOrderInput(WarehouseDisponibility disponibility)
        {
            Warehouse = disponibility.Warehouse ?? string.Empty;
            Disponibility = disponibility.Disponibility;
        }

        public string Warehouse { get; }
        public decimal Disponibility { get; }
        public string DisponibilityText => Disponibility.ToString("0.##", CultureInfo.CurrentCulture);

        public string QuantityText
        {
            get => _quantityText;
            set
            {
                if (_quantityText == value)
                    return;

                _quantityText = value ?? string.Empty;
                OnPropertyChanged(nameof(QuantityText));
                OnPropertyChanged(nameof(Quantity));
            }
        }

        public int Quantity
        {
            get
            {
                if (int.TryParse(_quantityText, NumberStyles.Integer, CultureInfo.CurrentCulture, out int currentValue))
                    return currentValue;

                if (int.TryParse(_quantityText, NumberStyles.Integer, CultureInfo.InvariantCulture, out currentValue))
                    return currentValue;

                return 0;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
