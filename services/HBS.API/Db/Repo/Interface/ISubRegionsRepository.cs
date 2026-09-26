using HBS.API.Db.models;

namespace HBS.API.Db.Repo.Interface;

public interface ISubRegionsRepository
{
    void Add(SubRegions subRegion);
    List<SubRegions> GetAll(int pageNumber, int pageSize);
    SubRegions? GetById(Guid id);
    void Update(SubRegions subRegion);
    void Delete(Guid id);
}
