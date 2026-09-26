using HBS.API.Db.Repo.Interface;
using HBS.API.Dtos.Hotels.CreateHotel;
using HBS.API.Services.Interface;
using HBS.API.Shared.Result;

namespace HBS.API.Services.Impl;

public class HotelService : IHotelService
{
    private readonly IHotelsRepository _hotelRepository;
    private readonly IHotelRequestRepository _hotelRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHotelsTranslationRepository _hotelTranslationRepository;
    private readonly ILanguagesRepository _languagesRepository;

    public HotelService(
        IHotelsRepository hotelRepository,
        IHotelRequestRepository hotelRequestRepository,
        IUserRepository userRepository,
        IHotelsTranslationRepository hotelTranslationRepository,
        ILanguagesRepository languagesRepository)
    {
        _hotelRepository = hotelRepository;
        _hotelRequestRepository = hotelRequestRepository;
        _userRepository = userRepository;
        _hotelTranslationRepository = hotelTranslationRepository;
        _languagesRepository = languagesRepository;
    }

    public async Task<IResult<CreateHotelResponse>> CreateHotelAsync(CreateHotelRequest request, Guid currentUserId)
    {
        
    }
    
}