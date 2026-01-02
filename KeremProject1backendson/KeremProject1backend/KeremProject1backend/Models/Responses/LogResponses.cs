namespace KeremProject1backend.Models.Responses
{
    public class LogDto
    {
        public long Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public char Action { get; set; } // C=Create, U=Update, D=Delete
        public string ActionName { get; set; } = string.Empty; // "Oluşturuldu", "Güncellendi", "Silindi"
        public int? OldModUser { get; set; }
        public string? OldModUsername { get; set; }
        public DateTime? OldModTime { get; set; }
        public int ModUser { get; set; }
        public string ModUsername { get; set; } = string.Empty;
        public DateTime ModTime { get; set; }
    }

    public class LogListResponse
    {
        public List<LogDto> Logs { get; set; } = new List<LogDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public int TotalPages { get; set; }
    }
}

