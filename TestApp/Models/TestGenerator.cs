using System;
using System.Collections.Generic;
using System.Linq;

namespace TestApp.Models;

public class TestGenerator
{
    private readonly Random _rng;
    public TestGenerator(Random rng) => _rng = rng;

    public List<Question> Generate(TestParameters p, List<Question> all, AppSettings settings)
    {
        int count = settings.TestQuestionCount;

        // 1. Выбираем вопросы
        var selected = p.Mode switch
        {
            TestMode.All => Shuffle(all),
            TestMode.Uniform => UniformByTopic(all, count),
            TestMode.Random => Shuffle(all).Take(count).ToList(),
            TestMode.TimedUniform => UniformByTopic(all, count),
            TestMode.ByTopic => Shuffle(all.Where(q => q.Topic == p.Topic).ToList()),
            _ => new List<Question>()
        };

        // 2. Перемешиваем ответы (если включено)
        if (settings.ShuffleAnswers)
        {
            selected = selected.Select(q => CloneAndShuffleOptions(q)).ToList();
        }

        return selected;
    }

    private List<Question> UniformByTopic(List<Question> all, int target)
    {
        var groups = all.GroupBy(q => q.Topic).ToList();
        if (groups.Count == 0) return new List<Question>();

        int n = groups.Count;
        int baseCount = target / n;
        int remainder = target % n;

        var shuffled = groups.ToDictionary(g => g.Key, g => Shuffle(g.ToList()));
        var topicOrder = Shuffle(shuffled.Keys.ToList());

        var result = new List<Question>();
        int deficit = 0;

        for (int i = 0; i < topicOrder.Count; i++)
        {
            var key = topicOrder[i];
            int want = baseCount + (i < remainder ? 1 : 0);
            var pool = shuffled[key];
            int take = Math.Min(want, pool.Count);
            result.AddRange(pool.Take(take));
            deficit += want - take;
        }

        if (deficit > 0)
        {
            var usedIds = result.Select(q => q.Id).ToHashSet();
            var remaining = Shuffle(all.Where(q => !usedIds.Contains(q.Id)).ToList());
            result.AddRange(remaining.Take(deficit));
        }

        return Shuffle(result);
    }

    private Question CloneAndShuffleOptions(Question original)
    {
        // Создаем копию, чтобы не менять исходный список вопросов в памяти
        var cloned = new Question
        {
            Id = original.Id,
            Topic = original.Topic,
            QuestionText = original.QuestionText,
            Explanation = original.Explanation,
            ImagePath = original.ImagePath,
            CorrectIndex = original.CorrectIndex,
            Options = new List<string>(original.Options)
        };

        var correctOption = cloned.Options[cloned.CorrectIndex];
        cloned.Options = Shuffle(cloned.Options);
        cloned.CorrectIndex = cloned.Options.IndexOf(correctOption);

        return cloned;
    }

    private List<T> Shuffle<T>(List<T> list)
    {
        var copy = new List<T>(list);
        for (int i = copy.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }
        return copy;
    }
}