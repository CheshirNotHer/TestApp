using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TestApp.Models;

namespace TestApp.ViewModels;

public enum QuestionStatus { Unanswered, Correct, Incorrect }

public class QuestionViewModel : INotifyPropertyChanged
{
    public Question Source { get; }
    public int Number { get; }
    public string QuestionText => Source.QuestionText;
    public string Explanation => Source.Explanation;
    public string Topic => Source.Topic;
    public int CorrectIndex => Source.CorrectIndex;
    public List<OptionViewModel> Options { get; }

    public string? ImageFullPath { get; }
    public bool HasImage => ImageFullPath != null;
    public ImageSource? ImageSource => LoadImage(ImageFullPath);
    public bool HasExplanation => !string.IsNullOrWhiteSpace(Explanation);

    private int? _selectedIndex;
    public int? SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex == value) return;
            _selectedIndex = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsAnswered));
            OnPropertyChanged(nameof(IsExplanationVisible)); // Добавлено
            OnPropertyChanged(nameof(IsCorrect));
            OnPropertyChanged(nameof(Status));
            UpdateOptionStates();
        }
    }

    public bool IsAnswered => _selectedIndex.HasValue;
    public bool IsCorrect => _selectedIndex == CorrectIndex;

    // Логика: показывать пояснение сразу (если включено в настройках) и только после ответа
    public bool IsExplanationVisible => IsAnswered && App.Settings.ShowExplanationImmediately;

    public QuestionStatus Status => !IsAnswered
        ? QuestionStatus.Unanswered
        : (IsCorrect ? QuestionStatus.Correct : QuestionStatus.Incorrect);

    public string AnswerStatusDisplay => Status switch
    {
        QuestionStatus.Correct => "Правильно",
        QuestionStatus.Incorrect => "Неправильно",
        _ => "Не отвечено"
    };

    public QuestionViewModel(Question source, int number, string? imagePath)
    {
        Source = source;
        Number = number;
        ImageFullPath = imagePath;
        Options = source.Options
            .Select((t, i) => new OptionViewModel(i, t, i == source.CorrectIndex))
            .ToList();
    }

    public void RevealAnswer()
    {
        if (IsAnswered) return;
        foreach (var o in Options)
            if (o.IsCorrect) o.State = OptionState.RevealCorrect;
    }

    private void UpdateOptionStates()
    {
        if (!IsAnswered) return;
        foreach (var o in Options)
        {
            if (o.Index == _selectedIndex)
                o.State = o.IsCorrect ? OptionState.Correct : OptionState.Wrong;
            else if (o.Index == CorrectIndex)
                o.State = OptionState.RevealCorrect;
            else
                o.State = OptionState.Neutral;
        }
    }

    private static ImageSource? LoadImage(string? path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new System.Uri(path, System.UriKind.Absolute);
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch { return null; }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}