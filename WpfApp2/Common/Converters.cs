using System;
using System.Globalization;
using System.Windows.Data;
using ProgressBarApp.Core;

namespace WpfApp2.Common {

public class 
DownloadProgressToStringConverter : IValueConverter {
    public object 
    Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
        if (value is null)
            return "ProgressBar";

        if (!(value is Progress))
            throw new InvalidOperationException("The target must be ProgressInfo");

        var downloadProgress = (Progress) value;
        var total = DownloadHelper.ConvertBytes(downloadProgress.Maximum);
        return $"{DownloadHelper.ConvertBytes(downloadProgress.Value, total.Item2, true, 1)} / {total.Item1}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class 
ProgressToIndeterminateConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
        if (value is null)
            return false;
        if (!(value is Progress))
            throw new InvalidOperationException("The target must be ProgressInfo");
        var downloadProgress = (Progress) value;
        return downloadProgress.Maximum < 1;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
}

