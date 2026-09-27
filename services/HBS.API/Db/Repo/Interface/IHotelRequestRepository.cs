using HBS.API.Db.models;

namespace HBS.API.Db.Repo.Interface;

public interface IHotelRequestRepository
{
  void Add(HotelRequests hotelRequest);
}
