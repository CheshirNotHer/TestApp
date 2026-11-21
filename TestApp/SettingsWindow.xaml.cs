using System;
using System.Windows;
using TestApp.Infrastructure;
using TestApp.ViewModels;

namespace TestApp;

public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _vm = new();

    public SettingsWindow()
    {
        InitializeComponent();
        WindowHelper.FixMaximize(this);
        DataContext = _vm;
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Close_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }

    private void Save_Click(object s, RoutedEventArgs e)
    {
        var error = _vm.Validate();
        if (error != null)
        {
            MessageBox.Show(error, "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        bool fileChanged = App.Settings.SelectedTestFile != _vm.SelectedTestFile;

        if (fileChanged)
        {
            try
            {
                App.QuestionService.Load(_vm.SelectedTestFile);
                App.Settings.SelectedTestFile = _vm.SelectedTestFile;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить выбранный файл теста:\n{ex.Message}",
                    "Ошибка файла", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        App.Settings.TimedTestMinutes = _vm.TimedTestMinutes;
        App.Settings.UniformQuestionCount = _vm.UniformQuestionCount;
        App.Settings.RandomQuestionCount = _vm.RandomQuestionCount;
        App.Settings.TimedUniformQuestionCount = _vm.TimedUniformQuestionCount;
        App.Settings.ShuffleAnswers = _vm.ShuffleAnswers;
        App.Settings.ShowExplanationImmediately = _vm.ShowExplanationImmediately;
        App.Settings.Theme = _vm.IsDarkTheme ? "Dark" : "Light";
        App.ApplyTheme(App.Settings.Theme);

        App.SettingsService.Save(App.Settings);
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object s, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}