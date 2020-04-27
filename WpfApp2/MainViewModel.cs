using System;
using System.IO;
using System.Threading;
using ProgressApp.Core;
using ProgressApp.Core.Common;
using WpfApp2.Common;

namespace WpfApp2 {

public class 
MainViewModel : ViewModel, IDisposable {
    private readonly string _fileName;
    private readonly Uri _sourceUri = new Uri(@"http://87.76.21.20/test.zip");
    private readonly ProgressObserver _progressObserver = new ProgressObserver();

    private string? _error;
    private long _initialBytePosition;
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

    public MainViewModel() {
        _fileName = Path.GetFileName(_sourceUri.LocalPath);
        DownloadProgress = new ProgressViewModel(_progressObserver);
        CurrentState = State.Idle;
    }

    private enum State { Idle, Running, Paused }
    private enum Trigger { Start, Pause, Cancel, End }

    private State CurrentState { get; set; }
    public ProgressViewModel DownloadProgress { get; }
    public string? Error { get => _error; private set => SetPropertyIfChanged(ref _error, value); }

    public RelayCommand StartCommand => new RelayCommand(() => Fire(Trigger.Start), () => CurrentState == State.Idle);
    public RelayCommand PauseCommand => new RelayCommand(() => Fire(Trigger.Pause), () =>  CurrentState == State.Running);
    public RelayCommand ResumeCommand => new RelayCommand(() => Fire(Trigger.Start), () => CurrentState == State.Paused);
    public RelayCommand CancelCommand => new RelayCommand(() => Fire(Trigger.Cancel), () => CurrentState != State.Idle);

    private async void 
    DownloadExecute() {
        try {
            Error = null;
            if (_initialBytePosition == 0 || _initialBytePosition != _fileName.GetFileSizeOrZero()) {
                _fileName.DeleteFileIfExist();
                _initialBytePosition = 0;
            }
            await using var fileStream = _fileName.OpenFileForWrite(SeekOrigin.End);
            var result = await Download.FileAsync(_sourceUri, fileStream, _cancellationToken.Token, _progressObserver, _initialBytePosition);
            if (result.IsSuccess) 
                Fire(Trigger.End);
            else if (CurrentState == State.Paused) {
                _initialBytePosition = result.BytesReceived;
                _progressObserver.OnProgress("Paused");
            }
        }
        catch (Exception ex) {
            Error = ex.Message;
            Reset();
        }
        finally {
            if (CurrentState != State.Paused)
                Reset();
            _cancellationToken = new CancellationTokenSource();
            StartCommand.Refresh();
        }
    }

    private void
    Fire(Trigger trigger) {
        CurrentState = TransitionTo(CurrentState, trigger);

        State
        TransitionTo(State state, Trigger value) =>
            (state, trigger: value) switch {
                (State.Idle, Trigger.Start) => ((Func<State>) (() => {
                    DownloadExecute();
                    return Error is null ? State.Running : State.Idle;
                }))(),
                (State.Idle, Trigger.Cancel) => ((Func<State>) (() => {
                    Reset();
                    return State.Idle;
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
                    return State.Idle;
                }))(),
                (State.Paused, Trigger.Cancel) => ((Func<State>) (() => {
                    Reset();
                    return State.Idle;
                }))(),
                (State.Paused, Trigger.Start) => ((Func<State>) (() => {
                    DownloadExecute();
                    return Error is null ? State.Running : State.Idle;
                }))(),
                _ => throw new NotSupportedException($"{CurrentState} has no transition on {value}")
        };
    }

    private void
    Reset() {
        _progressObserver.Reset();
        _initialBytePosition = 0;
    }

    public void
    Dispose() {
        _cancellationToken.Dispose();
        DownloadProgress.Dispose();
    }
}
}
