using System;
using ProgressBarApp.Core;
using WpfApp2.Infrastructure;

namespace WpfApp2.ViewModels {

public class 
ProgressBarViewModel : ViewModelBase, IDisposable
{
    private readonly ThrottleDispatcher _throttleDispatcher = new ThrottleDispatcher();
    private readonly IProgressProvider _progressProvider;
    private readonly double _refreshInterval;

    public ProgressBarViewModel(IProgressProvider progressProvider, double refreshIntervalMs = 0) {

        _refreshInterval = refreshIntervalMs;
        _progressProvider = progressProvider;
        _progressProvider.ProgressChanged += OnProgressChanged;
    }

    #region Properties

    private long _progressValue;
    public long Value {
        get => _progressValue;
        private set {
            if (_progressValue == value)
                return;
            _progressValue = value;
            _throttleDispatcher.Throttle(_refreshInterval, _=> OnPropertyChanged(nameof(Value)));
        }
    }

    private long _progressMaximum;
    public long Maximum {
        get => _progressMaximum;
        private set {
            if (Value > value)
                this.SetPropertyIfChanged(ref _progressValue, value);
            this.SetPropertyIfChanged(ref _progressMaximum, value);
        }
    }

    private ProgressInfo? _progressInfo;
    public ProgressInfo? Progress {
        get => _progressInfo;
        set => SetPropertyIfChanged(ref _progressInfo, value);
    }

    private string _text = string.Empty;
    public string Text {
        get => _text;
        set => this.SetPropertyIfChanged(ref _text, value);
    }

    #endregion

    private void 
    OnProgressChanged(object? sender, ProgressInfo p) {
        Maximum = p.Maximum;
        Value += p.ValueDelta;
        Text = p.Message;
        Progress = p;
    }

    public void 
    Dispose() => _progressProvider.ProgressChanged -= OnProgressChanged;
}
}
