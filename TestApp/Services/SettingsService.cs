using System;
using System.IO;
using System.Text.Json;
using TestApp.Models;

namespace TestApp.Services;

public class SettingsService
{
    private readonly string _path =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_path)) return new AppSettings();
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch { return new AppSettings(); }
    }

    public void Save(AppSettings settings)
    {
        try
        {
            File.WriteAllText(_path,
                JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { /* ignore */ }
    }
}