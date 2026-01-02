using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace KeremProject1backend.Services
{
    /// <summary>
    /// Dosya tipleri enum'u
    /// </summary>
    public enum FileType
    {
        User = 0,        // Kullanıcı profil resimleri
        Export = 1,     // Export edilmiş görseller
        Temp = 2,        // Geçici dosyalar
        List = 3        // Liste görselleri
    }

    /// <summary>
    /// Genel dosya yönetim servisi
    /// Temp dosyaları kalıcı konuma taşır, güvenli download linkleri oluşturur
    /// </summary>
    public static class FileService
    {
        private const string UPLOAD_PREFIX = "UPLOAD_";
        private const int FILE_ID_PADDING = 7;

        /// <summary>
        /// Temp dosyayı kalıcı konuma taşır ve dosya adını formatlar
        /// </summary>
        /// <param name="tempFileLink">Temp dosya linki (UPLOAD_ prefix'li)</param>
        /// <param name="recordId">Kayıt ID'si (User ID, List ID vb.)</param>
        /// <param name="fileName">Orijinal dosya adı</param>
        /// <param name="fileType">Dosya tipi</param>
        /// <param name="tempDirectory">Temp dosya dizini</param>
        /// <param name="targetDirectory">Hedef dizin</param>
        /// <returns>Formatlanmış dosya adı (null ise başarısız)</returns>
        public static string? MoveTempFileToPermanent(
            string tempFileLink,
            int recordId,
            string fileName,
            FileType fileType,
            string tempDirectory,
            string targetDirectory)
        {
            if (string.IsNullOrWhiteSpace(tempFileLink))
                return null;

            // Temp dosya kontrolü
            if (!tempFileLink.StartsWith(UPLOAD_PREFIX, StringComparison.OrdinalIgnoreCase))
                return null; // Zaten kalıcı bir dosya

            // Temp dosya yolu
            string tempFilePath = Path.Combine(tempDirectory, tempFileLink.Substring(UPLOAD_PREFIX.Length));

            if (!File.Exists(tempFilePath))
                return null; // Temp dosya bulunamadı

            // Hedef dizin yoksa oluştur
            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            // Formatlanmış dosya adı: RecordId_DateTime_FileName
            string formattedFileName = $"{recordId.ToString().PadLeft(FILE_ID_PADDING, '0')}_{DateTime.Now:yyyyMMddHHmmssffff}_{fileName}";

            // Hedef dosya yolu
            string targetFilePath = Path.Combine(targetDirectory, formattedFileName);

            try
            {
                // Dosyayı kopyala
                File.Copy(tempFilePath, targetFilePath, overwrite: true);

                // Temp dosyayı sil
                File.Delete(tempFilePath);

                return formattedFileName;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Güvenli download linki oluşturur
        /// </summary>
        /// <param name="fileName">Dosya adı</param>
        /// <param name="fileType">Dosya tipi</param>
        /// <param name="session">Kullanıcı session'ı</param>
        /// <param name="baseUrl">Base URL (appsettings'den gelecek)</param>
        /// <returns>Güvenli download linki</returns>
        public static string GenerateFileLink(
            string fileName,
            FileType fileType,
            ClaimsPrincipal session,
            string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            // Session'dan kullanıcı ID'sini al
            var userIdClaim = session.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return string.Empty;

            // Güvenlik için signature oluştur
            string signature = GenerateSignature(fileName, fileType, userIdClaim.Value);

            // Link formatı: baseUrl?filename=...&type=...&sessionno=...&signature=...
            string encodedFileName = Uri.EscapeDataString(fileName);
            return $"{baseUrl.TrimEnd('/')}?filename={encodedFileName}&type={(int)fileType}&sessionno={userIdClaim.Value}&signature={signature}";
        }

        /// <summary>
        /// Dosya linki için güvenlik imzası oluşturur
        /// </summary>
        private static string GenerateSignature(string fileName, FileType fileType, string userId, string? secretKey = null)
        {
            // Güvenlik için secret key kullan
            secretKey ??= "ANIME_RANKER_FILE_SECRET_KEY_2024"; // Default, appsettings'den alınmalı
            string data = $"{fileName}_{fileType}_{userId}_{secretKey}";
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes).Substring(0, 16).Replace("+", "-").Replace("/", "_");
            }
        }

        /// <summary>
        /// GenerateFileLink için secret key ile imza oluşturma
        /// </summary>
        public static string GenerateFileLinkWithSecret(
            string fileName,
            FileType fileType,
            ClaimsPrincipal session,
            string baseUrl,
            string secretKey)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            var userIdClaim = session.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return string.Empty;

            string signature = GenerateSignature(fileName, fileType, userIdClaim.Value, secretKey);
            // URL formatı: baseUrl?filename=...&type=...&sessionno=...&signature=...
            string encodedFileName = Uri.EscapeDataString(fileName);
            return $"{baseUrl.TrimEnd('/')}?filename={encodedFileName}&type={(int)fileType}&sessionno={userIdClaim.Value}&signature={signature}";
        }

        /// <summary>
        /// Dosya linki imzasını doğrular
        /// </summary>
        public static bool ValidateFileLinkSignature(
            string fileName,
            FileType fileType,
            string userId,
            string signature,
            string? secretKey = null)
        {
            string expectedSignature = GenerateSignature(fileName, fileType, userId, secretKey);
            return expectedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Dosya yolu oluşturur
        /// </summary>
        public static string GetFilePath(string fileName, FileType fileType, string baseDirectory)
        {
            string typeDirectory = fileType switch
            {
                FileType.User => "Users",
                FileType.Export => "Exports",
                FileType.Temp => "Temp",
                FileType.List => "Lists",
                _ => "Other"
            };

            return Path.Combine(baseDirectory, typeDirectory, fileName);
        }

        /// <summary>
        /// Dosya var mı kontrol eder
        /// </summary>
        public static bool FileExists(string fileName, FileType fileType, string baseDirectory)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            string filePath = GetFilePath(fileName, fileType, baseDirectory);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Dosyayı siler
        /// </summary>
        public static bool DeleteFile(string fileName, FileType fileType, string baseDirectory)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            string filePath = GetFilePath(fileName, fileType, baseDirectory);
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Dosya bilgilerini getirir
        /// </summary>
        public static FileInfoDto? GetFileInfo(string fileName, FileType fileType, string baseDirectory)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            string filePath = GetFilePath(fileName, fileType, baseDirectory);
            if (!File.Exists(filePath))
                return null;

            try
            {
                var fileInfo = new FileInfo(filePath);
                return new FileInfoDto
                {
                    FileName = fileName,
                    FilePath = filePath,
                    Size = fileInfo.Length,
                    Extension = fileInfo.Extension,
                    CreatedAt = fileInfo.CreationTime,
                    ModifiedAt = fileInfo.LastWriteTime
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Eski temp dosyaları temizler (belirtilen saatten eski)
        /// </summary>
        public static int CleanOldTempFiles(string tempDirectory, int expirationHours = 24)
        {
            if (!Directory.Exists(tempDirectory))
                return 0;

            int deletedCount = 0;
            DateTime expirationTime = DateTime.Now.AddHours(-expirationHours);

            try
            {
                var tempFiles = Directory.GetFiles(tempDirectory, "UPLOAD_*");
                foreach (var filePath in tempFiles)
                {
                    var fileInfo = new FileInfo(filePath);
                    if (fileInfo.CreationTime < expirationTime)
                    {
                        try
                        {
                            File.Delete(filePath);
                            deletedCount++;
                        }
                        catch { }
                    }
                }
            }
            catch { }

            return deletedCount;
        }

        /// <summary>
        /// Dosya listesini getirir (belirli bir tip için)
        /// </summary>
        public static List<FileInfoDto> GetFileList(FileType fileType, string baseDirectory, int? limit = null)
        {
            string typeDirectory = fileType switch
            {
                FileType.User => "Users",
                FileType.Export => "Exports",
                FileType.Temp => "Temp",
                FileType.List => "Lists",
                _ => "Other"
            };

            string directoryPath = Path.Combine(baseDirectory, typeDirectory);
            if (!Directory.Exists(directoryPath))
                return new List<FileInfoDto>();

            var files = new List<FileInfoDto>();
            try
            {
                var filePaths = Directory.GetFiles(directoryPath);
                if (limit.HasValue)
                    filePaths = filePaths.Take(limit.Value).ToArray();

                foreach (var filePath in filePaths)
                {
                    var fileInfo = new FileInfo(filePath);
                    files.Add(new FileInfoDto
                    {
                        FileName = fileInfo.Name,
                        FilePath = filePath,
                        Size = fileInfo.Length,
                        Extension = fileInfo.Extension,
                        CreatedAt = fileInfo.CreationTime,
                        ModifiedAt = fileInfo.LastWriteTime
                    });
                }
            }
            catch { }

            return files;
        }
    }

    /// <summary>
    /// Dosya bilgisi DTO
    /// </summary>
    public class FileInfoDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long Size { get; set; }
        public string Extension { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}

