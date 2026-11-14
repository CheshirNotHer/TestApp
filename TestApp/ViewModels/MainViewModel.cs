using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TestApp.Models;

namespace TestApp.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public string UniformButtonText => $"2. {App.Settings.TestQuestionCount} вопросов равномерно по темам";
    public string RandomButtonText => $"3. {App.Settings.TestQuestionCount} случайных вопросов";
    public string TimedButtonText => $"4. {App.Settings.TestQuestionCount} вопросов равномерно (с таймером)";
    public ObservableCollection<string> Topics { get; } = new();

    private string? _selectedTopic;
    public string? SelectedTopic
    {
        get => _selectedTopic;
        set { _selectedTopic = value; OnPropertyChanged(); }
    }

    public string QuestionCountText =>
        $"Всего валидных вопросов: {App.QuestionService.AllQuestions.Count}";
    public string TimedMinutesText =>
        $"Время на тест с таймером: {App.Settings.TimedTestMinutes} мин.";

    public event Action<TestParameters>? RequestStartTest;
    public event Action? RequestOpenSettings;

    public void RefreshTopics()
    {
        Topics.Clear();
        foreach (var t in App.QuestionService.GetTopics()) Topics.Add(t);
        SelectedTopic ??= Topics.Count > 0 ? Topics[0] : null;
    }

    public void RefreshLabels()
    {
        OnPropertyChanged(nameof(QuestionCountText));
        OnPropertyChanged(nameof(TimedMinutesText));
        OnPropertyChanged(nameof(UniformButtonText)); 
        OnPropertyChanged(nameof(RandomButtonText));  
        OnPropertyChanged(nameof(TimedButtonText));
    }

    public void StartAll() => RequestStartTest?.Invoke(new TestParameters { Mode = TestMode.All });
    public void StartUniform() => RequestStartTest?.Invoke(new TestParameters { Mode = TestMode.Uniform });
    public void StartRandom() => RequestStartTest?.Invoke(new TestParameters { Mode = TestMode.Random });
    public void StartTimed() => RequestStartTest?.Invoke(new TestParameters { Mode = TestMode.TimedUniform });
    public void StartByTopic()
    {
        if (!string.IsNullOrEmpty(SelectedTopic))
            RequestStartTest?.Invoke(new TestParameters
            {
                Mode = TestMode.ByTopic,
                Topic = SelectedTopic
            });
    }
    public void OpenSettings() => RequestOpenSettings?.Invoke();

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}