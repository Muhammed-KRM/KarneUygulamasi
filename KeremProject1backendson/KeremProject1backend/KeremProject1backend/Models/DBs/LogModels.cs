namespace KeremProject1backend.Models.DBs
{
    public class DataLog
    {
        public long Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public char Action { get; set; } // C, U, D
        public int? OldModUser { get; set; }
        public DateTime? OldModTime { get; set; }
        public int ModUser { get; set; }
        public DateTime ModTime { get; set; }
    }
}
