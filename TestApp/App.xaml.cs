using System;
using System.Windows;
using TestApp.Models;
using TestApp.Services;

namespace TestApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static QuestionService QuestionService { get; private set; } = null!;
        public static SettingsService SettingsService { get; private set; } = null!;
        public static AppSettings Settings { get; set; } = null!;
        public static TestGenerator TestGenerator { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SettingsService = new SettingsService();
            Settings = SettingsService.Load();

            QuestionService = new QuestionService();
            try
            {
                QuestionService.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить questions.json:\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            TestGenerator = new TestGenerator(new Random());
        }
    }

}
