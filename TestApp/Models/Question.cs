using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TestApp.Models;

public class Question
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("topic")] public string Topic { get; set; } = "";
    [JsonPropertyName("question")] public string QuestionText { get; set; } = "";
    [JsonPropertyName("options")] public List<string> Options { get; set; } = new();
    [JsonPropertyName("correctIndex")] public int CorrectIndex { get; set; }
    [JsonPropertyName("explanation")] public string Explanation { get; set; } = "";
    [JsonPropertyName("imagePath")] public string? ImagePath { get; set; }
}

public class QuestionsFile
{
    [JsonPropertyName("questions")] public List<Question> Questions { get; set; } = new();
}