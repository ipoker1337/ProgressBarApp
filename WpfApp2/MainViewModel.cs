﻿using System;
using System.IO;
using System.Threading;
using ProgressApp.Core;
using WpfApp2.Common;

namespace WpfApp2 {

// under development
public class
StateMachine<TState, TTrigger> {

    public class 
    Transition {
        private Action? _action;
        public TState Original { get; }
        public TState Destination { get; }
        public TTrigger Trigger { get; }

        public Transition(TState original, TState destination, TTrigger trigger, Action? action = null) =>
            (Original, Destination, Trigger, _action) = (original, destination, trigger, action);
    }

}

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

    public MainViewModel() {
        DownloadProgress = new ProgressViewModel(_progressHandler);
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
        //Andrew:
        //using var fileStream = File.Open(_fileName);
        //fileStream.Seek(0, SeekOrigin.End);
        //Файл будет создан, если не существует. Обычно вот так ссылку на стрим не хранят, как у тебя.
        //Стримы сразу Dispose'ят после завершения операции
        _fileStream = (_fileStream == Stream.Null) ? File.Create(_fileName) : _fileStream;
        var result = await Download.FileAsync(new Uri(@"http://87.76.21.20/test.zip"), _fileStream, _cancellationToken.Token, 
                                              _progressHandler, _lastBytePosition);
        //Andrew: Красиво, но так не делают :), т.к. слишком сложно. Просто трай кетч ставят и все. В Download.FileAsync
        //не должно быть обработок ошибок. try/catch ставят обычно как можно выше по иерархии классов
        result.OnSuccess(() => Fire(Trigger.End))
              .OnFailure(HandleException, x => x.Exception != null)
              .OnFailure(Pause, _ => CurrentState == State.Paused)
              .OnBoth(_ => { _cancellationToken = new CancellationTokenSource(); Command.Refresh(); });

        void
        HandleException(Result value) {
            Error = value.Exception?.Message;
            Fire(Trigger.Cancel);
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
        _progressHandler.Reset();
        DownloadProgress.Progress = null;
        _lastBytePosition = 0;
        CommandText = "Start";
        Command = StartCommand;
        Command.Refresh();
    }

    //Andrew: тут слишком сложно получилось.
    //Текст кнопок это почти всегда уходит в XAML. 
    //Если текст кнопок у тебя во ViewModel значит ты что-то делаешь не так.
    //В этой задаче нужно просто создать 3 разных кнопок в XAML: Start, Pause, Resume.
    //Каждая кнопка должна будет иметь Visibility==Collapsed если CanExecute == false
    private void
    Fire(Trigger trigger) {
        CurrentState = TransitionTo(CurrentState, trigger);

        State
        TransitionTo(State state, Trigger value) =>
        (state: state, trigger: value) switch {
        (State.Idle, Trigger.Start) => ((Func<State>) (() => {
            Error = null;
            CommandText = "Pause";
            Command = PauseCommand;
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
//
    public void
    Dispose() {
        _cancellationToken.Dispose();
        DownloadProgress.Dispose();
    }
}
}
