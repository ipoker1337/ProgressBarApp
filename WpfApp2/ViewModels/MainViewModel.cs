using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ProgressBarApp.Core;
using WpfApp2.Infrastructure;

namespace WpfApp2.ViewModels {

public class 
MainViewModel : ViewModelBase, IDisposable {
    private readonly ICommand _startCommand;
    private readonly ICommand _cancelCommand;

    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private ICommand _command;
    private string _commandText;

    public MainViewModel() {

        var fileDownloader = new TestFileDownloader();
        ProgressBar = new ProgressBarViewModel(fileDownloader);

        _startCommand = new RelayCommand(async () => { await StartAsync(); });
        _cancelCommand = new RelayCommand(Cancel);

        ResetToDefault();

        async Task StartAsync() {
            CommandText = "Cancel";
            Command = _cancelCommand;

            _cancellationTokenSource = new CancellationTokenSource();
            var downloadResult = await fileDownloader.DownloadFileAsync(new Uri(@"http://www.source.com/"), "destination", _cancellationTokenSource.Token);
        }

        void Cancel() {
            _cancellationTokenSource.Cancel();
            CommandText = "Install";
            Command = _startCommand;
            }

        void ResetToDefault() {
            CommandText = "Install";
            Command = _startCommand;
        }
    }

    #region Properties

    public ProgressBarViewModel ProgressBar { get; }

    public ICommand Command {
        get => _command;
        set => this.SetPropertyIfChanged(ref _command, value);
    }

    public string CommandText {
        get => _commandText;
        set => this.SetPropertyIfChanged(ref _commandText, value);
    }

    #endregion

    public void 
    Dispose() {
        _cancellationTokenSource.Dispose();
        ProgressBar.Dispose();
    }
}
}
