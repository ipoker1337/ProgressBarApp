using System;
using System.IO;
using System.Threading;
using ProgressApp.Core;
using WpfApp2.Common;

namespace WpfApp2 {

// under development

public class 
DownloadContext {
    public Uri RequestUri { get; }
    public string FileName { get; }
    public string Directory { get; }
    public ILogger Logger { get; }
    public ProgressObserver ProgressObserver { get; } = new ProgressObserver();
    public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

    public DownloadContext(Uri requestUri, string filename, string directory, ILogger? logger = null) =>
        (RequestUri, FileName, Directory, Logger) = (requestUri, filename, directory, logger ?? DefaultLogger.Null);
}

public class
DownloadViewModel {
    public DownloadViewModel(DownloadContext ctx) {
        DownloadProgress = new ProgressViewModel(ctx.ProgressObserver);
        CurrentState = State.Idle;
    }

    public enum State { Idle, Running, Paused }
    private enum Trigger { Start, Pause, Cancel, End }

    public State CurrentState { get; private set; }

    public ProgressViewModel DownloadProgress { get; }
}

public class 
MainViewModel : ViewModel, IDisposable {
    private const string _fileName = "test.zip";
    private readonly ProgressObserver _progressObserver = new ProgressObserver();
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

    private long _lastBytePosition;
    private string? _error;
    private string _commandText = string.Empty;
    private RelayCommand _command = new RelayCommand();

    public MainViewModel() {
        DownloadProgress = new ProgressViewModel(_progressObserver);
        CurrentState = State.Idle;
        Reset();
    }

    public enum State { Idle, Running, Paused }
    private enum Trigger { Start, Pause, Cancel, End }

    public State CurrentState { get; private set; }

    public ProgressViewModel DownloadProgress { get; }

    public string? Error { get => _error; private set => SetPropertyIfChanged(ref _error, value); }
    public string CommandText { get => _commandText; private set => SetPropertyIfChanged(ref _commandText, value); }
    public RelayCommand Command { get => _command; private set => SetPropertyIfChanged(ref _command, value); }

    public RelayCommand StartCommand => new RelayCommand(() => Fire(Trigger.Start), () => CurrentState != State.Running);
    public RelayCommand CancelCommand => new RelayCommand(() => Fire(Trigger.Cancel), () => CurrentState != State.Idle);
    public RelayCommand PauseCommand => new RelayCommand(() => Fire(Trigger.Pause), () =>  CurrentState == State.Running);

    // under development
    private async void 
    DownloadExecute() {
        try {
            // TODO: Исправить -  deltaValue передается после отмены операции и ProgressBar переходит в Indeterminate режим
            await using var fileStream = File.Open(_fileName, FileMode.Append);
            var result = await Download.FileAsync(new Uri(@"http://87.76.21.20/test.zip"), fileStream, _cancellationToken.Token, _progressObserver, _lastBytePosition);
            if (result.IsSuccess) 
                Fire(Trigger.End);
            if (CurrentState == State.Paused)
                Pause(result);
        }
        catch (Exception ex) {
            Error = ex is OperationCanceledException ? null : ex.Message;
            Fire(Trigger.Cancel);
        }
        finally {
            _cancellationToken = new CancellationTokenSource();
            Command.Refresh();
        }

        void
        Pause(Result value) {
            _lastBytePosition = value.BytesReceived;
            _progressObserver.OnProgress("Paused");
        }
    }

    private void
    Reset() {
        _progressObserver.Reset();
        _lastBytePosition = 0;
        CommandText = "Start";
        Command = StartCommand;
        Command.Refresh();
    }

    private void
    Fire(Trigger trigger) {
        CurrentState = TransitionTo(CurrentState, trigger);

        State
        TransitionTo(State state, Trigger value) =>
            (state, trigger: value) switch {
                (State.Idle, Trigger.Start) => ((Func<State>) (() => {
                    Error = null;
                    CommandText = "Pause";
                    Command = PauseCommand;
                    if (_lastBytePosition == 0)
                        if (File.Exists(_fileName)) File.Delete(_fileName);
                    DownloadExecute();
                    return State.Running;
                }))(),
                (State.Running, Trigger.Cancel) => ((Func<State>) (() => {
                    _cancellationToken.Cancel();
                    Reset();
                    return State.Idle;
                }))(),
                (State.Running, Trigger.Pause) => ((Func<State>) (() => {
                    _cancellationToken.Cancel();
                    CommandText = "Resume";
                    Command = StartCommand;
                    return State.Paused;
                }))(),
                (State.Running, Trigger.End) => ((Func<State>) (() => {
                    Reset();
                    return State.Idle;
                }))(),
                (State.Paused, Trigger.Cancel) => ((Func<State>) (() => {
                    Reset();
                    return State.Idle;
                }))(),
                (State.Paused, Trigger.Start) => ((Func<State>) (() => {
                    CommandText = "Pause";
                    Command = PauseCommand;
                    DownloadExecute();
                    return State.Running;
                }))(),
                _ => throw new NotSupportedException($"{CurrentState} has no transition on {value}")
            };
    }

    public void
    Dispose() {
        _cancellationToken.Dispose();
        DownloadProgress.Dispose();
    }
}
}
