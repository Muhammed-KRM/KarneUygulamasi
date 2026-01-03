using Microsoft.AspNetCore.Http;

namespace KeremProject1backend.Core.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadAsync(IFormFile file, string folderName);
        Task DeleteAsync(string fileName, string folderName);
    }
}
