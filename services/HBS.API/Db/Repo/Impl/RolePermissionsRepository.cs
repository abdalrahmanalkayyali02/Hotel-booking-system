using HBS.API.Db.models;
using HBS.API.Db.Repo.Interface;

namespace HBS.API.Db.Repo.Impl;

public class RolePermissionsRepository : IRolePermissionsRepository
{
    private readonly AppDbContext _context;

    public RolePermissionsRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(RolePermissions rolePermission)
    {
        _context.RolePermissions.Add(rolePermission);
        _context.SaveChanges();
    }

    public List<RolePermissions> GetAll(int pageNumber, int pageSize)
    {
        return _context.RolePermissions
            .OrderBy(rolePermission => rolePermission.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public RolePermissions? GetById(Guid id)
    {
        return _context.RolePermissions.FirstOrDefault(rolePermission => rolePermission.Id == id);
    }

    public void Update(RolePermissions rolePermission)
    {
        _context.RolePermissions.Update(rolePermission);
        _context.SaveChanges();
    }

    public void Delete(Guid id)
    {
        var rolePermission = GetById(id);
        if (rolePermission is null)
        {
            return;
        }
        _context.RolePermissions.Remove(rolePermission);
        _context.SaveChanges();
    }
}