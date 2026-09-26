using HBS.API.Db.models;
using HBS.API.Db.Repo.Interface;

namespace HBS.API.Db.Repo.Impl;

public class StateRepository : IStateRepository
{
    private readonly AppDbContext _context;
    public StateRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(States state)
    {
        _context.States.Add(state);
        _context.SaveChanges();
    }

    public List<States> GetAll(int pageNumber, int pageSize)
    {
        return _context.States
            .OrderBy(state => state.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public States? GetById(Guid id)
    {
        return _context.States.FirstOrDefault(state => state.Id == id);
    }

    public void Update(States state)
    {
        _context.States.Update(state);
        _context.SaveChanges();
    }

    public void Delete(Guid id)
    {
        var state = GetById(id);
        if (state is null)
        {
            return;
        }
        _context.States.Remove(state);
        _context.SaveChanges();
    }
}