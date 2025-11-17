using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IHostEnvironment _env;

        public LocalFileStorageService(IHostEnvironment env)
        {
            _env = env;
        }

        // relativeFolder e.g. "uploads/products"
        public async Task<string> SaveFileAsync(IFormFile file, string relativeFolder)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (string.IsNullOrWhiteSpace(relativeFolder)) relativeFolder = "uploads";

            // determine physical web root (prefer IWebHostEnvironment.WebRootPath if available,
            // but using IHostEnvironment we compute wwwroot relative to current directory)
            var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            // target folder inside wwwroot
            var targetFolder = Path.Combine(webRoot, relativeFolder.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

            // unique file name (preserve extension)
            var ext = Path.GetExtension(file.FileName) ?? string.Empty;
            var fileName = $"{Guid.NewGuid():N}{ext}";

            // combine folder + filename
            var filePath = Path.Combine(targetFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // return relative path from wwwroot using forward slashes for URLs: "uploads/products/xxx.jpg"
            var relativePath = Path.Combine(relativeFolder, fileName).Replace('\\', '/').TrimStart('/');
            return relativePath;
        }

        public Task DeleteFileAsync(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return Task.CompletedTask;

            var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            // physical path to the file
            var physicalPath = Path.Combine(webRoot, relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }

            return Task.CompletedTask;
        }

        public string GetPublicUrl(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return string.Empty;
            return $"/{relativePath.TrimStart('/')}";
        }
    }
}
