using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using TestApp.Models;
using TestApp.Services;

namespace TestApp;

public partial class App : Application
{
    public static QuestionService QuestionService { get; private set; } = null!;
    public static SettingsService SettingsService { get; private set; } = null!;
    public static AppSettings Settings { get; set; } = null!;
    public static TestGenerator TestGenerator { get; private set; } = null!;
    public static UpdateService UpdateService { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        SettingsService = new SettingsService();
        Settings = SettingsService.Load();

        ApplyTheme(Settings.Theme);

        QuestionService = new QuestionService();
        try
        {
            QuestionService.Load(Settings.SelectedTestFile);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось загрузить файл {Settings.SelectedTestFile}:\n{ex.Message}\n\nЗагружен файл по умолчанию.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            try
            {
                Settings.SelectedTestFile = "questions.json";
                QuestionService.Load("questions.json");
                SettingsService.Save(Settings); // сохраняем фолбэк
            }
            catch
            {
                MessageBox.Show("Файл questions.json также недоступен. Приложение будет закрыто.",
                    "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }
        }

        TestGenerator = new TestGenerator(new Random());
        UpdateService = new UpdateService();

        // Единственная точка запуска фоновой проверки обновлений.
        _ = Task.Run(async () =>
        {
            try { await UpdateService.CheckForUpdatesDailyAsync(); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[Update] {ex}"); }
        });
    }

    public static void ApplyTheme(string themeName)
    {
        bool isDark = themeName == "Dark";

        void SetBrush(string key, string colorHex)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
            brush.Freeze();
            Application.Current.Resources[key] = brush;
        }

        if (isDark)
        {
            SetBrush("BgColor", "#0F172A");
            SetBrush("CardBg", "#1E293B");
            SetBrush("TextMain", "#F1F5F9");
            SetBrush("TextMuted", "#94A3B8");
            SetBrush("BorderColor", "#334155");
            SetBrush("PrimaryColor", "#6366F1");
            SetBrush("PrimaryHoverColor", "#4F46E5");
            SetBrush("SuccessColor", "#34D399");
            SetBrush("SuccessBg", "#064E3B");
            SetBrush("ErrorColor", "#F87171");
            SetBrush("ErrorBg", "#7F1D1D");
            SetBrush("WindowButtonHover", "#334155");
            SetBrush("TimerBg", "#451A03");
            SetBrush("TimerText", "#FBBF24");
            SetBrush("WarningBg", "#422006");
            SetBrush("WarningText", "#FCD34D");
            SetBrush("WarningBorder", "#B45309");
        }
        else
        {
            SetBrush("BgColor", "#F8FAFC");
            SetBrush("CardBg", "#FFFFFF");
            SetBrush("TextMain", "#1E293B");
            SetBrush("TextMuted", "#64748B");
            SetBrush("BorderColor", "#E2E8F0");
            SetBrush("PrimaryColor", "#4F46E5");
            SetBrush("PrimaryHoverColor", "#4338CA");
            SetBrush("SuccessColor", "#10B981");
            SetBrush("SuccessBg", "#D1FAE5");
            SetBrush("ErrorColor", "#EF4444");
            SetBrush("ErrorBg", "#FEE2E2");
            SetBrush("WindowButtonHover", "#E2E8F0");
            SetBrush("TimerBg", "#FEF3C7");
            SetBrush("TimerText", "#D97706");
            SetBrush("WarningBg", "#FEF3C7");
            SetBrush("WarningText", "#92400E");
            SetBrush("WarningBorder", "#F59E0B");
        }

        // Тень карточек — сильнее в тёмной теме, иначе не видна
        var shadow = (DropShadowEffect)Application.Current.Resources["CardShadow"];
        shadow.Opacity = isDark ? 0.25 : 0.05;
    }
}