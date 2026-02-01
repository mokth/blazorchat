using Microsoft.Extensions.Options;

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

    public async Task<string> SaveFileAsync(string base64Data, string fileName)
    {
        try
        {
            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads");
            
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Remove data URL prefix if present
            if (base64Data.Contains(","))
            {
                base64Data = base64Data.Split(',')[1];
            }

            var fileBytes = Convert.FromBase64String(base64Data);
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

    public bool ValidateFile(string fileName, long fileSize)
    {
        var maxSizeMB = _configuration.GetValue<int>("FileUpload:MaxFileSizeMB");
        var maxSizeBytes = maxSizeMB * 1024 * 1024;

        if (fileSize > maxSizeBytes)
        {
            return false;
        }

        var allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions").Get<string[]>();
        var extension = Path.GetExtension(fileName).ToLower();

        return allowedExtensions?.Contains(extension) ?? false;
    }
}
