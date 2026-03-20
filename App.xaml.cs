using System.Windows.Threading;
using System.IO.Pipes;
using System.Windows;
using System.IO;

namespace CrosshairY
{
    public partial class App : System.Windows.Application
    {
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string RegistryValueName = "AppsUseLightTheme";

        private const string MutexName = @"Global\CrosshairY_Instance";
        internal const string PipeName = "CrosshairY_ActivationPipe";

        private Mutex? _instanceMutex;

        public App()
        {
            // Handle UI thread exceptions
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            // Handle background thread exceptions
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            bool ignoreMutexRule = e.Args.Any(a => a.Equals("--updating", StringComparison.OrdinalIgnoreCase) || a.Equals("--updated", StringComparison.OrdinalIgnoreCase));

            _instanceMutex = new Mutex(true, MutexName, out bool createdNew);

            if (!createdNew && !ignoreMutexRule)
            {
                // Notify existing instance
                TryActivateExistingInstance();
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            System.Windows.MessageBox.Show($"An undefined error has happened, please contact tsgsOFFICIAL to resolve this issue.\n\nInclude the following Error Message: {e.Exception.Message}", "Undefined Error", MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true; // Prevents the application from crashing
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                System.Windows.MessageBox.Show($"A critical error has happened, please contact tsgsOFFICIAL to resolve this issue.\n\nInclude the following Error Message: {ex.Message}", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void TryActivateExistingInstance()
        {
            try
            {
                using NamedPipeClientStream client = new NamedPipeClientStream(
                    ".",
                    PipeName,
                    PipeDirection.Out);

                client.Connect(500);
                using StreamWriter writer = new StreamWriter(client);
                writer.WriteLine("ACTIVATE");
                writer.Flush();
            }
            catch
            {
                // Existing instance not ready yet - safe to ignore
            }
        }
    }
}