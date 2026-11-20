using System;
using System.Threading.Tasks;
using TomLabs.AutoUpdate;

namespace TestApp.Services;

public class UpdateService
{
    private const string AppName = "TestApp";
    private const string GitHubOwner = "CheshirNotHer"; 
    private const string GitHubRepo = "TestApp";      

    private Updater? _updater;

    public bool IsUpdateAvailable => _updater?.Available != null;
    public string? AvailableVersion => _updater?.Available?.Version.ToString();

    public async Task CheckForUpdatesDailyAsync()
    {
        try
        {
            
            _updater = Updater.Start(new UpdateOptions(AppName, new GitHubReleasesSource(GitHubOwner, GitHubRepo))
            {
                ExitApplication = () => App.Current.Dispatcher.Invoke(() => App.Current.Shutdown()),
                Log = message => System.Diagnostics.Debug.WriteLine($"[Updater] {message}")
            });

           
            await _updater.CheckAsync();

            App.Settings.LastUpdateCheck = DateTime.Now;
            App.Settings.IsUpdateAvailable = IsUpdateAvailable;
            App.Settings.AvailableVersion = AvailableVersion;
            App.SettingsService.Save(App.Settings);

            System.Diagnostics.Debug.WriteLine($"[Check] Update available: {IsUpdateAvailable}, Version: {AvailableVersion}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Check Error] {ex}");
        }
    }

    public async Task PerformUpdateAsync()
    {
        if (!IsUpdateAvailable || _updater == null)
        {
            System.Diagnostics.Debug.WriteLine("[Update] No update available or updater is null");
            return;
        }

        try
        {
            System.Diagnostics.Debug.WriteLine("[Update] Starting download...");
            await _updater.DownloadAsync();
            System.Diagnostics.Debug.WriteLine("[Update] Download completed, applying...");

            _updater.ApplyAndRestart();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Update Error] {ex}");
            App.Current.Dispatcher.Invoke(() =>
                System.Windows.MessageBox.Show($"Ошибка обновления: {ex.Message}", "Ошибка",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error));
        }
    }

    public void MarkHealthy()
    {
        _updater?.MarkHealthy();
    }
}