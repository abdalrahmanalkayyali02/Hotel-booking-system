using HBS.API.Db.models;

namespace HBS.API.Db.Repo.Interface;

public interface IHotelsRepository
{
  void Add(Hotels hotel);
  List<Hotels> GetAll(int  pageNumber, int pageSize, Guid languageId);
  Hotels? GetById(Guid hotelId, Guid languageId);
  Hotels? GetByIdNormalized(Guid hotelId);
}
