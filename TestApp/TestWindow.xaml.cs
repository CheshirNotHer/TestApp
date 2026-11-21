using System.Windows;
using TestApp.Infrastructure;
using TestApp.Models;
using TestApp.ViewModels;

namespace TestApp;

public partial class TestWindow : Window
{
    private readonly TestParameters _parameters;
    private bool _forceClose;

    public TestWindow(TestParameters parameters)
    {
        InitializeComponent();
        WindowHelper.FixMaximize(this);
        _parameters = parameters;
        SetDataContext(CreateViewModel());
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Maximize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as TestViewModel;
        if (vm != null && !vm.IsFinished)
        {
            var result = MessageBox.Show(
                "Прервать тест? Текущие ответы будут потеряны.",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
        }
        _forceClose = true;
        Close();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Если пользователь закрывает окно стандартным Alt+F4 или крестиком
        // (не через Close_Click), — тоже спросим подтверждение.
        if (!_forceClose)
        {
            var vm = DataContext as TestViewModel;
            if (vm != null && !vm.IsFinished)
            {
                var result = MessageBox.Show(
                    "Прервать тест? Текущие ответы будут потеряны.",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) { e.Cancel = true; return; }
            }
        }
        base.OnClosing(e);
    }

    private TestViewModel CreateViewModel()
    {
        var questions = App.TestGenerator.Generate(_parameters, App.QuestionService.AllQuestions, App.Settings);
        if (questions.Count == 0)
        {
            MessageBox.Show("Нет валидных вопросов для выбранного режима.", "Пустой тест", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        var qvms = new System.Collections.Generic.List<QuestionViewModel>(questions.Count);
        for (int i = 0; i < questions.Count; i++)
            qvms.Add(new QuestionViewModel(questions[i], i + 1, App.QuestionService.ResolveImagePath(questions[i])));

        var vm = new TestViewModel(qvms, _parameters);
        vm.RequestClose += () => { _forceClose = true; Close(); };
        vm.RequestRetry += () => SetDataContext(CreateViewModel());
        return vm;
    }

    private void SetDataContext(TestViewModel vm) => DataContext = vm;
}