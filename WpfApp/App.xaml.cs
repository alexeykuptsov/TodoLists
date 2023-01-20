using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using WpfApp.ViewModels;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private Process? myAppProcess;
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var wpfAppDirPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
            var webAppWorkingDir = Path.Combine(wpfAppDirPath, "../../../../App");
            var webAppExePath = Path.Combine(webAppWorkingDir, "bin/Debug/net7.0/TodoLists.App.exe");

            myAppProcess = Process.Start(new ProcessStartInfo
            {
                FileName = webAppExePath,
                Arguments = "",
                WorkingDirectory = webAppWorkingDir,
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