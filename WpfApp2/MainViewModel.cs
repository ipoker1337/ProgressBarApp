using System;
using System.IO;
using System.Threading;
using ProgressApp.Core;
using WpfApp2.Common;

namespace WpfApp2 {

public class 
MainViewModel : ViewModel, IDisposable {
    private const string _fileName = "test.zip";
    private readonly ProgressObserver _progressObserver = new ProgressObserver();
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

    private long _lastBytePosition;
    private string? _error;

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

    public RelayCommand StartCommand => new RelayCommand(() => Fire(Trigger.Start), () => CurrentState == State.Idle);
    public RelayCommand PauseCommand => new RelayCommand(() => Fire(Trigger.Pause), () =>  CurrentState == State.Running);
    public RelayCommand ResumeCommand => new RelayCommand(() => Fire(Trigger.Start), () => CurrentState == State.Paused);
    public RelayCommand CancelCommand => new RelayCommand(() => Fire(Trigger.Cancel), () => CurrentState != State.Idle);

    // under development
    private async void 
    DownloadExecute() {
        try {
            await using var fileStream = File.Open(_fileName, FileMode.Append);
            var result = await Download.FileAsync(new Uri(@"http://87.76.21.20/test.zip"), fileStream, _cancellationToken.Token, _progressObserver, _lastBytePosition);
            if (result.IsSuccess) 
                Fire(Trigger.End);
            if (CurrentState == State.Paused)
                Pause(result);
            else {
                Reset();
            }
        }
        catch (Exception ex) {
            Error = ex.Message;
            Fire(Trigger.Cancel);
        }
        finally {
            _cancellationToken = new CancellationTokenSource();
            //Command.Refresh();
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

        //Command.Refresh();
    }

    private void
    Fire(Trigger trigger) {
        CurrentState = TransitionTo(CurrentState, trigger);

        State
        TransitionTo(State state, Trigger value) =>
            (state, trigger: value) switch {
                (State.Idle, Trigger.Start) => ((Func<State>) (() => {
                    Error = null;
                    if (_lastBytePosition == 0)
                        if (File.Exists(_fileName)) File.Delete(_fileName);
                    DownloadExecute();
                    return State.Running;
                }))(),
                (State.Running, Trigger.Cancel) => ((Func<State>) (() => {
                    _cancellationToken.Cancel();
                    return State.Idle;
                }))(),
                (State.Running, Trigger.Pause) => ((Func<State>) (() => {
                    _cancellationToken.Cancel();
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
}
