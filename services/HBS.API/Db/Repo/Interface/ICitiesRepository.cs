using HBS.API.Db.models;

namespace HBS.API.Db.Repo.Interface;

public interface ICitiesRepository
{
    void Add(Cities city);
    List<Cities> GetAll(int pageNumber, int pageSize);
    Cities? GetById(Guid id);
    void Update(Cities city);
    void Delete(Guid id);
    bool CityBelongsToCountry(Guid countryId, Guid cityId);
    string? GetName(Guid id, Guid languageId);

}