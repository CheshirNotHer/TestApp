using System.Windows;
using TestApp.Infrastructure;
using TestApp.Models;
using TestApp.ViewModels;

namespace TestApp;

public partial class TestWindow : Window
{
    private readonly TestParameters _parameters;

    public TestWindow(TestParameters parameters)
    {
        InitializeComponent();
        WindowHelper.FixMaximize(this);
        _parameters = parameters;
        SetDataContext(CreateViewModel());
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Maximize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private TestViewModel CreateViewModel()
    {
        var questions = App.TestGenerator.Generate(_parameters, App.QuestionService.AllQuestions, App.Settings);
        if (questions.Count == 0)
            MessageBox.Show("Нет валидных вопросов для выбранного режима.", "Пустой тест", MessageBoxButton.OK, MessageBoxImage.Warning);

        var qvms = new System.Collections.Generic.List<QuestionViewModel>(questions.Count);
        for (int i = 0; i < questions.Count; i++)
            qvms.Add(new QuestionViewModel(questions[i], i + 1, App.QuestionService.ResolveImagePath(questions[i])));

        var vm = new TestViewModel(qvms, _parameters);
        vm.RequestClose += Close;
        vm.RequestRetry += () => SetDataContext(CreateViewModelForRetry());
        return vm;
    }

    private TestViewModel CreateViewModelForRetry()
    {
        var questions = App.TestGenerator.Generate(_parameters, App.QuestionService.AllQuestions, App.Settings);
        var qvms = new System.Collections.Generic.List<QuestionViewModel>(questions.Count);
        for (int i = 0; i < questions.Count; i++)
            qvms.Add(new QuestionViewModel(questions[i], i + 1, App.QuestionService.ResolveImagePath(questions[i])));

        var vm = new TestViewModel(qvms, _parameters);
        vm.RequestClose += Close;
        vm.RequestRetry += () => SetDataContext(CreateViewModelForRetry());
        return vm;
    }

    private void SetDataContext(TestViewModel vm) => DataContext = vm;
}