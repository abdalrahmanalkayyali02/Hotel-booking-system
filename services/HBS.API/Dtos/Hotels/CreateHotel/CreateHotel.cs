namespace HBS.API.Dtos.Hotels.CreateHotel;

public record CreateHotelRequest(
    string City,
    string Country, 
    float StarRating,
    string? PhoneNumberCountryCode,
    string? PhoneNumber, 
    string? Email,
    List<CreateHotelTranslationRequest> Translations
);
    
public record CreateHotelResponse(
    Guid HotelId,
    bool PendingApproval
);