using HBS.API.Db.models;
using HBS.API.Db.Repo.Interface;
using Microsoft.EntityFrameworkCore;

namespace HBS.API.Db.Repo.Impl;

public class HotelRepository : IHotelsRepository
{
  private readonly AppDbContext _context;
  public HotelRepository(AppDbContext context)
  {
    _context = context;
  }

  public void Add(Hotels hotel)
  {
    _context.Hotels.Add(hotel);
  }

  public List<Hotels> GetAll(int pageNumber, int pageSize, Guid languageId)
  {
    return _context.Hotels
      .Include(hotel=>hotel.Translations
        .Where(translation=>translation.LanguageId==languageId))
      .OrderBy(hotel => hotel.Id)
      .Skip((pageNumber - 1) * pageSize)
      .Take(pageSize)
      .ToList();
  }

  public Hotels? GetById(Guid hotelId, Guid languageId)
  {
    return _context.Hotels
      .Include(hotel => hotel.Translations
        .Where(translation => translation.LanguageId == languageId))
      .FirstOrDefault(hotel => hotel.Id == hotelId);
  }

  public Hotels? GetByIdNormalized(Guid hotelId)
  {
    return _context.Hotels.FirstOrDefault(hotel => hotel.Id == hotelId);
  }
}
