using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ProgressApp.Core;
using ProgressApp.Core.Common;
using Byte = ProgressApp.Core.Common.Byte;

namespace WpfApp2.Common {

public class 
ProgressToTextConverter : IValueConverter {
    private DateTime _lastUpdate = DateTime.MinValue;

    private int _updateThresholdMs;
    public int UpdateThresholdMs {
        get => _updateThresholdMs;
        set => _updateThresholdMs = value.VerifyNonNegative();
    }

    public object 
    Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
        if (value is null)
            return string.Empty;
        var now = DateTime.Now;
        var elapsed = now - _lastUpdate;
        if (UpdateThresholdMs > elapsed.TotalMilliseconds)
            return Binding.DoNothing;
        _lastUpdate = now;
        return ProgressToText((Progress) value);

        static string 
        ProgressToText(Progress progress) {
            var rate = progress.Rate.FromBytes().ToReadable();
            if (!progress.TargetValue.HasValue || progress.TargetValue < progress.Value)
                return $"{rate}/s - {progress.Value.FromBytes().ToReadable()}";

            var targetValue = progress.TargetValue?.FromBytes().ToReadable();
            var unit = progress.TargetValue?.FromBytes().LargestUnit;
            var currentValue = progress.Value.FromBytes().ToReadable(unit ?? Byte.Unit.Megabytes);
            // 4.9 MB/s - 24.7 MB of 58.6 MB, 6 secs left
            return $"{rate}/s - {currentValue} of {targetValue}, {progress.TimeLeft.ToReadable()} left";
        }
    }

    public object 
    ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class 
ProgressToIndeterminateConverter : IValueConverter {
    public object 
    Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is null)
            return false;
        var progress = (Progress) value;
        return !progress.TargetValue.HasValue || progress.TargetValue < progress.Value;
    }

    public object 
    ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class
ProgressToValueConverter : IValueConverter {
    public object
    Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((Progress) value)?.Value ?? 0;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
    throw new NotImplementedException();
}

public class
NullToVisibilityConverter : IValueConverter {
    public object 
    Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        value == null ? Visibility.Collapsed : Visibility.Visible;

    public object 
    ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

}

