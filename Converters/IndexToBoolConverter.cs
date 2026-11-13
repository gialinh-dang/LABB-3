using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Windows.Data;

namespace LABB_3.Converters
{
    // Radio button: is this button's number (parameter) equal to CorrectIndex?
    public class IndexToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == parameter?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is true)
                return int.Parse(parameter.ToString()!);

            return Binding.DoNothing;
        }
    }
}