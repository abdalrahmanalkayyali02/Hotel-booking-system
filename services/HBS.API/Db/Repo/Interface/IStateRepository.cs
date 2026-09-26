using HBS.API.Db.models;

namespace HBS.API.Db.Repo.Interface;

public interface IStateRepository
{
    void Add(States state);
    List<States> GetAll(int pageNumber, int pageSize);
    States? GetById(Guid id);
    void Update(States state);
    void Delete(Guid id);
}