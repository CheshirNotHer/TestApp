using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TestApp.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private int _minutes;
    public int TimedTestMinutes
    {
        get => _minutes;
        set { _minutes = value; OnPropertyChanged(); }
    }

    private int _qCount;
    public int TestQuestionCount
    {
        get => _qCount;
        set { _qCount = value; OnPropertyChanged(); }
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

    private string _selectedTestFile;
    public string SelectedTestFile
    {
        get => _selectedTestFile;
        set { _selectedTestFile = value; OnPropertyChanged(); }
    }

    public SettingsViewModel()
    {
        TimedTestMinutes = App.Settings.TimedTestMinutes;
        TestQuestionCount = App.Settings.TestQuestionCount;
        ShuffleAnswers = App.Settings.ShuffleAnswers;
        ShowExplanationImmediately = App.Settings.ShowExplanationImmediately;
        IsDarkTheme = App.Settings.Theme == "Dark";

        AvailableTestFiles = App.QuestionService.GetAvailableTestFiles();
        SelectedTestFile = App.Settings.SelectedTestFile;

        if (!AvailableTestFiles.Contains(SelectedTestFile) && AvailableTestFiles.Count > 0)
        {
            SelectedTestFile = AvailableTestFiles[0];
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}