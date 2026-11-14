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
    private string _baseDir = AppDomain.CurrentDomain.BaseDirectory;

    public IReadOnlyList<string> GetTopics() =>
        AllQuestions.Select(q => q.Topic).Distinct().OrderBy(t => t).ToList();

    public void Load(string? path = null)
    {
        path ??= Path.Combine(_baseDir, "questions.json");
        _baseDir = Path.GetDirectoryName(Path.GetFullPath(path)) ?? _baseDir;

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