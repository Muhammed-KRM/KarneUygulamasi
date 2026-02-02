using System.Text.Json;
using KeremProject1backend.Models.DBs;

namespace KeremProject1backend.Services;

public class OpticalParserService
{
    // Logic for parsing optical TXT files

    public async Task<List<ParsedOpticalLine>> ParseFileAsync(Stream fileStream)
    {
        var result = new List<ParsedOpticalLine>();
        using var reader = new StreamReader(fileStream);
        int lineNumber = 0;

        while (!reader.EndOfStream)
        {
            lineNumber++;
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line) || line.Length < 37) continue;

            // Format: [Öğr.No(0-9)] [AdSoyad(10-35)] [Kitapçık(36)] [Cevaplar(37-160)]
            result.Add(new ParsedOpticalLine
            {
                LineNumber = lineNumber,
                StudentNumber = line.Substring(0, 10).Trim(),
                StudentName = line.Substring(10, 25).Trim(),
                BookletType = line.Substring(36, 1).ToUpper(),
                Answers = line.Substring(37).Trim()
            });
        }
        return result;
    }

    public Dictionary<string, LessonScore> CalculateResults(
        string studentAnswers,
        Dictionary<string, string> answerKey,
        Dictionary<string, LessonConfig> lessonConfigs)
    {
        var results = new Dictionary<string, LessonScore>();

        foreach (var lesson in answerKey)
        {
            var lessonName = lesson.Key;
            var correctAnswers = lesson.Value;

            if (!lessonConfigs.TryGetValue(lessonName, out var config)) continue;

            // Extract answers for this specific lesson
            if (config.StartIndex < 0 || config.StartIndex >= studentAnswers.Length) continue;

            string studentLessonAnswers = studentAnswers.Substring(config.StartIndex,
                Math.Min(config.QuestionCount, studentAnswers.Length - config.StartIndex));

            var lessonScore = new LessonScore();

            for (int i = 0; i < correctAnswers.Length && i < studentLessonAnswers.Length; i++)
            {
                char studentAns = studentLessonAnswers[i];
                char correctAns = correctAnswers[i];

                bool isCorrect = studentAns == correctAns;
                bool isEmpty = studentAns == ' ' || studentAns == '0';
                bool isWrong = !isEmpty && !isCorrect;

                if (isCorrect) lessonScore.Correct++;
                else if (isEmpty) lessonScore.Empty++;
                else lessonScore.Wrong++;

                // Topic Mapping
                var topicName = GetTopicForQuestion(i, config.TopicMapping);
                if (!string.IsNullOrEmpty(topicName))
                {
                    if (!lessonScore.TopicScores.ContainsKey(topicName))
                        lessonScore.TopicScores[topicName] = new TopicScore();

                    var topicScore = lessonScore.TopicScores[topicName];
                    if (isCorrect) topicScore.Correct++;
                    else if (isEmpty) topicScore.Empty++;
                    else topicScore.Wrong++;
                }
            }

            lessonScore.Net = lessonScore.Correct - (lessonScore.Wrong / 4.0f);
            lessonScore.SuccessRate = correctAnswers.Length > 0 ? (int)((lessonScore.Correct / (float)correctAnswers.Length) * 100) : 0;

            // Calculate Topic Nets
            foreach (var ts in lessonScore.TopicScores.Values)
            {
                ts.Net = ts.Correct - (ts.Wrong / 4.0f);
            }

            results[lessonName] = lessonScore;
        }

        return results;
    }

    private string? GetTopicForQuestion(int questionIndex, Dictionary<string, string> topicMapping)
    {
        if (topicMapping == null) return null;

        foreach (var mapping in topicMapping)
        {
            var range = mapping.Key.Split('-');
            if (range.Length == 2 && int.TryParse(range[0], out int start) && int.TryParse(range[1], out int end))
            {
                if (questionIndex >= start && questionIndex <= end)
                    return mapping.Value;
            }
            else if (int.TryParse(mapping.Key, out int single) && single == questionIndex)
            {
                return mapping.Value;
            }
        }
        return null;
    }
}

public class ParsedOpticalLine
{
    public int LineNumber { get; set; }
    public required string StudentNumber { get; set; }
    public required string StudentName { get; set; }
    public required string BookletType { get; set; }
    public required string Answers { get; set; }
}

public class LessonConfig
{
    public int StartIndex { get; set; }
    public int QuestionCount { get; set; }
    public Dictionary<string, string> TopicMapping { get; set; } = new(); // e.g., "0-9": "Topic A"
}

public class LessonScore
{
    public int Correct { get; set; }
    public int Wrong { get; set; }
    public int Empty { get; set; }
    public float Net { get; set; }
    public int SuccessRate { get; set; }
    public Dictionary<string, TopicScore> TopicScores { get; set; } = new();
}

public class TopicScore
{
    public string TopicName { get; set; } = string.Empty;
    public int Correct { get; set; }
    public int Wrong { get; set; }
    public int Empty { get; set; }
    public float Net { get; set; }
}
