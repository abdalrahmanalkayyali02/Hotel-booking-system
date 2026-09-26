using HBS.API.Dtos.Hotels.CreateHotel;
using HBS.API.Shared.Result;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace HBS.API.Services.Interface;

public interface IHotelService
{
        Task<IResult<CreateHotelResponse>> CreateHotelAsync(CreateHotelRequest request, Guid currentUserId);

        Task<IResult> GetHotelsAsync(HotelFilterDto filter);

        Task<IResult> GetHotelByIdAsync(Guid hotelId);

        Task<IResult> UpdateHotelAsync(Guid hotelId, UpdateHotelDto request, Guid currentUserId);

        Task<IResult> DeactivateHotelAsync(Guid hotelId, Guid currentUserId);

        Task<IResult> AddHotelImageAsync(Guid hotelId, AddHotelImageDto request, Guid currentUserId);

        Task<IResult> RemoveHotelImageAsync(Guid hotelId, Guid imageId, Guid currentUserId);

        Task<IResult> SetPrimaryHotelImageAsync(Guid hotelId, Guid imageId, Guid currentUserId);
}