using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TestApp.ViewModels;

public enum OptionState { Neutral, Correct, Wrong, RevealCorrect }

public class OptionViewModel : INotifyPropertyChanged
{
    public int Index { get; }
    public string Text { get; }
    public bool IsCorrect { get; }

    private OptionState _state = OptionState.Neutral;
    public OptionState State
    {
        get => _state;
        set { if (_state != value) { _state = value; OnPropertyChanged(); } }
    }

    public OptionViewModel(int index, string text, bool isCorrect)
    {
        Index = index;
        Text = text;
        IsCorrect = isCorrect;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}