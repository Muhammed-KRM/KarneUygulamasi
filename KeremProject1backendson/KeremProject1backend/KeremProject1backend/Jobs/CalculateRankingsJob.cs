using Hangfire;
using KeremProject1backend.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Jobs;

public class CalculateRankingsJob
{
    private readonly ApplicationContext _context;

    public CalculateRankingsJob(ApplicationContext context)
    {
        _context = context;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task Execute(int examId)
    {
        var results = await _context.ExamResults
            .Where(er => er.ExamId == examId)
            .ToListAsync();

        if (!results.Any()) return;

        // 1. Calculate Institution Rank
        var orderedResults = results.OrderByDescending(r => r.TotalNet).ToList();
        for (int i = 0; i < orderedResults.Count; i++)
        {
            orderedResults[i].InstitutionRank = i + 1;
        }

        // 2. Calculate Class Rank
        var studentIds = results.Select(r => r.StudentId).ToList();
        var classroomMap = await _context.ClassroomStudents
            .Where(cs => studentIds.Contains(cs.Student.UserId))
            .Select(cs => new { cs.Student.UserId, cs.ClassroomId })
            .ToListAsync();

        var studentToClass = classroomMap.ToDictionary(x => x.UserId, x => x.ClassroomId);

        var resultsByClass = results.GroupBy(r => studentToClass.ContainsKey(r.StudentId) ? studentToClass[r.StudentId] : -1);

        foreach (var classGroup in resultsByClass)
        {
            if (classGroup.Key == -1) continue;
            var classOrdered = classGroup.OrderByDescending(r => r.TotalNet).ToList();
            for (int i = 0; i < classOrdered.Count; i++)
            {
                classOrdered[i].ClassRank = i + 1;
            }
        }

        await _context.SaveChangesAsync();
    }
}

