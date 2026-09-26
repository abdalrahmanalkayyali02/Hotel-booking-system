using HBS.API.Db.models;
using HBS.API.Db.Repo.Interface;

namespace HBS.API.Db.Repo.Impl;

public class SubRegionsRepository : ISubRegionsRepository
{
    private readonly AppDbContext _context;

    public SubRegionsRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(SubRegions subRegion)
    {
        _context.SubRegions.Add(subRegion);
        _context.SaveChanges();
    }

    public List<SubRegions> GetAll(int pageNumber, int pageSize)
    {
        return _context.SubRegions
            .OrderBy(subRegion => subRegion.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public SubRegions? GetById(Guid id)
    {
        return _context.SubRegions.FirstOrDefault(subRegion => subRegion.Id == id);
    }

    public void Update(SubRegions subRegion)
    {
        _context.SubRegions.Update(subRegion);
        _context.SaveChanges();
    }

    public void Delete(Guid id)
    {
        var subRegion = GetById(id);
        if (subRegion is null)
        {
            return;
        }

        _context.SubRegions.Remove(subRegion);
        _context.SaveChanges();
    }
}
