using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace TestApp.Services;

public class UpdateService
{
    private const string GitHubOwner = "CheshirNotHer";
    private const string GitHubRepo = "TestApp";

    private string _downloadUrl = string.Empty;
    public bool IsUpdateAvailable { get; private set; }
    public string? AvailableVersion { get; private set; }

    public async Task CheckForUpdatesDailyAsync()
    {
        // Проверка раз в 24 часа
        if (App.Settings.LastUpdateCheck.HasValue &&
            (DateTime.Now - App.Settings.LastUpdateCheck.Value).TotalHours < 24)
        {
            IsUpdateAvailable = App.Settings.IsUpdateAvailable;
            AvailableVersion = App.Settings.AvailableVersion;
            return;
        }

        try
        {
            using var client = new HttpClient();
            // GitHub API требует User-Agent
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TestApp", "1.0"));

            var url = $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/releases/latest";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode) return;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Получаем версию из тега (например, "v0.9.2")
            string tag = root.GetProperty("tag_name").GetString() ?? "";
            string cleanVersion = tag.TrimStart('v', 'V');

            // Получаем текущую версию приложения
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var currentVersionStr = $"{currentVersion.Major}.{currentVersion.Minor}.{currentVersion.Build}";

            // Сравниваем версии
            if (Version.TryParse(cleanVersion, out var remoteVer) &&
                Version.TryParse(currentVersionStr, out var localVer) &&
                remoteVer > localVer)
            {
                // Ищем ZIP-архив в релизе
                foreach (var asset in root.GetProperty("assets").EnumerateArray())
                {
                    string name = asset.GetProperty("name").GetString() ?? "";
                    if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        _downloadUrl = asset.GetProperty("browser_download_url").GetString() ?? "";
                        IsUpdateAvailable = true;
                        AvailableVersion = cleanVersion;
                        break;
                    }
                }
            }

            App.Settings.LastUpdateCheck = DateTime.Now;
            App.Settings.IsUpdateAvailable = IsUpdateAvailable;
            App.Settings.AvailableVersion = AvailableVersion;
            App.SettingsService.Save(App.Settings);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Update Check Error] {ex.Message}");
        }
    }
    private bool HasWriteAccessToAppDirectory()
    {
        try
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string testFile = Path.Combine(appDir, "write_test.tmp");

            // Пытаемся создать и удалить тестовый файл
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task PerformUpdateAsync()
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

            // 1. Скачиваем ZIP
            using (var client = new HttpClient())
            {
                var bytes = await client.GetByteArrayAsync(_downloadUrl);
                await File.WriteAllBytesAsync(zipPath, bytes);
            }

            // 2. Очищаем папку и распаковываем
            if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
            ZipFile.ExtractToDirectory(zipPath, extractPath);

            // 3. Создаем bat-скрипт для замены файлов
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string batPath = Path.Combine(tempPath, "update.bat");

            string batContent = $@"
@echo off
timeout /t 2 /nobreak > NUL
echo Обновление файлов...
xcopy /s /y /q ""{extractPath}\*.*"" ""{appDir}""
echo Запуск приложения...
start """" ""{Path.Combine(appDir, "TestApp.exe")}""
del ""%~f0""
";
            await File.WriteAllTextAsync(batPath, batContent);

            // 4. Запускаем bat-скрипт и закрываем текущее приложение
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