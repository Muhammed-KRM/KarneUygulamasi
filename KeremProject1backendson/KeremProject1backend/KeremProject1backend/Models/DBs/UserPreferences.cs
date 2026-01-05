namespace KeremProject1backend.Models.DBs;

public class UserPreferences
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    // UI Ayarları
    public string Theme { get; set; } = "dark"; // dark, light, auto
    public string Language { get; set; } = "tr";
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string TimeFormat { get; set; } = "24h"; // 24h, 12h

    // Bildirim Ayarları
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool ExamResultNotifications { get; set; } = true;
    public bool MessageNotifications { get; set; } = true;
    public bool AccountLinkNotifications { get; set; } = true;

    // Profil Düzenleme (UI Layout) - JSON string
    public string ProfileLayout { get; set; } = "{}"; // JSON with widget positions
    public bool ShowStatistics { get; set; } = true;
    public bool ShowActivity { get; set; } = true;
    public bool ShowAchievements { get; set; } = true;

    // Dashboard Ayarları - JSON string
    public string DashboardLayout { get; set; } = "{}"; // JSON with widget positions
    public string VisibleWidgets { get; set; } = "[]"; // JSON array of widget names

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

