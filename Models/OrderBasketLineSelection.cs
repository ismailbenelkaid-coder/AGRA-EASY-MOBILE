using Services;
using System.ComponentModel;

namespace AGRA_EASY_MOBILE.Models
{
    public class OrderBasketLineSelection : INotifyPropertyChanged
    {
        private string _quantityText;

        public OrderBasketLineSelection(SorderBasketLine line)
        {
            Line = line;
            _quantityText = line.Quantity ?? string.Empty;
        }

        public SorderBasketLine Line { get; }

        public string QuantityText
        {
            get => _quantityText;
            set
            {
                if (_quantityText == value)
                    return;

                _quantityText = value ?? string.Empty;
                OnPropertyChanged(nameof(QuantityText));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
