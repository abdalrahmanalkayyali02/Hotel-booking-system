using HBS.API.Db.models;

namespace HBS.API.Db.Repo.Interface;

public interface ILanguagesRepository
{
    void Add(Languages language);
    List<Languages> GetAll(int pageNumber, int pageSize);
    Languages? GetById(Guid id);
    void Update(Languages language);
    void Delete(Guid id);
    Languages? GetLanguageByCode(string code);
}