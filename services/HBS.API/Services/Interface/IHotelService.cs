using HBS.API.Dtos.Hotels.ChoosePrimaryImage;
using HBS.API.Dtos.Hotels.CreateHotel;
using HBS.API.Dtos.Hotels.GetAll;
using HBS.API.Dtos.Hotels.GetById;
using HBS.API.Dtos.Hotels.SoftDelete;
using HBS.API.Dtos.Hotels.UpdateHotel;
using HBS.API.Dtos.Hotels.UploadHotelImages;
using HBS.API.Shared.Result;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace HBS.API.Services.Interface;

public interface IHotelService
{
  Task<Result<CreateHotelResponse>> CreateHotelAsync(CreateHotelRequest request, Guid currentUserId ,string currentUserRole);
  Task<Result<List<GetAllHotelsResponse>>> GetAllHotels(int pageNumber, int pageSize);
  Task<Result<GetHotelByIdResponse>> GetHotelByIdAsync(Guid hotelId);
  Task<Result<UpdateHotelResponse>> UpdateHotelAsync(Guid hotelId, UpdateHotelRequest request, Guid currentUserId, string currentUserRole);
  Task<Result<SoftDeleteHotelResponse>>  SoftDeleteHotelAsync(Guid hotelId, Guid currentUserId, string currentUserRole);
  Task<Result<UploadHotelImageResponse>> UploadHotelImageAsync(Guid hotelId, UploadHotelImagesRequest request, Guid currentUserId, string currentUserRole);
  Task<Result> DeleteHotelImageAsync(Guid imageId, Guid currentUserId, string currentUserRole);
  Task<Result> MakeImagePrimaryAsync(ChoosePrimaryImageRequest request,  Guid currentUserId, string currentUserRole);

}
