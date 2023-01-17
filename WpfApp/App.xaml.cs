using System;
using System.Diagnostics;
using System.Windows;
using WpfApp.ViewModels;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Process? myAppProcess;
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            myAppProcess = Process.Start(new ProcessStartInfo
            {
                FileName = @"C:\Code\ak\github\TodoLists\App\bin\Debug\net7.0\TodoLists.App.exe",
                Arguments = "",
                WorkingDirectory = @"C:\Code\ak\github\TodoLists\App\bin\Debug\net7.0\",
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardInput = true,
                CreateNoWindow = true,
            });
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            MainWindow.DataContext = new MainWindowViewModel(myAppProcess!);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (myAppProcess != null)
            {
                myAppProcess.Kill();
            }
            
            base.OnExit(e);
        }
    }
}