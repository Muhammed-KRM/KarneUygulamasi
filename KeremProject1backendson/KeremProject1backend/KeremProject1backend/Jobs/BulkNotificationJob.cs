using Hangfire;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Services;
using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Jobs;

public class BulkNotificationJob
{
    private readonly ApplicationContext _context;
    private readonly NotificationService _notificationService;

    public BulkNotificationJob(ApplicationContext context, NotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task Execute(int examId)
    {
        var results = await _context.ExamResults
            .Include(er => er.Exam)
            .Where(er => er.ExamId == examId && er.IsConfirmed)
            .ToListAsync();

        if (!results.Any()) return;

        var exam = results.First().Exam;

        // Send notifications in batches to avoid overwhelming the system
        const int batchSize = 50;
        for (int i = 0; i < results.Count; i += batchSize)
        {
            var batch = results.Skip(i).Take(batchSize);
            
            foreach (var result in batch)
            {
                await _notificationService.SendNotificationAsync(
                    result.StudentId,
                    "Sınav Sonucunuz Açıklandı",
                    $"{exam.Title} sınav sonucunuz onaylandı. Kurum Sırası: {result.InstitutionRank}, Sınıf Sırası: {result.ClassRank}",
                    NotificationType.System,
                    $"/exams/results/{result.Id}"
                );
            }

            // Small delay between batches to avoid overwhelming the system
            if (i + batchSize < results.Count)
            {
                await Task.Delay(100); // 100ms delay between batches
            }
        }
    }
}

