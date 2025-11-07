using System.Globalization;
using System.Windows.Data;

namespace TableFlo.UI.Converters;

/// <summary>
/// Converts a boolean to one of two string values based on the parameter
/// Parameter format: "TrueValue|FalseValue"
/// </summary>
public class BoolToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string paramStr)
        {
            var parts = paramStr.Split('|');
            if (parts.Length == 2)
            {
                return boolValue ? parts[0] : parts[1];
            }
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

