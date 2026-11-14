using System.Windows;
using TestApp.Models;
using TestApp.ViewModels;

namespace TestApp;

public partial class TestWindow : Window
{
    private readonly TestParameters _parameters;

    public TestWindow(TestParameters parameters)
    {
        InitializeComponent();
        _parameters = parameters;
        SetDataContext(CreateViewModel());
    }

    private TestViewModel CreateViewModel()
    {
        // ВОТ ЗДЕСЬ мы передаем третьим аргументом App.Settings
        var questions = App.TestGenerator.Generate(_parameters, App.QuestionService.AllQuestions, App.Settings);

        if (questions.Count == 0)
        {
            MessageBox.Show("Нет валидных вопросов для выбранного режима.",
                "Пустой тест", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        var qvms = new System.Collections.Generic.List<QuestionViewModel>(questions.Count);
        for (int i = 0; i < questions.Count; i++)
            qvms.Add(new QuestionViewModel(questions[i], i + 1,
                App.QuestionService.ResolveImagePath(questions[i])));

        var vm = new TestViewModel(qvms, _parameters);
        vm.RequestClose += Close;
        vm.RequestRetry += () =>
        {
            var newVm = CreateViewModelForRetry();
            SetDataContext(newVm);
        };
        return vm;
    }

    private TestViewModel CreateViewModelForRetry()
    {
        // И ВОТ ЗДЕСЬ тоже передаем App.Settings
        var questions = App.TestGenerator.Generate(_parameters, App.QuestionService.AllQuestions, App.Settings);

        var qvms = new System.Collections.Generic.List<QuestionViewModel>(questions.Count);
        for (int i = 0; i < questions.Count; i++)
            qvms.Add(new QuestionViewModel(questions[i], i + 1,
                App.QuestionService.ResolveImagePath(questions[i])));

        var vm = new TestViewModel(qvms, _parameters);
        vm.RequestClose += Close;
        vm.RequestRetry += () => SetDataContext(CreateViewModelForRetry());
        return vm;
    }

    private void SetDataContext(TestViewModel vm) => DataContext = vm;
}