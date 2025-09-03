using System;
using System.Globalization;
using System.Windows.Data;

namespace DocFinder.App.Converters;

public sealed class UtcToLocalConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dt)
        {
            return dt.ToLocalTime().ToString("g", culture);
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
