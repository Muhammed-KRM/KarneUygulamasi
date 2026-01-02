namespace KeremProject1backend.Models.Responses
{
    public class ExportImageResponse
    {
        public string ImageUrl { get; set; } = string.Empty; // Export edilmiş görselin URL'i
        public string ImageBase64 { get; set; } = string.Empty; // Base64 encoded görsel
    }

    public class EmbedCodeResponse
    {
        public string EmbedCode { get; set; } = string.Empty; // HTML embed kodu
        public string EmbedUrl { get; set; } = string.Empty; // Embed URL
    }
}

