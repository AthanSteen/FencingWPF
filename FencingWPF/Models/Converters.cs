using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FencingWPF.Models
{
    public class RankToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int rank)
            {
                return rank switch
                {
                    1 => new SolidColorBrush(Color.FromRgb(255, 215, 0)), // gold
                    2 => new SolidColorBrush(Color.FromRgb(192, 192, 192)), // silver
                    3 => new SolidColorBrush(Color.FromRgb(205, 127, 50)), // bronze
                    _ => new SolidColorBrush(Color.FromRgb(40, 40, 40)) // default dark
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
}
