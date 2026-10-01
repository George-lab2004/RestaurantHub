namespace RestaurantHub.Services
{
    public class FileService : IFileService
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" }; //so we dojt accept any file to  avoid attacks from exe html etc
        private const long MaxSizeBytes = 2 * 1024 * 1024; // 2 MB to limit the size of the image to avoid attacks from large files

        private readonly IWebHostEnvironment _env;

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveImageAsync(IFormFile file, string folder)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
                throw new InvalidOperationException("Only .jpg, .jpeg, .png and .webp images are allowed.");

            if (file.Length > MaxSizeBytes)
                throw new InvalidOperationException("Image must be 2 MB or smaller.");

            var folderPath = Path.Combine(_env.WebRootPath, "images", folder);
            Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}{extension}"; // Generate a unique file name to avoid collisions
            var fullPath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/{folder}/{fileName}";
        }

        public void DeleteImage(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath) || !relativePath.StartsWith("/images/"))
                return;

            var fullPath = Path.Combine(
                _env.WebRootPath,
                relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}