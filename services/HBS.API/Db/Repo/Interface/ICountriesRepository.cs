using HBS.API.Db.models;

namespace HBS.API.Db.Repo.Interface;

public interface ICountriesRepository
{
    void Add(Countries country);
    List<Countries> GetAll(int pageNumber, int pageSize);
    Countries? GetById(Guid id);
    void Update(Countries country);
    void Delete(Guid id);
    string? GetName(Guid id, Guid languageId);
}