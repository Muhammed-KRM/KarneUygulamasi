namespace KeremProject1backend.Models.DTOs.Requests;

public class VotePollRequest
{
    public List<int> OptionIndices { get; set; } = new(); // Multiple choice için birden fazla seçenek
}

