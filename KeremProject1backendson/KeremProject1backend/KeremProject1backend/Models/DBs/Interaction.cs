using KeremProject1backend.Models.Enums;

namespace KeremProject1backend.Models.DBs;

public class Interaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int? ContentId { get; set; }
    public Content? Content { get; set; }

    public InteractionType Type { get; set; } // Like, Save, Share, View, Report

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

