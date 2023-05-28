using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace WpfApp.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private string myTodoListsAppStatusText;

    public MainWindowViewModel(Process todoListsAppProcess)
    {
        todoListsAppProcess.Exited += (_, _) =>
        {
            TodoListsAppStatusText = "TodoLists.App не работает";
        };
        myTodoListsAppStatusText = todoListsAppProcess.HasExited ? "TodoLists.App не работает" : "TodoLists.App OK";
    }

    public string TodoListsAppStatusText
    {
        get => myTodoListsAppStatusText;
        set => SetField(ref myTodoListsAppStatusText, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}