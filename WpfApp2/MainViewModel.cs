using System;
using System.IO;
using System.Threading;
using ProgressApp.Core;
using WpfApp2.Common;

namespace WpfApp2 {

public class 
MainViewModel : ViewModelBase, IDisposable {
    private const string _fileName = "test.zip";

    private readonly RelayCommand _startCommand;
    private readonly RelayCommand _pauseCommand;

    private CancellationTokenSource _cts;
    private Stream _fileStream = Stream.Null;
    
    private bool _isCanceled;
    private long _lastBytePosition;

    public MainViewModel() {
        _cts = new CancellationTokenSource();
        var progressHandler = new ProgressHandler();
        DownloadProgress = new ProgressViewModel(progressHandler);

        _pauseCommand = new RelayCommand(() => { _isCanceled = false; _cts.Cancel(); }, CanCancel);
        CancelCommand = new RelayCommand(() => { _isCanceled = true; _cts.Cancel(); }, CanCancel);
        Command = _startCommand = new RelayCommand(DownloadExecute, () => !IsDownloading);

        bool CanCancel () => !_cts.IsCancellationRequested && !_startCommand.CanExecute();
// Under development
        async void 
        DownloadExecute() {
            IsDownloading = true;
            if (_fileStream == Stream.Null)
                _fileStream = File.Create(_fileName);

            var result = await Download.FileAsync(new Uri(@"http://87.76.21.20/test.zip"), _fileStream, _cts.Token, progressHandler, _lastBytePosition);
            _cts = new CancellationTokenSource();
            IsDownloading = false;

            if (result.IsFailure) {
                if (result.Exception != null) {
                    Error = $"Error: {result.Exception.Message}";
                    Reset();
                    return;
                }
                if (_isCanceled)
                    progressHandler.Report(0, 0, "Canceled");
                else {
                    progressHandler.Report("Paused");
                    CommandText = "Resume";
                    _lastBytePosition = result.TotalBytesReceived;
                    return;
                }
            }
            if (result.IsSuccess)  
                progressHandler.Report("Finished");

            Reset();

            void
            Reset() {
                // completed or canceled
                CommandText = "Start";
                Command.Refresh();
                _lastBytePosition = 0;
                _fileStream.Close();
                _fileStream = Stream.Null;
            }
        }
    }

    private string _error = string.Empty;
    public string Error { get => _error; set => SetPropertyIfChanged(ref _error, value); }

    private bool _isDownloading;
    public bool IsDownloading {
        get => _isDownloading;
        set {
            if (value) {
                CommandText = "Pause";
                Command = _pauseCommand;
            }
            else {
                Command = _startCommand;
            }

            SetPropertyIfChanged(ref _isDownloading, value);
        }
    }

    public ProgressViewModel DownloadProgress { get; }

    private RelayCommand _command = new RelayCommand();
    public RelayCommand Command { get => _command; set => SetPropertyIfChanged(ref _command, value); }

    public RelayCommand CancelCommand { get; }

    private string _commandText = "Start";
    public string CommandText { get => _commandText; set => SetPropertyIfChanged(ref _commandText, value); }

    public void
    Dispose() {
        _cts.Dispose();
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