using HBS.API.Db.models;

namespace HBS.API.Db.Repo.Interface;

public interface IHotelImagesRepository
{
    void Add(HotelImages image);

    HotelImages? GetById(Guid imageId);

    List<HotelImages> GetByHotelId(Guid hotelId);

    void Delete(HotelImages image);
}
