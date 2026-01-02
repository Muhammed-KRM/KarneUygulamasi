using System.ComponentModel.DataAnnotations;

namespace KeremProject1backend.Models.Requests
{
    public class GetUserLogsRequest
    {
        public int? UserId { get; set; } // null ise kendi logları, admin için başka kullanıcının logları
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 20;
        public string? TableName { get; set; } // Filtreleme için
        public char? Action { get; set; } // C, U, D - Filtreleme için
        public DateTime? StartDate { get; set; } // Tarih aralığı
        public DateTime? EndDate { get; set; }
    }

    public class GetAdminLogsRequest
    {
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 50;
        public int? UserId { get; set; } // Belirli bir kullanıcının logları
        public string? TableName { get; set; } // Filtreleme için
        public char? Action { get; set; } // C, U, D - Filtreleme için
        public DateTime? StartDate { get; set; } // Tarih aralığı
        public DateTime? EndDate { get; set; }
    }
}

