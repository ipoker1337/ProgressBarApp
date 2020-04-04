using System;
using System.IO;
using System.Threading;
using System.Windows.Input;
using ProgressApp.Core;
using WpfApp2.Common;

namespace WpfApp2 {

public class 
MainViewModel : ViewModelBase, IDisposable {
    private const string _fileName = "test.zip";

    private readonly RelayCommand _startCommand;

    private CancellationTokenSource _cancellationTokenSource;
    private Stream _fileStream = Stream.Null;
    
    private bool _isCanceled;
    private bool _isDownloading;
    private long _lastBytePosition;

    public MainViewModel() {
        _cancellationTokenSource = new CancellationTokenSource();
        var progressHandler = new ProgressHandler();
        DownloadProgress = new ProgressViewModel(progressHandler);

        var pauseCommand = new RelayCommand(() => { _isCanceled = false; _cancellationTokenSource.Cancel(); }, () => !_cancellationTokenSource.IsCancellationRequested && !_startCommand.CanExecute());
        CancelCommand = new RelayCommand(() => { _isCanceled = true; _cancellationTokenSource.Cancel(); }, () => !_cancellationTokenSource.IsCancellationRequested && !_startCommand.CanExecute());
        Command = _startCommand = new RelayCommand(DownloadExecute, () => !_isDownloading);

        async void 
        DownloadExecute() {
            _isDownloading = true;
            CommandText = "Pause";
            Command = pauseCommand;

            if (_fileStream == Stream.Null)
                _fileStream = File.Create(_fileName);

            var result = await Download.FileAsync(new Uri(@"http://87.76.21.20/test.zip"), _fileStream, _cancellationTokenSource.Token, progressHandler, _lastBytePosition);
            _cancellationTokenSource = new CancellationTokenSource();
            _isDownloading = false;
            Command = _startCommand;

            if (result.IsFailure && !_isCanceled) {
                // pause
                CommandText = "Resume";
                _lastBytePosition = result.TotalBytesReceived;
                return;
            }
            // completed or canceled
            CommandText = "Start";
            Command.Refresh();
            _lastBytePosition = 0;
            _fileStream.Close();
            _fileStream = Stream.Null;
        }
    }

    public ProgressViewModel DownloadProgress { get; }

    public RelayCommand CancelCommand { get; }

    private RelayCommand _command = new RelayCommand();
    public RelayCommand Command {
        get => _command;
        set => SetPropertyIfChanged(ref _command, value);
    }

    private string _commandText = "Start";
    public string CommandText {
        get => _commandText;
        set => SetPropertyIfChanged(ref _commandText, value);
    }

    public void 
    Dispose() {
        _cancellationTokenSource.Dispose();
        DownloadProgress.Dispose();
    }
}
}

        //var result = await Download.FileAsync(new Uri(@"https://speed.hetzner.de/100MB.bin"), _fileStream, _cancellationTokenSource.Token, progressHandler, _resumeBytePosition);
        //var result = await Download.FileAsync(new Uri(@"http://h2n-uptoyou.azureedge.net/main/Hand2NoteInstaller.exe"), _fileStream, 
        //                                      _cancellationTokenSource.Token, _progressHandler, _lastBytePosition);
