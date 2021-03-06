﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
namespace TCC.Converters
{
    public class HPbarColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return ((SolidColorBrush)Application.Current.FindResource("HpDebuffColor"));
            }
            else
            {
                return ((SolidColorBrush)Application.Current.FindResource("HpColor"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
