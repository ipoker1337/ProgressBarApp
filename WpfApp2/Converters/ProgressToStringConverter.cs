using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using ProgressBarDemo.Domain.Interfaces;
using WpfApp2.ViewModels;

namespace WpfApp2.Converters {
    class ProgressToStringConverter : IValueConverter {

        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) {

            if (value is null)
                return string.Empty;

            if (!(value is IDownloadProgress))
                throw new InvalidOperationException("The target must be a IDownloadProgress");

            var downloadProgress = (IDownloadProgress) value;
            var total = DownloadHelper.ConvertBytes(downloadProgress.TotalBytesToReceive);
            return $"{DownloadHelper.ConvertBytes(downloadProgress.BytesReceived, total.Item2, true, 1)} / {total.Item1}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}

