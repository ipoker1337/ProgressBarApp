using System;
using System.Windows.Threading;
using ProgressApp.Core;
using ProgressApp.Core.Common;

namespace WpfApp2 {

public class 
ProgressViewModel : ViewModelBase, IDisposable {
    private readonly IHasProgress _progress;
    private readonly DispatcherTimer _textTimer;
    private readonly DispatcherTimer _progressTimer;

    public ProgressViewModel(IHasProgress progress) {
        _progress = progress;

// Under development
            _textTimer = new DispatcherTimer();
            _textTimer.Interval = 500.Milliseconds();
            _textTimer.Tick += OnTextUpdate;
            _textTimer.Start();

        _progressTimer = new DispatcherTimer();
        _progressTimer.Interval = 100.Milliseconds();
        _progressTimer.Tick += OnProgressUpdate;
        _progressTimer.Start(); 
//
    }

    private void OnProgressUpdate(object? sender, EventArgs e) {
        TargetValue = _progress.Progress?.TargetValue ?? 0;
        Text = _progress.Progress?.Message;
        Value = _progress.Progress?.Value ?? 0;
        //Progress = _progressProvider.Progress;
    }


    private void OnTextUpdate(object? sender, EventArgs e) {
        Progress = _progress.Progress;
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
        private set => SetPropertyIfChanged(ref _targetValue, value);
    }

    private Progress? _progressProperty;
    public Progress? Progress {
        get => _progressProperty;
        set => SetPropertyIfChanged(ref _progressProperty, value);
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
