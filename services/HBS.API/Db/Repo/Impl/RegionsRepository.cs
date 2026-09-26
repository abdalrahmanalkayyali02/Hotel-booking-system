using HBS.API.Db.models;
using HBS.API.Db.Repo.Interface;

namespace HBS.API.Db.Repo.Impl;

public class RegionsRepository : IRegionsRepository
{
    private readonly AppDbContext _context;
    public RegionsRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Regions region)
    {
        _context.Regions.Add(region);
        _context.SaveChanges();
    }

    public List<Regions> GetAll(int pageNumber, int pageSize)
    {
        return _context.Regions
            .OrderBy(region => region.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public Regions? GetById(Guid id)
    {
        return _context.Regions.FirstOrDefault(region => region.Id == id);
    }

    public void Update(Regions region)
    {
        _context.Regions.Update(region);
        _context.SaveChanges();
    }

    public void Delete(Guid id)
    {
        var region = GetById(id);
        if (region is null)
        {
            return;
        }
        _context.Regions.Remove(region);
        _context.SaveChanges();
    }
}