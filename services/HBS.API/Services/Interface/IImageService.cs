using HBS.API.Dtos.Images.UploadImages;

namespace HBS.API.Services.Interface;

public interface IImageService
{
  Task<UploadImagesResponse> UploadImageAsync(IFormFile file);
  Task DeleteImageAsync(string publicId);
}
