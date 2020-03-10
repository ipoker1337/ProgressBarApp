﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfApp2.ViewModels {

public class ViewModelBase : INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void 
    OnPropertyChanged([CallerMemberName] string propertyName = "") {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool 
    SetPropertyIfChanged<T>(ref T field, T value, [CallerMemberName] string propertyName = "") {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);

        return true;
    }
}
}
