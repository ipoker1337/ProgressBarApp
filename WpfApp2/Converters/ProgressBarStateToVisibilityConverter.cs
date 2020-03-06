using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WpfApp2.ViewModels;

namespace WpfApp2.Converters
{
    class ProgressBarStateToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is ProgressBarState) {
                var state = (ProgressBarState) value;
                //return state == ProgressBarState.Disabled ? Visibility.Collapsed : Visibility.Visible;

                return true;
            }

            throw new InvalidOperationException("The target must be a ProgressBarState");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
