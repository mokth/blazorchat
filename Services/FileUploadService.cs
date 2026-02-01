using Microsoft.Extensions.Options;

namespace blazorchat.Services;

public class FileUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public FileUploadService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    public async Task<(bool success, string? filePath, string? error)> SaveFileAsync(string base64Data, string fileName, string fileType)
    {
        try
        {
            var maxFileSizeMB = _configuration.GetValue<int>("FileUpload:MaxFileSizeInMB", 10);
            var allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions").Get<string[]>() ?? Array.Empty<string>();

            // Extract file extension
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            // Validate extension
            if (allowedExtensions.Length > 0 && !allowedExtensions.Contains(extension))
            {
                return (false, null, $"File type {extension} is not allowed");
            }

            // Decode base64
            byte[] fileBytes;
            try
            {
                // Remove data URL prefix if present
                if (base64Data.Contains(","))
                {
                    base64Data = base64Data.Split(',')[1];
                }
                fileBytes = Convert.FromBase64String(base64Data);
            }
            catch
            {
                return (false, null, "Invalid file data");
            }

            // Validate file size
            var fileSizeInMB = fileBytes.Length / (1024.0 * 1024.0);
            if (fileSizeInMB > maxFileSizeMB)
            {
                return (false, null, $"File size exceeds maximum allowed size of {maxFileSizeMB}MB");
            }

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            // Save file
            await File.WriteAllBytesAsync(filePath, fileBytes);

            // Return relative URL
            var fileUrl = $"/uploads/{uniqueFileName}";
            return (true, fileUrl, null);
        }
        catch (Exception ex)
        {
            return (false, null, $"Error saving file: {ex.Message}");
        }
    }

    public bool DeleteFile(string fileUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl))
                return false;

            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
