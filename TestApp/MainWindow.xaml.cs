using System.Windows;
using TestApp.Infrastructure;
using TestApp.Models;
using TestApp.ViewModels;

namespace TestApp;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm = new();

    public MainWindow()
    {
        InitializeComponent();
        WindowHelper.FixMaximize(this);
        DataContext = _vm;
        _vm.RefreshTopics();
        _vm.RefreshLabels();
        _vm.RequestStartTest += StartTest;
        _vm.RequestOpenSettings += OpenSettings;
        Task.Run(async () =>
        {
            await App.UpdateService.CheckForUpdatesDailyAsync();
            Dispatcher.Invoke(() => _vm.RefreshUpdateStatus());
        });
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Maximize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void StartTest(TestParameters p)
    {
        this.Hide(); 

        var test = new TestWindow(p);

        test.ShowDialog();

        this.Show(); 
        _vm.RefreshLabels();
    }

    private void OpenSettings()
    {
        this.Hide(); 

        var win = new SettingsWindow(); 

        win.ShowDialog();

        this.Show();
        _vm.RefreshTopics();
        _vm.RefreshLabels();
    }

    private void All_Click(object s, RoutedEventArgs e) => _vm.StartAll();
    private void Uniform_Click(object s, RoutedEventArgs e) => _vm.StartUniform();
    private void Random_Click(object s, RoutedEventArgs e) => _vm.StartRandom();
    private void Timed_Click(object s, RoutedEventArgs e) => _vm.StartTimed();
    private void ByTopic_Click(object s, RoutedEventArgs e) => _vm.StartByTopic();
    private void Settings_Click(object s, RoutedEventArgs e) => _vm.OpenSettings();
}