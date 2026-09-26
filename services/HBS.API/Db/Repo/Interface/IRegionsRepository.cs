using HBS.API.Db.models;

namespace HBS.API.Db.Repo.Interface;

public interface IRegionsRepository
{
    void Add(Regions region);
    List<Regions> GetAll(int pageNumber, int pageSize);
    Regions? GetById(Guid id);
    void Update(Regions region);
    void Delete(Guid id);
}