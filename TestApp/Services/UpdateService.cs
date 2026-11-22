using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace TestApp.Services;

public class UpdateProgress
{
    public string Stage { get; init; } = "";
    public int Percent { get; init; }   // 0–100
    public bool IsIndeterminate { get; init; }
}

public class UpdateService
{
    private const string GitHubOwner = "CheshirNotHer";
    private const string GitHubRepo = "TestApp";

    private string _downloadUrl = string.Empty;
    public bool IsUpdateAvailable { get; private set; }
    public string? AvailableVersion { get; private set; }

    /// <summary>Событие: статус обновления изменился (баннер на главном окне).</summary>
    public event Action? UpdateStatusChanged;

    public async Task CheckForUpdatesDailyAsync()
    {
        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        var currentVersionStr = $"{currentVersion!.Major}.{currentVersion.Minor}.{currentVersion.Build}";

        // Если в настройках записано обновление, но текущая версия программы уже >= сохраненной — сбрасываем
        if (App.Settings.IsUpdateAvailable &&
            Version.TryParse(App.Settings.AvailableVersion, out var cachedVer) &&
            Version.TryParse(currentVersionStr, out var localVerCheck) &&
            localVerCheck >= cachedVer)
        {
            App.Settings.IsUpdateAvailable = false;
            App.Settings.AvailableVersion = null;
            App.SettingsService.Save(App.Settings);
        }

        // Если ранее уже находили обновление — сразу показываем баннер, пока идёт проверка в фоне
        if (App.Settings.IsUpdateAvailable && !string.IsNullOrEmpty(App.Settings.AvailableVersion))
        {
            IsUpdateAvailable = true;
            AvailableVersion = App.Settings.AvailableVersion;
            string cachedTag = $"v{App.Settings.AvailableVersion}";
            _downloadUrl = $"https://github.com/{GitHubOwner}/{GitHubRepo}/releases/download/{cachedTag}/TestApp-win-{cachedTag}.zip";
            UpdateStatusChanged?.Invoke();
        }

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TestApp", "1.0"));

            var response = await client.GetAsync(
                $"https://github.com/{GitHubOwner}/{GitHubRepo}/releases/latest",
                HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode) return;

            // Итоговый URL после редиректа: https://github.com/CheshirNotHer/TestApp/releases/tag/v0.9.3
            string finalUrl = response.RequestMessage?.RequestUri?.ToString() ?? "";
            string tag = finalUrl.Split('/')[^1]; // например, "v0.9.3"
            string cleanVersion = tag.TrimStart('v', 'V');

            if (Version.TryParse(cleanVersion, out var remoteVer) &&
                Version.TryParse(currentVersionStr, out var localVer) &&
                remoteVer > localVer)
            {
                // Подставляем динамический tag в имя архива: TestApp-win-x64-v0.9.3.zip
                _downloadUrl = $"https://github.com/{GitHubOwner}/{GitHubRepo}/releases/download/{tag}/TestApp-win-{tag}.zip";
                IsUpdateAvailable = true;
                AvailableVersion = cleanVersion;
            }
            else
            {
                IsUpdateAvailable = false;
                AvailableVersion = null;
                _downloadUrl = string.Empty;
            }

            App.Settings.LastUpdateCheck = DateTime.Now;
            App.Settings.IsUpdateAvailable = IsUpdateAvailable;
            App.Settings.AvailableVersion = AvailableVersion;
            App.SettingsService.Save(App.Settings);

            UpdateStatusChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Update Check Error] {ex.Message}");
        }
    }


    private static bool HasWriteAccessToAppDirectory()
    {
        try
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string testFile = Path.Combine(appDir, "write_test.tmp");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return true;
        }
        catch { return false; }
    }

    public async Task PerformUpdateAsync(IProgress<UpdateProgress>? progress = null)
    {
        if (!IsUpdateAvailable || string.IsNullOrEmpty(_downloadUrl)) return;

        if (!HasWriteAccessToAppDirectory())
        {
            System.Windows.MessageBox.Show(
                "Невозможно обновить приложение, так как папка с программой защищена от записи.\n\n" +
                "Пожалуйста, переместите папку с TestApp в другое место (например, на Рабочий стол, Загрузки) и попробуйте снова.",
                "Ошибка доступа",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }

        try
        {
            string tempPath = Path.GetTempPath();
            string zipPath = Path.Combine(tempPath, "TestApp_Update.zip");
            string extractPath = Path.Combine(tempPath, "TestApp_Extracted");

            // 1. Скачивание с прогрессом
            progress?.Report(new UpdateProgress { Stage = "Скачивание обновления…", Percent = 0 });

            using (var client = new HttpClient())
            using (var response = await client.GetAsync(_downloadUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                long? total = response.Content.Headers.ContentLength;
                long received = 0;

                await using var stream = await response.Content.ReadAsStreamAsync();
                await using var file = File.Create(zipPath);

                var buffer = new byte[81920];
                int read;
                while ((read = await stream.ReadAsync(buffer)) > 0)
                {
                    await file.WriteAsync(buffer.AsMemory(0, read));
                    received += read;
                    int percent = total.HasValue && total.Value > 0
                        ? (int)(received * 100 / total.Value)
                        : 0;
                    progress?.Report(new UpdateProgress { Stage = "Скачивание обновления…", Percent = percent });
                }
            }

            // 2. Распаковка
            progress?.Report(new UpdateProgress { Stage = "Распаковка…", IsIndeterminate = true });
            if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
            ZipFile.ExtractToDirectory(zipPath, extractPath);

            // 3. bat-скрипт
            progress?.Report(new UpdateProgress { Stage = "Подготовка к установке…", IsIndeterminate = true });

            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string exePath = Process.GetCurrentProcess().MainModule?.FileName
                             ?? Path.Combine(appDir, "TestApp.exe");
            string batPath = Path.Combine(tempPath, "update.bat");

            string batContent = $@"
@echo off
timeout /t 2 /nobreak > NUL
echo Обновление файлов...
xcopy /s /y /q ""{extractPath}\*.*"" ""{appDir}""
echo Запуск приложения...
start """" ""{exePath}""
del ""{zipPath}""
rmdir /s /q ""{extractPath}""
del ""%~f0""
";
            await File.WriteAllTextAsync(batPath, batContent);

            // 4. Сброс флага обновления, запуск скрипта и выход
            App.Settings.IsUpdateAvailable = false;
            App.Settings.AvailableVersion = null;
            App.Settings.LastUpdateCheck = DateTime.Now;
            App.SettingsService.Save(App.Settings);

            progress?.Report(new UpdateProgress { Stage = "Установка…", Percent = 100 });
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"\"{batPath}\"\"",
                CreateNoWindow = true,
                UseShellExecute = false
            });

            App.Current.Shutdown();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Ошибка при обновлении:\n{ex.Message}", "Ошибка",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
}
