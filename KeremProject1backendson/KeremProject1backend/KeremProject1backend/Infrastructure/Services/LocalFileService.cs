using KeremProject1backend.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace KeremProject1backend.Infrastructure.Services
{
    public class LocalFileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        public LocalFileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File cannot be empty");

            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folderName);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return Path.Combine("uploads", folderName, uniqueFileName).Replace("\\", "/");
        }

        public Task DeleteAsync(string fileName, string folderName)
        {
            string filePath = Path.Combine(_environment.WebRootPath, "uploads", folderName, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }
    }
}
