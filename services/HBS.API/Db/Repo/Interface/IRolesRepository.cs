using HBS.API.Db.models;

namespace HBS.API.Db.Repo.Interface;

public interface IRolesRepository
{
    void Add(Roles role);
    List<Roles> GetAll(int pageNumber, int pageSize);
    Roles? GetById(Guid id);
    void Update(Roles role);
    void Delete(Guid id);
    string? GetName(Guid id, Guid languageId);
}