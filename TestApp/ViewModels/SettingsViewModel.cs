using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TestApp.Infrastructure;
using TestApp.Services;

namespace TestApp.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private int _minutes;
    public int TimedTestMinutes
    {
        get => _minutes;
        set { _minutes = value; OnPropertyChanged(); }
    }

    private int _uniformCount;
    public int UniformQuestionCount
    {
        get => _uniformCount;
        set { _uniformCount = value; OnPropertyChanged(); }
    }

    private int _randomCount;
    public int RandomQuestionCount
    {
        get => _randomCount;
        set { _randomCount = value; OnPropertyChanged(); }
    }

    private int _timedCount;
    public int TimedUniformQuestionCount
    {
        get => _timedCount;
        set { _timedCount = value; OnPropertyChanged(); }
    }

    private bool _shuffleAnswers;
    public bool ShuffleAnswers
    {
        get => _shuffleAnswers;
        set { _shuffleAnswers = value; OnPropertyChanged(); }
    }

    private bool _showExplanation;
    public bool ShowExplanationImmediately
    {
        get => _showExplanation;
        set { _showExplanation = value; OnPropertyChanged(); }
    }

    private bool _isDarkTheme;
    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set { _isDarkTheme = value; OnPropertyChanged(); }
    }

    private List<string> _availableTestFiles = new();
    public List<string> AvailableTestFiles
    {
        get => _availableTestFiles;
        set { _availableTestFiles = value; OnPropertyChanged(); }
    }

    private string _selectedTestFile = "";
    public string SelectedTestFile
    {
        get => _selectedTestFile;
        set { _selectedTestFile = value; OnPropertyChanged(); }
    }

    public bool CanUpdate => App.Settings.IsUpdateAvailable;

    // === Прогресс обновления ===

    private bool _isUpdating;
    public bool IsUpdating
    {
        get => _isUpdating;
        private set { _isUpdating = value; OnPropertyChanged(); OnPropertyChanged(nameof(UpdateButtonText)); }
    }

    private string _updateStage = "";
    public string UpdateStage
    {
        get => _updateStage;
        private set { _updateStage = value; OnPropertyChanged(); }
    }

    private int _updatePercent;
    public int UpdatePercent
    {
        get => _updatePercent;
        private set { _updatePercent = value; OnPropertyChanged(); }
    }

    private bool _updateIndeterminate;
    public bool UpdateIsIndeterminate
    {
        get => _updateIndeterminate;
        private set { _updateIndeterminate = value; OnPropertyChanged(); }
    }

    public string UpdateButtonText => IsUpdating ? "Обновление…" : "Обновить приложение";

    public ICommand UpdateCommand { get; }

    public SettingsViewModel()
    {
        TimedTestMinutes = App.Settings.TimedTestMinutes;
        UniformQuestionCount = App.Settings.UniformQuestionCount;
        RandomQuestionCount = App.Settings.RandomQuestionCount;
        TimedUniformQuestionCount = App.Settings.TimedUniformQuestionCount;
        ShuffleAnswers = App.Settings.ShuffleAnswers;
        ShowExplanationImmediately = App.Settings.ShowExplanationImmediately;
        IsDarkTheme = App.Settings.Theme == "Dark";

        AvailableTestFiles = App.QuestionService.GetAvailableTestFiles();
        SelectedTestFile = App.Settings.SelectedTestFile;
        if (!AvailableTestFiles.Contains(SelectedTestFile) && AvailableTestFiles.Count > 0)
            SelectedTestFile = AvailableTestFiles[0];

        UpdateCommand = new AsyncRelayCommand(RunUpdateAsync, () => CanUpdate && !IsUpdating);
    }

    private async System.Threading.Tasks.Task RunUpdateAsync()
    {
        IsUpdating = true;
        UpdateStage = "Подготовка…";
        UpdatePercent = 0;
        UpdateIsIndeterminate = false;

        var progress = new Progress<UpdateProgress>(p =>
        {
            UpdateStage = p.Stage;
            UpdateIsIndeterminate = p.IsIndeterminate;
            UpdatePercent = p.Percent;
        });

        try
        {
            await App.UpdateService.PerformUpdateAsync(progress);
        }
        finally
        {
            IsUpdating = false;
            UpdateStage = "";
            UpdatePercent = 0;
            UpdateIsIndeterminate = false;
        }
    }

    public string? Validate()
    {
        if (TimedTestMinutes <= 0)
            return "Время теста с таймером должно быть положительным.";
        if (UniformQuestionCount <= 0)
            return "Количество вопросов (режим 2) должно быть положительным.";
        if (RandomQuestionCount <= 0)
            return "Количество вопросов (режим 3) должно быть положительным.";
        if (TimedUniformQuestionCount <= 0)
            return "Количество вопросов (режим 4) должно быть положительным.";
        return null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}