using HBS.API.Db.models;
using HBS.API.Db.Repo.Interface;

namespace HBS.API.Db.Repo.Impl;

public class HotelsTranslationRepository : IHotelsTranslationRepository
{
  private readonly AppDbContext  _context;
  public HotelsTranslationRepository(AppDbContext context)
  {
    _context = context;
  }

  public void Add(HotelsTranslation hotelTranslation)
  {
    _context.HotelsTranslations.Add(hotelTranslation);
  }

  public HotelsTranslation? GetByHotelIdAndLanguage(Guid hotelId, Guid languageId)
  {
    return _context.HotelsTranslations.FirstOrDefault(hotel => hotel.HotelId == hotelId &&
                                                               hotel.LanguageId == languageId);
  }

}
