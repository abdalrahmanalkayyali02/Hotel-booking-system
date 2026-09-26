using HBS.API.Db.models;

namespace HBS.API.Db.Repo.Interface;

public interface IPermissionsRepository
{
    void Add(Permissions permission);
    List<Permissions> GetAll(int pageNumber, int pageSize);
    Permissions? GetById(Guid id);
    void Update(Permissions permission);
    void Delete(Guid id);
}