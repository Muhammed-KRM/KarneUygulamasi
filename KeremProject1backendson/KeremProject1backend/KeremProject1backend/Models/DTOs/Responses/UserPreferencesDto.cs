namespace KeremProject1backend.Models.DTOs.Responses;

public class UserPreferencesDto
{
    public string Theme { get; set; } = "dark";
    public string Language { get; set; } = "tr";
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string TimeFormat { get; set; } = "24h";
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool ExamResultNotifications { get; set; } = true;
    public bool MessageNotifications { get; set; } = true;
    public bool AccountLinkNotifications { get; set; } = true;
    public string ProfileLayout { get; set; } = "{}";
    public bool ShowStatistics { get; set; } = true;
    public bool ShowActivity { get; set; } = true;
    public bool ShowAchievements { get; set; } = true;
    public string DashboardLayout { get; set; } = "{}";
    public string VisibleWidgets { get; set; } = "[]";
}

