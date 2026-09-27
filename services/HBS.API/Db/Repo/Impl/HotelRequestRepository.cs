using HBS.API.Db.models;
using HBS.API.Db.Repo.Interface;

namespace HBS.API.Db.Repo.Impl;

public class HotelRequestRepository : IHotelRequestRepository
{
    private readonly AppDbContext  _context;
    public HotelRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(HotelRequests hotelRequest)
    {
      _context.HotelRequests.Add(hotelRequest);
    }
}
