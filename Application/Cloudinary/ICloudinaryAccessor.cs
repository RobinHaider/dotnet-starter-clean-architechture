using Microsoft.AspNetCore.Http;

namespace Application.Cloudinary
{
    public interface ICloudinaryAccessor
    {
        Task<CloudinaryUploadResult> AddPhoto(IFormFile file);
        Task<string> DeletePhoto(string publicId);
    }

    public class CloudinaryUploadResult
    {
        public string PublicId { get; set; }
        public string Url { get; set; }
    }
}