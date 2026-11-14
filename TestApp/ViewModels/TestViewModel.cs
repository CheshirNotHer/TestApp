using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using TestApp.Infrastructure;
using TestApp.Models;

namespace TestApp.ViewModels;

public class TestViewModel : INotifyPropertyChanged
{
    public List<QuestionViewModel> Questions { get; }
    public TestParameters Parameters { get; }

    private int _currentIndex;
    public int CurrentIndex
    {
        get => _currentIndex;
        set
        {
            if (_currentIndex == value) return;
            _currentIndex = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentQuestion));
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(IsLastQuestion));
            OnPropertyChanged(nameof(ProgressText));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public QuestionViewModel? CurrentQuestion =>
        _currentIndex >= 0 && _currentIndex < Questions.Count ? Questions[_currentIndex] : null;

    public bool CanGoBack => _currentIndex > 0;
    public bool CanGoNext => CurrentQuestion?.IsAnswered == true;
    public bool IsLastQuestion => _currentIndex == Questions.Count - 1;
    public string ProgressText => $"Вопрос {_currentIndex + 1} из {Questions.Count}";

    private bool _isFinished;
    public bool IsFinished
    {
        get => _isFinished;
        private set { if (_isFinished != value) { _isFinished = value; OnPropertyChanged(); } }
    }

    public bool IsTimed => Parameters.Mode == TestMode.TimedUniform;

    private TimeSpan _timeRemaining;
    public TimeSpan TimeRemaining
    {
        get => _timeRemaining;
        set
        {
            _timeRemaining = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TimeRemainingText));
        }
    }
    public string TimeRemainingText => _timeRemaining.ToString(@"mm\:ss");

    public int TotalCount => Questions.Count;
    public int AnsweredCount => Questions.Count(q => q.IsAnswered);
    public int CorrectCount => Questions.Count(q => q.IsAnswered && q.IsCorrect);
    public int IncorrectCount => Questions.Count(q => q.IsAnswered && !q.IsCorrect);
    public int UnansweredCount => Questions.Count(q => !q.IsAnswered);
    public double Percent => TotalCount > 0 ? (double)CorrectCount / TotalCount * 100 : 0;
    public string PercentText => $"{Percent:F1}%";

    public ICommand SelectOptionCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand FinishCommand { get; }
    public ICommand RetryCommand { get; }
    public ICommand BackToMenuCommand { get; }

    public event Action? RequestClose;
    public event Action? RequestRetry;

    private DispatcherTimer? _timer;

    public TestViewModel(List<QuestionViewModel> questions, TestParameters p)
    {
        Questions = questions;
        Parameters = p;

        SelectOptionCommand = new RelayCommand<OptionViewModel>(SelectOption);
        NextCommand = new RelayCommand(Next, () => CanGoNext);
        BackCommand = new RelayCommand(Back, () => CanGoBack);
        FinishCommand = new RelayCommand(() => Finish(), () => CanGoNext && IsLastQuestion);
        RetryCommand = new RelayCommand(() => RequestRetry?.Invoke());
        BackToMenuCommand = new RelayCommand(() => RequestClose?.Invoke());

        if (IsTimed && App.Settings.TimedTestMinutes > 0)
        {
            TimeRemaining = TimeSpan.FromMinutes(App.Settings.TimedTestMinutes);
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        TimeRemaining -= TimeSpan.FromSeconds(1);
        if (TimeRemaining <= TimeSpan.Zero)
        {
            TimeRemaining = TimeSpan.Zero;
            Finish();
        }
    }

    private void SelectOption(OptionViewModel option)
    {
        var q = CurrentQuestion;
        if (q == null || q.IsAnswered) return;
        q.SelectedIndex = option.Index;

        OnPropertyChanged(nameof(AnsweredCount));
        OnPropertyChanged(nameof(CorrectCount));
        OnPropertyChanged(nameof(CanGoNext));
        CommandManager.InvalidateRequerySuggested();
    }

    private void Next()
    {
        if (!CanGoNext) return;
        if (IsLastQuestion) { Finish(); return; }
        CurrentIndex++;
    }

    private void Back()
    {
        if (!CanGoBack) return;
        CurrentIndex--;
    }

    private void Finish()
    {
        if (IsFinished) return;
        _timer?.Stop();
        _timer = null;

        foreach (var q in Questions.Where(q => !q.IsAnswered))
            q.RevealAnswer();

        IsFinished = true;
        OnPropertyChanged(nameof(CorrectCount));
        OnPropertyChanged(nameof(IncorrectCount));
        OnPropertyChanged(nameof(UnansweredCount));
        OnPropertyChanged(nameof(Percent));
        OnPropertyChanged(nameof(PercentText));
        CommandManager.InvalidateRequerySuggested();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}