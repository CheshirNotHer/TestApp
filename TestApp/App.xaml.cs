using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
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

        // 1. Инициализируем сервис обновлений
        UpdateService = new UpdateService();

        // 2. Запускаем проверку обновлений в фоновом потоке (не блокируя запуск)
        Task.Run(async () =>
        {
            await UpdateService.CheckForUpdatesDailyAsync();
        });

        // 3. Безопасно подписываемся на событие загрузки главного окна
        // Используем Dispatcher, чтобы дождаться, пока MainWindow будет создан
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (MainWindow != null)
            {
                MainWindow.Loaded += (s, args) =>
                {
                    UpdateService?.MarkHealthy();
                };
            }
        }), DispatcherPriority.ApplicationIdle);
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
        }
    }
}