using System;
using System.Windows.Threading;
using ProgressApp.Core;
using ProgressApp.Core.Common;

namespace ProgressApp.Wpf {
public class 
ProgressViewModel : ViewModel, IDisposable {
    private readonly IHasProgress _provider;
    private readonly DispatcherTimer _timer;

    public ProgressViewModel(IHasProgress provider, int refreshRateMs = 50) {
        _provider = provider;
        _timer = new DispatcherTimer();
        _timer.Interval = refreshRateMs.Milliseconds().VerifyGreaterZero();
        _timer.Tick += OnProgressUpdate;
        _timer.Start();
    }

    private void OnProgressUpdate(object? sender, EventArgs e) {
        TargetValue = _provider.Progress?.TargetValue ?? 0;
        Caption = _provider.Progress?.Message ?? string.Empty;
        Progress = _provider.Progress;
    }

    private long _targetValue;
    public long TargetValue {
        get => _targetValue;
        private set => SetPropertyIfChanged(ref _targetValue, value);
    }

    private Progress? _progress;
    public Progress? Progress {
        get => _progress;
        private set => SetPropertyIfChanged(ref _progress, value);
    }

    private string _caption = string.Empty;
    public string Caption {
        get => _caption;
        private set => SetPropertyIfChanged(ref _caption, value);
    }

    public void
    Dispose() {
        _timer.Tick -= OnProgressUpdate;
        _timer.Stop();
    }
}
}
