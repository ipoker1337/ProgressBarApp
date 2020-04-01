using System;
using System.IO;
using System.Threading;
using System.Windows.Input;
using ProgressApp.Core;
using WpfApp2.Common;

namespace WpfApp2 {

public class 
MainViewModel : ViewModelBase, IDisposable {
    private readonly ICommand _startCommand;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public MainViewModel() {
        var fileDownloader = new TestFileDownloader();
        //var progressHandler = new ProgressHandler();
        Progress = new ProgressViewModel(fileDownloader);

        var cancelCommand = new RelayCommand(
            () => { _cancellationTokenSource.Cancel(); }, 
            () => !_cancellationTokenSource.IsCancellationRequested);

        CommandText = "Start";
        Command = _startCommand = new RelayCommand(async () => {
            CommandText = "Cancel";
            Command = cancelCommand;
            _cancellationTokenSource = new CancellationTokenSource();
            //await Download.FileAsync(new Uri(@"https://speed.hetzner.de/100MB.bin"), Stream.Null,  _cancellationTokenSource.Token, progressHandler);
            var result = await fileDownloader.DownloadFileAsync(new Uri(@"https://source"), "dest", _cancellationTokenSource.Token);
            CommandText = "Start";
            Command = _startCommand;
        });
    }

    public ProgressViewModel Progress { get; }

    private ICommand _command = new RelayCommand();
    public ICommand Command {
        get => _command;
        set => SetPropertyIfChanged(ref _command, value);
    }

    private string _commandText = string.Empty;
    public string CommandText {
        get => _commandText;
        set => SetPropertyIfChanged(ref _commandText, value);
    }

    public void 
    Dispose() {
        _cancellationTokenSource.Dispose();
        Progress.Dispose();
    }
}
}
