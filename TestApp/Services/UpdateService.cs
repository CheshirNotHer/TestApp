using System;
using System.Threading.Tasks;
using TomLabs.AutoUpdate;

namespace TestApp.Services;

public class UpdateService
{
    private const string AppName = "TestApp";
    private const string GitHubOwner = "CheshirNotHer";
    private const string GitHubRepo = "TestApp";

    public bool IsUpdateAvailable => Updater.Current?.Available != null;
    public string? AvailableVersion => Updater.Current?.Available?.Version.ToString();

    public async Task CheckForUpdatesDailyAsync()
    {
        if (App.Settings.LastUpdateCheck.HasValue &&
            (DateTime.Now - App.Settings.LastUpdateCheck.Value).TotalHours < 24)
        {
            return;
        }

        try
        {
            // Запускаем Updater (создаёт синглтон и запускает фоновую проверку)
            var updater = Updater.Start(new UpdateOptions(AppName, new GitHubReleasesSource(GitHubOwner, GitHubRepo))
            {
                ExitApplication = () => App.Current.Dispatcher.Invoke(() => App.Current.Shutdown()),
                Log = message => System.Diagnostics.Debug.WriteLine($"[Updater] {message}")
            });

            // Явно дожидаемся завершения проверки
            await updater.CheckAsync();

            // Сохраняем статус
            App.Settings.LastUpdateCheck = DateTime.Now;
            App.Settings.IsUpdateAvailable = IsUpdateAvailable;
            App.Settings.AvailableVersion = AvailableVersion;
            App.SettingsService.Save(App.Settings);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка проверки обновлений: {ex.Message}");
        }
    }

    public async Task PerformUpdateAsync()
    {
        if (!IsUpdateAvailable) return;

        try
        {
            var updater = Updater.Current;
            if (updater == null) return;

            // 1. Скачиваем и проверяем обновление
            await updater.DownloadAsync();

            // 2. Применяем и перезапускаем приложение
            updater.ApplyAndRestart();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка обновления: {ex.Message}");
        }
    }

    public void MarkHealthy()
    {
        Updater.Current?.MarkHealthy();
    }
}