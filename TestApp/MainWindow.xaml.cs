using System.Windows;
using TestApp.Models;
using TestApp.ViewModels;

namespace TestApp;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
        _vm.RefreshTopics();
        _vm.RefreshLabels();
        _vm.RequestStartTest += StartTest;
        _vm.RequestOpenSettings += OpenSettings;
    }

    private void StartTest(TestParameters p)
    {
        var test = new TestWindow(p) { Owner = this };
        test.ShowDialog();
        // после закрытия настройки могли измениться
        _vm.RefreshLabels();
    }

    private void OpenSettings()
    {
        var win = new SettingsWindow { Owner = this };
        win.ShowDialog();
        _vm.RefreshLabels();
    }

    private void All_Click(object s, RoutedEventArgs e) => _vm.StartAll();
    private void Uniform_Click(object s, RoutedEventArgs e) => _vm.StartUniform();
    private void Random_Click(object s, RoutedEventArgs e) => _vm.StartRandom();
    private void Timed_Click(object s, RoutedEventArgs e) => _vm.StartTimed();
    private void ByTopic_Click(object s, RoutedEventArgs e) => _vm.StartByTopic();
    private void Settings_Click(object s, RoutedEventArgs e) => _vm.OpenSettings();
}