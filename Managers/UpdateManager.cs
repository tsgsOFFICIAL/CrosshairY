using System.Net.Http.Headers;
using System.Diagnostics;
using CrosshairY.Utility;
using CrosshairY.Models;
using System.Text.Json;
using System.Net.Http;
using System.IO;

namespace CrosshairY.Managers
{
    public sealed class UpdateManager
    {
        private static readonly Lazy<UpdateManager> _instance = new(() => new UpdateManager());
        public static UpdateManager Instance => _instance.Value;

        private readonly string _repositoryOwner = "tsgsOFFICIAL";
        private readonly string _repositoryName = "CrosshairY";
        private readonly string _folderPath = "UI/bin/Release/net10.0-windows/publish/win-x64";

        public event EventHandler<ProgressEventArgs>? DownloadProgress;
        public event EventHandler? UpdateAvailable;

        private DateTime? _lastUpdateCheck = null;
        private const int UpdateCheckIntervalHours = 12;

        // Private constructor to prevent instantiation from outside the class
        private UpdateManager()
        {
            // Create a background task to check for updates periodically
            Task.Run(async () =>
            {
                // Wait a few seconds after launch so we don't compete for resources during startup.
                await Task.Delay(TimeSpan.FromSeconds(5));

                while (true)
                {
                    try
                    {
                        if (ShouldCheckForUpdates() && !App.Settings.App.UpdateAvailable)
                        {
                            bool isAvailable = await IsUpdateAvailableAsync();
                            if (isAvailable)
                            {
                                if (App.Settings.App.AutoUpdate)
                                    await DownloadUpdate();
                                else
                                {
                                    App.Settings.App.UpdateAvailable = true;
                                    UpdateAvailable?.Invoke(this, EventArgs.Empty);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log background check errors here if necessary
                        Debug.WriteLine($"Background update check failed: {ex.Message}");
                    }

                    // Check again every hour to see if the hours have passed since the last check
                    await Task.Delay(TimeSpan.FromHours(1));
                }
            });
        }

        public void Initialize()
        { }

        private bool ShouldCheckForUpdates()
        {
            if (_lastUpdateCheck == null)
                return true;
            return (DateTime.Now - _lastUpdateCheck.Value).TotalHours >= UpdateCheckIntervalHours;
        }

        public async Task<bool> IsUpdateAvailableAsync()
        {
            bool updateAvailable;
            FileVersionInfo localVersionInfo = FileVersionInfo.GetVersionInfo(Helper.GetExePath());
            UpdateInfo? serverUpdateInfo;

            try
            {
                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true
                };

                client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

                serverUpdateInfo = JsonSerializer.Deserialize<UpdateInfo>(await client.GetStringAsync("https://raw.githubusercontent.com/tsgsOFFICIAL/CrosshairY/feature/new-ui/UpdateInfo.json")) ?? new UpdateInfo();
            }
            catch (Exception)
            {
                return false;
            }

            if (Version.TryParse(serverUpdateInfo.Version, out Version? serverVersion) && Version.TryParse(localVersionInfo.FileVersion, out Version? localVersion))
                updateAvailable = serverVersion > localVersion;
            else
                updateAvailable = false;

            _lastUpdateCheck = DateTime.Now;

            return updateAvailable;
        }

        /// <summary>
        /// Downloads the latest update for the application from the configured GitHub repository and restarts the
        /// application to apply the update.
        /// </summary>
        /// <remarks>This method initiates an asynchronous download of the update files and then restarts
        /// the application upon successful completion. Any errors encountered during the update process are logged for
        /// debugging purposes. This method should typically be called from the UI thread, as it may cause the
        /// application to exit and restart.</remarks>
        public async Task DownloadUpdate()
        {
            string basePath = Path.Combine(Environment.ExpandEnvironmentVariables("%APPDATA%"), "CrosshairY");
            string updatePath = Path.Combine(basePath, "Update");

            using GitHubDirectoryDownloaderService downloader = new GitHubDirectoryDownloaderService(_repositoryOwner, _repositoryName, _folderPath, basePath);
            downloader.ProgressUpdated += OnProgressChanged!;

            try
            {
                await downloader.DownloadDirectoryAsync(updatePath);

                Process.Start(Path.Combine(updatePath, "CrosshairY"), "--updating");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                NotificationManager.ShowNotification("Update Error", $"An error occurred while updating the application.\n{ex.Message}\n\nTry again after this time", 300);
            }
        }
        /// <summary>
        /// Raises the event that reports progress updates during an operation.
        /// </summary>
        /// <param name="sender">The source of the event. Typically, this is the object that initiated the progress update.</param>
        /// <param name="e">A ProgressEventArgs object that contains the progress data, such as the percentage completed. Must not be
        /// null.</param>
        private void OnProgressChanged(object sender, ProgressEventArgs e)
        {
            DownloadProgress?.Invoke(this, e);
        }
    }
}