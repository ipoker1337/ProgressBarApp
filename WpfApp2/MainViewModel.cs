using System;
using System.IO;
using System.Threading;
using ProgressApp.Core;
using WpfApp2.Common;

namespace WpfApp2 {

public class 
MainViewModel : ViewModelBase, IDisposable {
    private const string _fileName = "test.zip";
    private readonly ProgressHandler _progressHandler = new ProgressHandler();
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
    private Stream _fileStream = Stream.Null;
    
    private bool _isPaused;
    private bool _isDownloading;
    private long _lastBytePosition;
    private string _error = string.Empty;
    private string _commandText = string.Empty;
    private RelayCommand _command = new RelayCommand();

    public MainViewModel() {
        DownloadProgress = new ProgressViewModel(_progressHandler);
        IsDownloading = false;
        Command = StartCommand;
    }

    public ProgressViewModel DownloadProgress { get; }

    public string Error { get => _error; private set => SetPropertyIfChanged(ref _error, value); }
    public string CommandText { get => _commandText; private set => SetPropertyIfChanged(ref _commandText, value); }
    public RelayCommand Command { get => _command; private set => SetPropertyIfChanged(ref _command, value); }

    public RelayCommand StartCommand => new RelayCommand(DownloadExecute, () => !IsDownloading);
    public RelayCommand PauseCommand => new RelayCommand(() => { _isPaused = true; _cancellationToken.Cancel();},
                                                         () => IsDownloading && !_cancellationToken.IsCancellationRequested);
    public RelayCommand CancelCommand => new RelayCommand(() => { _isPaused = false; _cancellationToken.Cancel(); },
                                                          () => _fileStream != Stream.Null && IsDownloading && !_cancellationToken.IsCancellationRequested);

    public bool IsDownloading {
        get => _isDownloading;
        private set {
            CommandText = value ? "Pause" : (_isPaused ? "Resume" : "Start");
            Command = value ? PauseCommand : StartCommand;
            SetPropertyIfChanged(ref _isDownloading, value);
        }
    }

    // under development
    private async void 
    DownloadExecute() {
        Error = string.Empty;
        if (_fileStream == Stream.Null)
            _fileStream = File.Create(_fileName);

        IsDownloading = true;
        var result = await Download.FileAsync(new Uri(@"http://87.76.21.20/test.zip"), _fileStream, _cancellationToken.Token, _progressHandler, _lastBytePosition);
        IsDownloading = false;

        result.OnSuccess(Reset)
              .OnFailure(HandleException, x => x.Exception != null)
              .OnFailure(Pause, _ => _isPaused && _cancellationToken.IsCancellationRequested)
              .OnFailure(Reset, _ => !_isPaused && _cancellationToken.IsCancellationRequested)
              .OnBoth(_ => { _cancellationToken = new CancellationTokenSource(); Command.Refresh(); });

        void
        HandleException(Result value) {
            Error = value.Exception?.Message ?? string.Empty;
            Reset();
        }

        void
        Pause(Result value) {
            _lastBytePosition = value.BytesReceived;
            _progressHandler.Report("Paused");
        }

        void
        Reset() {
            _fileStream.Close();
            _fileStream = Stream.Null;
            _progressHandler.Report(0, 0);
            _lastBytePosition = 0;
            _isPaused = false;
            IsDownloading = false;
        }
    }

    public void
    Dispose() {
        _cancellationToken.Dispose();
        DownloadProgress.Dispose();
    }
}
}
