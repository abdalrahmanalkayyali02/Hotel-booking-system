using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HBS.API.Dtos.Images.UploadImages;
using HBS.API.Services.Interface;

namespace HBS.API.Services.Impl;

public class ImageService : IImageService
{
  private readonly Cloudinary _cloudinary;

  public ImageService(Cloudinary cloudinary)
  {
    _cloudinary = cloudinary;
  }

  public async Task<UploadImagesResponse> UploadImageAsync(IFormFile file)
  {
    if (file is null || file.Length == 0)
    {
      throw new ArgumentException("Image file is required.");
    }

    if (!file.ContentType.StartsWith("image/"))
    {
      throw new ArgumentException("Only image files are allowed.");
    }

    const long maxFileSize = 10 * 1024 * 1024;

    if (file.Length > maxFileSize)
    {
      throw new ArgumentException("Image file cannot exceed 10 MB.");
    }

    await using var stream = file.OpenReadStream();

    var uploadParams = new ImageUploadParams
    {
      File = new FileDescription(file.FileName, stream)
    };

    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

    if (uploadResult.Error is not null)
    {
      throw new InvalidOperationException(
        $"Cloudinary upload failed: {uploadResult.Error.Message}"
      );
    }

    return new UploadImagesResponse(
      uploadResult.PublicId,
      uploadResult.SecureUrl.ToString()
    );
  }

  public async Task DeleteImageAsync(string publicId)
  {
    if (string.IsNullOrWhiteSpace(publicId))
    {
      throw new ArgumentException("Public ID is required.");
    }

    var deletionParams = new DeletionParams(publicId);

    var result = await _cloudinary.DestroyAsync(deletionParams);

    if (result.Error is not null)
    {
      throw new InvalidOperationException(
        $"Cloudinary deletion failed: {result.Error.Message}"
      );
    }
  }
}
