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
            // config.StartIndex might need to be adjusted based on total length
            string studentLessonAnswers = studentAnswers.Substring(config.StartIndex,
                Math.Min(config.QuestionCount, studentAnswers.Length - config.StartIndex));

            int correct = 0, wrong = 0, empty = 0;

            for (int i = 0; i < correctAnswers.Length && i < studentLessonAnswers.Length; i++)
            {
                char studentAns = studentLessonAnswers[i];
                char correctAns = correctAnswers[i];

                if (studentAns == ' ' || studentAns == '0') empty++;
                else if (studentAns == correctAns) correct++;
                else wrong++;
            }

            float net = correct - (wrong / 4.0f);
            int successRate = correctAnswers.Length > 0 ? (int)((correct / (float)correctAnswers.Length) * 100) : 0;

            results[lessonName] = new LessonScore
            {
                Correct = correct,
                Wrong = wrong,
                Empty = empty,
                Net = net,
                SuccessRate = successRate,
                TopicScores = new List<TopicScore>() // Topic scores can be calculated separately if needed
            };
        }

        return results;
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
    public required Dictionary<string, string> TopicMapping { get; set; } // e.g., "0-9": "Topic A"
}

public class LessonScore
{
    public int Correct { get; set; }
    public int Wrong { get; set; }
    public int Empty { get; set; }
    public float Net { get; set; }
    public int SuccessRate { get; set; }
    public List<TopicScore>? TopicScores { get; set; }
}

public class TopicScore
{
    public string TopicName { get; set; } = string.Empty;
    public int Correct { get; set; }
    public int Wrong { get; set; }
    public int Empty { get; set; }
    public float Net { get; set; }
}
