namespace KeremProject1backend.Models.DTOs.Responses;

public class ContentAnalyticsDto
{
    public int ContentId { get; set; }
    public int Views { get; set; }
    public int Likes { get; set; }
    public int Comments { get; set; }
    public int Saves { get; set; }
    public int Shares { get; set; }
    public double EngagementRate { get; set; }
    public List<DailyViewDto> ViewsByDay { get; set; } = new List<DailyViewDto>();
    public List<TopEngagerDto> TopEngagers { get; set; } = new List<TopEngagerDto>();
}

