using System.Windows;
using TestApp.ViewModels;

namespace TestApp;

public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _vm = new();

    public SettingsWindow()
    {
        InitializeComponent();
        DataContext = _vm;
    }

    private void Save_Click(object s, RoutedEventArgs e)
    {
        if (_vm.TimedTestMinutes <= 0 || _vm.TestQuestionCount <= 0)
        {
            MessageBox.Show("Укажите положительные числа для времени и количества вопросов.",
                "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        App.Settings.TimedTestMinutes = _vm.TimedTestMinutes;
        App.Settings.TestQuestionCount = _vm.TestQuestionCount;
        App.Settings.ShuffleAnswers = _vm.ShuffleAnswers;
        App.Settings.ShowExplanationImmediately = _vm.ShowExplanationImmediately;

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