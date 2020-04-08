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
    
    private long _lastBytePosition;
    private string? _error;
    private string _commandText = string.Empty;
    private RelayCommand _command = new RelayCommand();

    public State CurrentState { get; private set; }

    public enum State { Idle, Active, Paused }
    public enum Trigger { Start, Pause, Cancel }

    public State 
    TransitionTo(Trigger trigger) =>
    (CurrentState, trigger) switch {
        (State.Idle, Trigger.Start) => ((Func<State>)(() => {
            Error = null;
            CommandText = "Pause";
            Command = PauseCommand;
            DownloadExecute();
            return CurrentState = State.Active;
        }))(),
        (State.Active, Trigger.Cancel) => ((Func<State>)(() => {
            _cancellationToken.Cancel();
            Reset();
            return CurrentState = State.Idle;
        }))(),
        (State.Active, Trigger.Pause) => ((Func<State>)(() => {
            _cancellationToken.Cancel();
            CommandText = "Resume";
            Command = StartCommand;
            return CurrentState = State.Paused;
        }))(),
        (State.Paused, Trigger.Cancel) => ((Func<State>)(() => {
            Reset();
            return CurrentState = State.Idle;
        }))(),
        (State.Paused, Trigger.Start) => ((Func<State>)(() => {
            CommandText = "Pause";
            Command = PauseCommand;
            DownloadExecute();
            return CurrentState = State.Active;
        }))(),
        _ => throw new NotSupportedException($"{CurrentState} has no transition on {trigger}")
    };

    public MainViewModel() {
        DownloadProgress = new ProgressViewModel(_progressHandler);
        CurrentState = State.Idle;
        Reset();
    }

    public ProgressViewModel DownloadProgress { get; }

    public string? Error { get => _error; private set => SetPropertyIfChanged(ref _error, value); }
    public string CommandText { get => _commandText; private set => SetPropertyIfChanged(ref _commandText, value); }
    public RelayCommand Command { get => _command; private set => SetPropertyIfChanged(ref _command, value); }

    public RelayCommand StartCommand => new RelayCommand(() => TransitionTo(Trigger.Start), () => CurrentState != State.Active);
    public RelayCommand CancelCommand => new RelayCommand(() => TransitionTo(Trigger.Cancel), () => CurrentState != State.Idle);
    public RelayCommand PauseCommand => new RelayCommand(() => TransitionTo(Trigger.Pause), () =>  CurrentState == State.Active);

    // under development
    private async void 
    DownloadExecute() {
        if (_fileStream == Stream.Null)
            _fileStream = File.Create(_fileName);

        var result = await Download.FileAsync(new Uri(@"http://87.76.21.20/test.zip"), _fileStream, _cancellationToken.Token, 
                                              _progressHandler, _lastBytePosition);

        result.OnSuccess(Reset)
              .OnFailure(HandleException, x => x.Exception != null)
              .OnFailure(Pause, _ => CurrentState == State.Paused)
              .OnBoth(_ => { _cancellationToken = new CancellationTokenSource(); Command.Refresh(); });

        void
        HandleException(Result value) {
            Error = value.Exception?.Message;
            Reset();
        }

        void
        Pause(Result value) {
            _lastBytePosition = value.BytesReceived;
            _progressHandler.Report("Paused");
        }
    }

    private void
    Reset() {
        _fileStream.Close();
        _fileStream = Stream.Null;
        _progressHandler.Report(0, 0);
        _lastBytePosition = 0;
        CommandText = "Start";
        Command = StartCommand;
    }

    public void
    Dispose() {
        _cancellationToken.Dispose();
        DownloadProgress.Dispose();
    }
}
}
