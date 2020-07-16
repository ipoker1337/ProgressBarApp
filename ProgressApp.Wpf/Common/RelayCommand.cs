﻿using System;
using System.Windows.Input;

namespace ProgressApp.Wpf.Common {
public class 
RelayCommand<T> : ICommand {
    private readonly Action<T> _execute;
    private readonly Func<T, bool>? _canExecute;

    public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null) {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public void 
    Execute(object parameter) => _execute((T)parameter);

    public bool 
    CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? true;

    public void 
    Refresh() => CommandManager.InvalidateRequerySuggested();

    public event EventHandler CanExecuteChanged {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}

public class 
RelayCommand : ICommand {
    private readonly Action? _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action? execute = null, Func<bool>? canExecute = null) {
        _execute = execute;
        _canExecute = canExecute;
    }

    public void 
    Execute(object parameter) => _execute?.Invoke();

    public bool 
    CanExecute(object? parameter = null) => _canExecute?.Invoke() ?? true;

    public void 
    Refresh() => CommandManager.InvalidateRequerySuggested();

    public event EventHandler CanExecuteChanged {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
}
}
