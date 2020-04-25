using System;
using System.IO;
using System.Threading;
using ProgressApp.Core;
using WpfApp2.Common;

namespace WpfApp2 {

public class 
MainViewModel : ViewModel, IDisposable {
    private readonly Uri _sourceUri = new Uri(@"http://87.76.21.20/test.zip");
    private readonly ProgressObserver _progressObserver;
    private readonly string _fileName;

    private CancellationTokenSource _cancellationToken;
    private long _initialBytePosition;
    private string? _error;

    public MainViewModel() {
        _fileName = Path.GetFileName(_sourceUri.LocalPath);
        _progressObserver = new ProgressObserver();
        _cancellationToken = new CancellationTokenSource();
        DownloadProgress = new ProgressViewModel(_progressObserver);
        DownloadState = State.Idle;
    }

    private enum State { Idle, Running, Paused }
    private enum Trigger { Start, Pause, Cancel, End }

    public ProgressViewModel DownloadProgress { get; }
    public string? Error { get => _error; private set => SetPropertyIfChanged(ref _error, value); }

    public RelayCommand StartCommand => new RelayCommand(() => Fire(Trigger.Start), () => DownloadState == State.Idle);
    public RelayCommand PauseCommand => new RelayCommand(() => Fire(Trigger.Pause), () =>  DownloadState == State.Running);
    public RelayCommand ResumeCommand => new RelayCommand(() => Fire(Trigger.Start), () => DownloadState == State.Paused);
    public RelayCommand CancelCommand => new RelayCommand(() => Fire(Trigger.Cancel), () => DownloadState != State.Idle);

    private State DownloadState { get; set; }

    private async void 
    DownloadExecute() {
        try {
            await using var fileStream = File.Open(_fileName, FileMode.Append);
            var result = await Download.FileAsync(_sourceUri, fileStream, _cancellationToken.Token, _progressObserver, _initialBytePosition);
            if (result.IsSuccess) {
                Fire(Trigger.End);
            }
            if (DownloadState == State.Paused) {
                _initialBytePosition = result.BytesReceived;
                _progressObserver.OnProgress("Paused");
            }
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
            StartCommand.Refresh();
        }
    }

    private void
    Reset() {
        _progressObserver.Reset();
        _initialBytePosition = 0;
    }

    private void
    Fire(Trigger trigger) {
        DownloadState = TransitionTo(DownloadState, trigger);

        State
        TransitionTo(State state, Trigger value) =>
            (state, trigger: value) switch {
                (State.Idle, Trigger.Start) => ((Func<State>) (() => {
                    Error = null;
                    if (_initialBytePosition == 0)
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
                _ => throw new NotSupportedException($"{DownloadState} has no transition on {value}")
            };
    }

    public void
    Dispose() {
        _cancellationToken.Dispose();
        DownloadProgress.Dispose();
    }
}
}
