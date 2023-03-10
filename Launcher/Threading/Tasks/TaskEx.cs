using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using EasyExceptions;

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void

namespace WpfApp.Threading.Tasks;

public static class TaskEx
{
    [ExcludeFromCodeCoverage]
    public static async void FireAndForgetSafeAsync(this Task task, Action<Exception>? handler = null)
    {
        try
        {
            await task;
        }
        catch (Exception ex)
        {
            if (handler != null)
            {
                handler.Invoke(ex);
            }
            else
            {
                MessageBox.Show(ExceptionDumpUtil.Dump(ex));
            }
        }
    }
        
    [ExcludeFromCodeCoverage]
    public static async void FireAndDoContinuationSafeAsync<T>(this Task<T> task, Action<AsyncCompletedEventArgs> continuationAction)
    {
        try
        {
            var taskResult = await task;
            continuationAction(new AsyncCompletedEventArgs(null, false, taskResult));
        }
        catch (OperationCanceledException)
        {
            continuationAction(new AsyncCompletedEventArgs(null, true, null));
        }
        catch (Exception ex)
        {
            continuationAction(new AsyncCompletedEventArgs(ex, false, null));
        }
    }
}