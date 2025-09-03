using System;
using System.Globalization;
using System.Windows.Data;

namespace DocFinder.App.Converters;

public sealed class FileSizeConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long bytes)
        {
            double size = bytes;
            string[] order = { "B", "KB", "MB", "GB", "TB" };
            int i = 0;
            while (size >= 1024 && i < order.Length - 1)
            {
                size /= 1024;
                i++;
            }
            return string.Format(culture, "{0:0.00} {1}", size, order[i]);
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

