using Microsoft.AspNetCore.Components.Forms;

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

    public async Task<(bool success, string? filePath, string? errorMessage)> UploadFileAsync(IBrowserFile file, string subfolder = "files")
    {
        try
        {
            var maxFileSizeMB = _configuration.GetValue<int>("FileUpload:MaxFileSizeMB", 10);
            var maxFileSizeBytes = maxFileSizeMB * 1024 * 1024;

            if (file.Size > maxFileSizeBytes)
            {
                return (false, null, $"File size exceeds maximum allowed size of {maxFileSizeMB}MB");
            }

            var allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions").Get<string[]>() ?? Array.Empty<string>();
            var fileExtension = Path.GetExtension(file.Name).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return (false, null, $"File type {fileExtension} is not allowed");
            }

            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", subfolder);
            Directory.CreateDirectory(uploadPath);

            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            using (var stream = file.OpenReadStream(maxFileSizeBytes))
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }

            var relativePath = $"/uploads/{subfolder}/{uniqueFileName}";
            return (true, relativePath, null);
        }
        catch (Exception ex)
        {
            return (false, null, $"Error uploading file: {ex.Message}");
        }
    }

    public async Task<(bool success, string? filePath, string? errorMessage)> SaveBase64FileAsync(string base64Data, string fileName, string subfolder = "files")
    {
        try
        {
            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", subfolder);
            Directory.CreateDirectory(uploadPath);

            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            var base64Content = base64Data.Contains(",") ? base64Data.Split(',')[1] : base64Data;
            var fileBytes = Convert.FromBase64String(base64Content);

            await File.WriteAllBytesAsync(filePath, fileBytes);

            var relativePath = $"/uploads/{subfolder}/{uniqueFileName}";
            return (true, relativePath, null);
        }
        catch (Exception ex)
        {
            return (false, null, $"Error saving file: {ex.Message}");
        }
    }
}
