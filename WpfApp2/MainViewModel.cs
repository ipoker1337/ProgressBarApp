using System;
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
        Progress = new ProgressViewModel(fileDownloader);

        var cancelCommand = new RelayCommand(
            () => { _cancellationTokenSource.Cancel(); }, 
            () => !_cancellationTokenSource.IsCancellationRequested);
        CommandText = "Start";

        Command = _startCommand = new RelayCommand(async () => {
            CommandText = "Cancel";
            Command = cancelCommand;
            _cancellationTokenSource = new CancellationTokenSource();
            await fileDownloader.DownloadFileAsync(new Uri(@"http://www.source.com/"), "destination", _cancellationTokenSource.Token);
            CommandText = "Start";
            Command = _startCommand;
        });
    }

    public ProgressViewModel Progress { get; }

    private ICommand _command = new RelayCommand();
    public ICommand Command {
        get => _command;
        set => this.SetPropertyIfChanged(ref _command, value);
    }

    private string _commandText = string.Empty;
    public string CommandText {
        get => _commandText;
        set => this.SetPropertyIfChanged(ref _commandText, value);
    }

    public void 
    Dispose() {
        _cancellationTokenSource.Dispose();
        Progress.Dispose();
    }
}
}
