using KeremProject1backend.Models.Enums;

namespace KeremProject1backend.Models.DTOs.Responses;

public class ContentDetailDto : ContentDto
{
    public List<CommentDto>? Comments { get; set; }
    public bool IsAuthor { get; set; }
}

