using System;
using System.Windows.Threading;
using ProgressApp.Core;
using ProgressApp.Core.Common;

namespace WpfApp2 {

public class 
ProgressViewModel : ViewModelBase, IDisposable {
    private readonly IProgressProvider _progressProvider;
    private readonly DispatcherTimer _textTimer;
    private readonly DispatcherTimer _progressTimer;

    public ProgressViewModel(IProgressProvider progressProvider) {
        _progressProvider = progressProvider;

        _textTimer = new DispatcherTimer();
        _textTimer.Interval = 500.Milliseconds();
        _textTimer.Tick += OnTextUpdate;
        _textTimer.Start();

        _progressTimer = new DispatcherTimer();
        _progressTimer.Interval = 100.Milliseconds();
        _progressTimer.Tick += OnProgressUpdate;
        _progressTimer.Start();

        //_progressProvider.ProgressChanged += OnProgressChanged;
        Text = "TEST";
    }

    private void OnProgressUpdate(object? sender, EventArgs e) {
        TargetValue = _progressProvider.Progress?.TargetValue ?? 0;
        Text = _progressProvider.Progress?.Message;
        Value = _progressProvider.Progress?.Value ?? 0;
    }


    private void OnTextUpdate(object? sender, EventArgs e) {
        Progress = _progressProvider.Progress;
    }

    private void 
    OnProgressChanged(Progress p) {
        TargetValue = p.TargetValue;
        Value = p.Value;
        Text = p.Message;
        Progress = p;
    }

    #region Properties

    private long _value;
    public long Value {
        get => _value;
        private set => SetPropertyIfChanged(ref _value, value);
    }

    private long _targetValue;
    public long TargetValue {
        get => _targetValue;
        private set {
            if (Value > value) Value = value;
            SetPropertyIfChanged(ref _targetValue, value);
        }
    }

    private Progress? _progress;
    public Progress? Progress {
        get => _progress;
        set => SetPropertyIfChanged(ref _progress, value);
    }

    private string? _text = string.Empty;
    public string? Text {
        get => _text;
        set => SetPropertyIfChanged(ref _text, value);
    }

    #endregion

    public void 
    Dispose() {
        _progressTimer.Tick -= OnProgressUpdate;
        _textTimer.Tick -= OnTextUpdate;
    }
}
}
