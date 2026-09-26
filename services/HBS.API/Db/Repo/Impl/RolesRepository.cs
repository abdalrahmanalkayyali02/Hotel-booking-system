using HBS.API.Db.models;
using HBS.API.Db.Repo.Interface;

namespace HBS.API.Db.Repo.Impl;

public class RolesRepository : IRolesRepository
{
    private readonly AppDbContext _context;

    public RolesRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Roles role)
    {
        _context.Roles.Add(role);
        _context.SaveChanges();
    }

    public List<Roles> GetAll(int pageNumber, int pageSize)
    {
        return _context.Roles
            .OrderBy(role => role.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public Roles? GetById(Guid id)
    {
        return _context.Roles.FirstOrDefault(role => role.Id == id);
    }

    public void Update(Roles role)
    {
        _context.Roles.Update(role);
        _context.SaveChanges();
    }

    public void Delete(Guid id)
    {
        var role = GetById(id);
        if (role is null)
        {
            return;
        }
        _context.Roles.Remove(role);
        _context.SaveChanges();
    }

    public string? GetName(Guid id, Guid languageId)
    {
        return _context.RoleTranslations
            .Where(translation => translation.RoleId == id && translation.LanguageId == languageId)
            .Select(translation => translation.Name)
            .FirstOrDefault();
    }
}