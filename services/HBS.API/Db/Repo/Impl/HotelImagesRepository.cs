using HBS.API.Db.models;

using HBS.API.Db.Repo.Interface;

namespace HBS.API.Db.Repo.Impl;

public class HotelImagesRepository : IHotelImagesRepository
{
  private readonly AppDbContext _context;

  public HotelImagesRepository(AppDbContext context)
  {
    _context = context;
  }

  public void Add(HotelImages image)
  {
    _context.HotelImages.Add(image);
  }

  public HotelImages? GetById(Guid imageId)
  {
    return _context.HotelImages
      .FirstOrDefault(image => image.Id == imageId);
  }

  public List<HotelImages> GetByHotelId(Guid hotelId)
  {
    return _context.HotelImages
      .Where(image => image.HotelId == hotelId)
      .ToList();
  }

  public void Delete(HotelImages image)
  {
    _context.HotelImages.Remove(image);
  }

  public HotelImages? GetPrimaryImageByHotelId(Guid hotelId)
  {
    return _context.HotelImages
      .FirstOrDefault(image => image.HotelId == hotelId &&
                               image.IsPrimary == true);
  }

  public void Update(HotelImages image)
  {
    _context.HotelImages.Update(image);
  }
}
