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
    private long _lastBytePosition;

    public MainViewModel() {
        DownloadProgress = new ProgressViewModel(_progressHandler);
        IsDownloading = false;
        Command = StartCommand;
    }

    public ProgressViewModel DownloadProgress { get; }

    private string _commandText = string.Empty;
    public string CommandText { get => _commandText; set => SetPropertyIfChanged(ref _commandText, value); }

    private RelayCommand _command = new RelayCommand();
    public RelayCommand Command { get => _command; set => SetPropertyIfChanged(ref _command, value); }

    public RelayCommand StartCommand => new RelayCommand(DownloadExecute, () => !IsDownloading);
    public RelayCommand PauseCommand => new RelayCommand(() => { _isPaused = true; _cancellationToken.Cancel();}, 
                                                         () => IsDownloading && !_cancellationToken.IsCancellationRequested);

    public RelayCommand CancelCommand => new RelayCommand(() => { _isPaused = false; _cancellationToken.Cancel(); }, 
                                                          () => _fileStream != Stream.Null && IsDownloading && !_cancellationToken.IsCancellationRequested);

    private string _error = string.Empty;
    public string Error { get => _error; set => SetPropertyIfChanged(ref _error, value); }

    private bool _isDownloading;
    public bool IsDownloading {
        get => _isDownloading;
        set {
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

        if (result.IsException) {
            Error = $"Error: {result.Exception?.Message}";
            Reset();
        }
        else if (result.IsFailure && _isPaused) {
            _progressHandler.Report("Paused");
            _lastBytePosition = result.TotalBytesReceived;
        }
        else {
            Reset();
        }

        _cancellationToken = new CancellationTokenSource();
        Command.Refresh();

        void
        Reset() {
            _fileStream.Close();
            _fileStream = Stream.Null;
            CommandText = "Start";
            Command = StartCommand;
            _progressHandler.Report(0, 0);
            _lastBytePosition = 0;
        }
    }

    public void
    Dispose() {
        _cancellationToken.Dispose();
        DownloadProgress.Dispose();
    }
}
}

        //var result = await Download.FileAsync(new Uri(@"https://speed.hetzner.de/100MB.bin"), _fileStream, _cancellationTokenSource.Token, progressHandler, _resumeBytePosition);
        //var result = await Download.FileAsync(new Uri(@"http://h2n-uptoyou.azureedge.net/main/Hand2NoteInstaller.exe"), _fileStream, 
        //                                      _cancellationTokenSource.Token, _progressHandler, _lastBytePosition);


     //async void 
     //   DownloadExecute() {
     //       IsDownloading = true;
     //       //_isDownloading = true;
     //       //CommandText = "Pause";
     //       //Command = pauseCommand;

     //       if (_fileStream == Stream.Null)
     //           _fileStream = File.Create(_fileName);

     //       var result = await Download.FileAsync(new Uri(@"http://87.76.21.20/test.zip"), _fileStream, _cts.Token, progressHandler, _lastBytePosition);
     //       _cts = new CancellationTokenSource();
     //       IsDownloading = false;
     //       //_isDownloading = false;
     //       //Command = _startCommand;

     //       if (result.IsFailure && result.Exception != null) {
     //           progressHandler.Report(0, 0, "Error: " + result.Exception.Message);
     //           Reset();
     //           return;
     //       }
     //       if (result.IsFailure && !_isCanceled) {
     //           // pause
     //           progressHandler.Report("Paused");
     //           CommandText = "Resume";
     //           _lastBytePosition = result.TotalBytesReceived;
     //           return;
     //       }
     //       if (result.IsFailure && _isCanceled) {
     //           progressHandler.Report(0, 0, "Canceled");
     //       }
     //       if (result.IsSuccess) 
     //           progressHandler.Report("Finished");

     //       Reset();

     //       void
     //       Reset() {
     //           // completed or canceled
     //           CommandText = "Start";
     //           Command.Refresh();
     //           _lastBytePosition = 0;
     //           _fileStream.Close();
     //           _fileStream = Stream.Null;
     //       }
     //   }