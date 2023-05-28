using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApp.Threading.Tasks;

namespace WpfApp.Windows.Input;

[ExcludeFromCodeCoverage]
public class AlwaysCanExecuteCommand : ICommand
{
    public AlwaysCanExecuteCommand(Action execute)
    {
        myExecute = _ => execute();
    }

    public AlwaysCanExecuteCommand(Action<object?> execute)
    {
        myExecute = execute;
    }

    public AlwaysCanExecuteCommand(Func<Task> execute)
    {
        myExecute = _ =>
        {
            execute().FireAndForgetSafeAsync();
        };
    }

    public AlwaysCanExecuteCommand(Func<object?, Task> execute)
    {
        myExecute = parameter =>
        {
            execute(parameter).FireAndForgetSafeAsync();
        };
    }

#pragma warning disable 67
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        myExecute.Invoke(parameter);
    }

    private readonly Action<object?> myExecute;
}