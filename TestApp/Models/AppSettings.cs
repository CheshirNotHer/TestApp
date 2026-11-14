namespace TestApp.Models;

public class AppSettings
{
    public int TimedTestMinutes { get; set; } = 30;
    public int TestQuestionCount { get; set; } = 20; 
    public bool ShuffleAnswers { get; set; } = true; 
    public bool ShowExplanationImmediately { get; set; } = true;
    public string Theme { get; set; } = "Light";
}