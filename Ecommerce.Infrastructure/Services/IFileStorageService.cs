using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string relativeFolder);
        Task DeleteFileAsync(string relativePath);
        string GetPublicUrl(string relativePath);
    }
}
