using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using ProgressApp.Core;
using ProgressApp.Core.Common;
using static ProgressApp.Core.Common.ByteSize;

namespace ProgressApp.Wpf.Common {
public abstract class
ConverterBase : MarkupExtension, IValueConverter {
    public override object 
    ProvideValue(IServiceProvider serviceProvider) => this;

    public abstract object 
    Convert(object value, Type targetType, object parameter, CultureInfo culture);

    public object 
    ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class 
ProgressToTextConverter : ConverterBase {
    private DateTime _lastUpdate = DateTime.MinValue;

    private int _updateThresholdMs;
    public int UpdateThresholdMs {
        get => _updateThresholdMs;
        set => _updateThresholdMs = value.VerifyNonNegative();
    }

    public override object 
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
            var currentValue = progress.Value.FromBytes().ToReadable(unit ?? Unit.Megabytes);
            // 4.9 MB/s - 24.7 MB of 58.6 MB, 6 secs left
            return $"{rate}/s - {currentValue} of {targetValue}, {progress.TimeLeft.ToReadable()} left";
        }
    }
}

public class 
ProgressToIndeterminateConverter : ConverterBase {
    public override object 
    Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is null)
            return false;
        var progress = (Progress) value;
        return !progress.TargetValue.HasValue || progress.TargetValue < progress.Value;
    }
}

public class
ProgressToValueConverter : ConverterBase {
    public override object
    Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((Progress) value)?.Value ?? 0;
}

public class
NullToVisibilityConverter : ConverterBase {
    public override object 
    Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        value == null ? Visibility.Collapsed : Visibility.Visible;
}

public class
InverseNullToVisibilityConverter : ConverterBase {
    public override object 
    Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value != null ? Visibility.Collapsed : Visibility.Visible;

}

public class
BooleanToVisibilityConverter : ConverterBase {
    public override object 
    Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        (bool)value ? Visibility.Visible : Visibility.Collapsed;
}

public class
InverseBooleanToVisibilityConverter : ConverterBase {
    public override object 
    Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        (bool)value ? Visibility.Collapsed : Visibility.Visible;
}


}

