namespace KeremProject1backend.Infrastructure.Jobs
{
    public class ReportJobs
    {
        public Task GenerateExamReport(int examId)
        {
            // Bu metod Hangfire tarafından arka planda çağırılacak
            // Simülasyon: Rapor oluşturma işlemi
            Console.WriteLine($"Exam report generation started for ExamId: {examId}");

            // TODO: Rapor oluşturma mantığı buraya eklenecek

            Console.WriteLine($"Exam report generation completed for ExamId: {examId}");
            return Task.CompletedTask;
        }
    }
}
