using HBS.API.Db.models;

namespace HBS.API.Db.Repo.Interface;

public interface IHotelsTranslationRepository
{
  void Add(HotelsTranslation hotelTranslation);
  HotelsTranslation? GetByHotelIdAndLanguage(Guid hotelId, Guid languageId);

}
