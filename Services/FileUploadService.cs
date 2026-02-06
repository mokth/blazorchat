using blazorchat.Models;

namespace blazorchat.Services;

public class FileUploadService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public FileUploadService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public async Task<string> SaveFileAsync(string base64Data, string fileName, MessageType messageType)
    {
        try
        {
            // Sanitize filename to prevent path traversal attacks
            fileName = Path.GetFileName(fileName);
            
            var configuredPath = _configuration.GetValue<string>("FileUpload:UploadPath") ?? "wwwroot/uploads";
            var uploadPath = Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(_environment.ContentRootPath, configuredPath);
            
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileBytes = ParseBase64Payload(base64Data);
            if (!ValidateFile(fileName, fileBytes.LongLength, messageType))
            {
                throw new InvalidOperationException("File validation failed.");
            }
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            await File.WriteAllBytesAsync(filePath, fileBytes);

            return $"/uploads/{uniqueFileName}";
        }
        catch (Exception ex)
        {
            throw new Exception($"Error saving file: {ex.Message}");
        }
    }

    public bool ValidateFile(string fileName, long fileSize, MessageType messageType)
    {
        var maxSizeBytes = GetMaxSizeBytes(messageType);

        if (fileSize > maxSizeBytes)
        {
            return false;
        }

        var allowedExtensions = GetAllowedExtensions(messageType);
        var extension = Path.GetExtension(fileName).ToLower();

        return allowedExtensions?.Contains(extension) ?? false;
    }

    public void DeleteFileIfExists(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl) || !fileUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var configuredPath = _configuration.GetValue<string>("FileUpload:UploadPath") ?? "wwwroot/uploads";
        var uploadsRoot = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(_environment.ContentRootPath, configuredPath);
        var targetPath = Path.Combine(uploadsRoot, Path.GetFileName(relativePath));

        if (File.Exists(targetPath))
        {
            File.Delete(targetPath);
        }
    }

    private static byte[] ParseBase64Payload(string base64Data)
    {
        if (base64Data.Contains(","))
        {
            base64Data = base64Data.Split(',')[1];
        }

        return Convert.FromBase64String(base64Data);
    }

    private long GetMaxSizeBytes(MessageType messageType)
    {
        var maxSizeMb = messageType switch
        {
            MessageType.Image => _configuration.GetValue<int>("FileUpload:MaxImageSizeMB"),
            MessageType.Voice => _configuration.GetValue<int>("FileUpload:MaxAudioSizeMB"),
            MessageType.File => _configuration.GetValue<int>("FileUpload:MaxFileSizeMB"),
            _ => _configuration.GetValue<int>("FileUpload:MaxFileSizeMB")
        };

        return maxSizeMb * 1024L * 1024L;
    }

    private string[]? GetAllowedExtensions(MessageType messageType)
    {
        return messageType switch
        {
            MessageType.Image => _configuration.GetSection("FileUpload:AllowedImageExtensions").Get<string[]>(),
            MessageType.Voice => _configuration.GetSection("FileUpload:AllowedAudioExtensions").Get<string[]>(),
            MessageType.File => _configuration.GetSection("FileUpload:AllowedFileExtensions").Get<string[]>(),
            _ => _configuration.GetSection("FileUpload:AllowedFileExtensions").Get<string[]>()
        };
    }
}
