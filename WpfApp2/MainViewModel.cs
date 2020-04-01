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
    private readonly ICommand _cancelCommand;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public MainViewModel() {
        var progressHandler = new ProgressHandler();
        Progress = new ProgressViewModel(progressHandler);

        _cancelCommand = new RelayCommand(
            () => { _cancellationTokenSource.Cancel(); }, 
            () => !_cancellationTokenSource.IsCancellationRequested);

        Command = _startCommand = new RelayCommand(async () => {
            CommandText = "Cancel";
            Command = _cancelCommand;

            _cancellationTokenSource = new CancellationTokenSource();
            var result = await Download.FileAsync(new Uri(@"https://speed.hetzner.de/100MB.bin"), Stream.Null,  _cancellationTokenSource.Token, progressHandler);

            if (!result.IsSuccess && result.Exception != null)
                progressHandler.Report("Error: " + result.Exception.Message);

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

    private string _commandText = "Start";
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
