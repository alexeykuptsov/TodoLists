using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace WpfApp.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly Process myPostgresProcess;
    private string myTodoListsAppStatusText;
    private string myPostgresStatusText;

    public MainWindowViewModel(Process todoListsAppProcess, Process postgresProcess)
    {
        myPostgresProcess = postgresProcess;
        todoListsAppProcess.Exited += (_, _) =>
        {
            TodoListsAppStatusText = "TodoLists.App не работает";
        };
        myTodoListsAppStatusText = todoListsAppProcess.HasExited ? "TodoLists.App не работает" : "TodoLists.App OK";

        postgresProcess.Exited += (_, _) =>
        {
            UpdatePostgresStatusText();
        };
        UpdatePostgresStatusText();
    }

    private void UpdatePostgresStatusText()
    {
        if (!myPostgresProcess.HasExited)
        {
            PostgresStatusText = "PostgreSQL OK";
            return;
        }

        StringBuilder resultBuilder = new StringBuilder();
        resultBuilder.AppendLine("PostgreSQL не работает");
        try
        {
            resultBuilder.AppendLine("stderr: " + myPostgresProcess.StandardError.ReadToEnd());
        }
        catch (InvalidOperationException e)
        {
            resultBuilder.AppendLine("stderr: " + e.Message);
        }
        try
        {
            resultBuilder.AppendLine("stdout: " + myPostgresProcess.StandardOutput.ReadToEnd());
        }
        catch (InvalidOperationException e)
        {
            resultBuilder.AppendLine("stdout: " + e.Message);
        }
        PostgresStatusText = resultBuilder.ToString();
    }

    public string PostgresStatusText
    {
        get => myPostgresStatusText;
        set => SetField(ref myPostgresStatusText, value);
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