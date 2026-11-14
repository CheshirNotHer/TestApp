using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TestApp.Infrastructure; 

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object v, Type t, object? p, CultureInfo c) =>
        v is bool b && b ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object v, Type t, object? p, CultureInfo c) =>
        throw new NotSupportedException();
}