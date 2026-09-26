using HBS.API.Db.models;
using HBS.API.Db.Repo.Interface;

namespace HBS.API.Db.Repo.Impl;

public class PermissionsRepository : IPermissionsRepository
{
    private readonly AppDbContext _context;
    public PermissionsRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Permissions permission)
    {
        _context.Permissions.Add(permission);
        _context.SaveChanges();
    }

    public List<Permissions> GetAll(int pageNumber, int pageSize)
    {
        return _context.Permissions
            .OrderBy(permission => permission.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public Permissions? GetById(Guid id)
    {
        return _context.Permissions.FirstOrDefault(permission => permission.Id == id);
    }

    public void Update(Permissions permission)
    {
        _context.Permissions.Update(permission);
        _context.SaveChanges();
    }

    public void Delete(Guid id)
    {
        var permission = GetById(id);
        if (permission is null)
        {
            return;
        }
        _context.Permissions.Remove(permission);
        _context.SaveChanges();
    }
}