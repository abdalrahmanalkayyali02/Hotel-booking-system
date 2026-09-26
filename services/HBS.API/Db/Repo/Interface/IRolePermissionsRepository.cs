using HBS.API.Db.models;

namespace HBS.API.Db.Repo.Interface;

public interface IRolePermissionsRepository
{
    void Add(RolePermissions rolePermission);
    List<RolePermissions> GetAll(int pageNumber, int pageSize);
    RolePermissions? GetById(Guid id);
    void Update(RolePermissions rolePermission);
    void Delete(Guid id);
}