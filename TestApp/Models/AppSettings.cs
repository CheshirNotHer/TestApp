using System;

namespace TestApp.Models;

public class AppSettings
{
    public int TimedTestMinutes { get; set; } = 30;

    // Счётчики вопросов по режимам
    public int UniformQuestionCount { get; set; } = 20;
    public int RandomQuestionCount { get; set; } = 20;
    public int TimedUniformQuestionCount { get; set; } = 20;

    public bool ShuffleAnswers { get; set; } = true;
    public bool ShowExplanationImmediately { get; set; } = true;
    public string Theme { get; set; } = "Light";
    public string SelectedTestFile { get; set; } = "questions.json";

    // Автообновление
    public DateTime? LastUpdateCheck { get; set; }
    public bool IsUpdateAvailable { get; set; }
    public string? AvailableVersion { get; set; }

    /// <summary>Целевое количество вопросов для режима (0 — без ограничения).</summary>
    public int GetQuestionCount(TestMode mode) => mode switch
    {
        TestMode.Uniform => UniformQuestionCount,
        TestMode.Random => RandomQuestionCount,
        TestMode.TimedUniform => TimedUniformQuestionCount,
        _ => 0
    };
}