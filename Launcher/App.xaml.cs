using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using Serilog;
using Serilog.Events;
using WpfApp.ViewModels;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private Process? myTodoListsAppProcess;
        private Process? myPostgresProcess;
        
        protected override void OnStartup(StartupEventArgs e)
        { 
            base.OnStartup(e);
            
            try
            {
                AttachToParentConsole();
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.File("TodoLists.Launcher.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
                    .WriteTo.Console()
                    .CreateLogger();
                Log.Information("Start");

                AppDomain.CurrentDomain.UnhandledException += (_, args) =>
                {
                    var logLevel = args.IsTerminating ? LogEventLevel.Fatal : LogEventLevel.Error;
                    if (args.ExceptionObject is Exception e)
                    {
                        Log.Write(logLevel, e, "Unhandled exception");
                    }
                    else
                    {
                        Log.Write(logLevel, args.ExceptionObject.ToString(), "Unhandled exception (object).");
                    }
                };
                DispatcherUnhandledException += (_, args) =>
                {
                    Log.Error(args.Exception, "Unhandled exception");
                    args.Handled = true;
                };

                var wpfAppDirPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;

                var environmentIsDevelopment = Environment.GetEnvironmentVariable("Environment") == "Development";
                
                string webAppWorkingDir;
                string webAppExePath;
                if (environmentIsDevelopment)
                {
                    webAppWorkingDir = Path.Combine(wpfAppDirPath, "../../../../App");
                    webAppExePath = Path.Combine(webAppWorkingDir, "bin/Debug/net7.0/TodoLists.App.exe");
                }
                else
                {
                    webAppWorkingDir = Path.Combine(wpfAppDirPath, "../App");
                    webAppExePath = Path.Combine(webAppWorkingDir, "TodoLists.App.exe");
                }

                myTodoListsAppProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = webAppExePath,
                    Arguments = "",
                    WorkingDirectory = webAppWorkingDir,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                });
                
                string postgresWorkingDir;
                string postgresDataDir;
                if (environmentIsDevelopment)
                {
                    postgresWorkingDir = Path.Combine(wpfAppDirPath, "../../../../pgsql/bin");
                    postgresDataDir = Path.Combine(wpfAppDirPath, "../../../../data");
                }
                else
                {
                    postgresWorkingDir = Path.Combine(wpfAppDirPath, "../pgsql/bin");
                    postgresDataDir = Path.Combine(wpfAppDirPath, "../data");
                }

                if (!Directory.Exists(postgresDataDir))
                {
                    Directory.CreateDirectory(postgresDataDir);
                    var passFilePath = Path.Combine(postgresWorkingDir, "../pass.txt");
                    var initDbProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = Path.Combine(postgresWorkingDir, "initdb.exe"),
                        Arguments = $"-D {postgresDataDir} -U postgres --no-locale -E UTF8 -A scram-sha-256 --pwfile={passFilePath}",
                        WorkingDirectory = postgresWorkingDir,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardInput = true,
                        CreateNoWindow = true,
                    });
                    initDbProcess!.WaitForExit();
                    if (initDbProcess.ExitCode != 0)
                    {
                        throw new Exception("Assertion failed: initDbProcess.ExitCode != 0");
                    }
                }

                var pgCtlArguments = $"-o \"-p 41577\" -D {postgresDataDir} -l {Path.Combine(postgresWorkingDir, "../postgres.log")} start";
                myPostgresProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = Path.Combine(postgresWorkingDir, "pg_ctl.exe"),
                    Arguments = pgCtlArguments,
                    WorkingDirectory = postgresWorkingDir,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                });
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Failed to init the application");
                Shutdown(-1);
            }
            base.OnStartup(e);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            MainWindow!.DataContext = new MainWindowViewModel(myTodoListsAppProcess!, myPostgresProcess!);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            myTodoListsAppProcess?.Kill();
            myPostgresProcess?.Kill();

            if (e.ApplicationExitCode == 0)
                Log.Information("Exited gracefully");
            Log.CloseAndFlush();
            
            base.OnExit(e);
        }

        #region Rider Console support

        private const int ATTACH_PARENT_PROCESS = -1;

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        /// <summary>
        ///     Redirects the console output of the current process to the parent process.
        /// </summary>
        /// <remarks>
        ///     Must be called before calls to <see cref="Console.WriteLine()" />.
        /// </remarks>
        public static void AttachToParentConsole()
        {
            AttachConsole(ATTACH_PARENT_PROCESS);
        }

        #endregion
    }
}