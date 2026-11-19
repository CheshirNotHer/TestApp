using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TestApp.Models;

namespace TestApp.Services;

public class QuestionService
{
    public List<Question> AllQuestions { get; private set; } = new();
    private readonly string _baseDir;

    public QuestionService()
    {
        _baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tests");

        if (!Directory.Exists(_baseDir))
        {
            Directory.CreateDirectory(_baseDir);
        }
    }

    public IReadOnlyList<string> GetTopics() =>
        AllQuestions.Select(q => q.Topic).Distinct().OrderBy(t => t).ToList();

    public List<string> GetAvailableTestFiles()
    {
        if (!Directory.Exists(_baseDir)) return new List<string>();

        return Directory.GetFiles(_baseDir, "*.json")
            .Select(Path.GetFileName)
            .OrderBy(f => f)
            .ToList()!;
    }

    public void Load(string? fileName = null)
    {
        if (string.IsNullOrWhiteSpace(fileName)) fileName = "questions.json";

        var path = Path.Combine(_baseDir, fileName);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Файл теста не найден по пути: {path}");

        var json = File.ReadAllText(path);
        var file = JsonSerializer.Deserialize<QuestionsFile>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        AllQuestions = (file?.Questions ?? new()).Where(IsValid).ToList();
    }

    public string? ResolveImagePath(Question q)
    {
        if (string.IsNullOrWhiteSpace(q.ImagePath)) return null;

        var full = Path.IsPathRooted(q.ImagePath)
            ? q.ImagePath
            : Path.Combine(_baseDir, q.ImagePath);

        return File.Exists(full) ? full : null;
    }

    private static bool IsValid(Question q) =>
        q.Options is { Count: >= 2 } &&
        q.CorrectIndex >= 0 && q.CorrectIndex < q.Options.Count;
}