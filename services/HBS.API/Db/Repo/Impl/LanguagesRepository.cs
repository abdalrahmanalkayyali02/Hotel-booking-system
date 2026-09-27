using HBS.API.Db.models;
using HBS.API.Db.Repo.Interface;

namespace HBS.API.Db.Repo.Impl;

public class LanguagesRepository : ILanguagesRepository
{
    private readonly AppDbContext _context;
    public LanguagesRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Languages language)
    {
        _context.Languages.Add(language);
        _context.SaveChanges();
    }

    public List<Languages> GetAll(int pageNumber, int pageSize)
    {
        return _context.Languages
            .OrderBy(language => language.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public Languages? GetById(Guid id)
    {
        return _context.Languages.FirstOrDefault(language => language.Id == id);
    }

    public void Update(Languages language)
    {
        _context.Languages.Update(language);
        _context.SaveChanges();
    }

    public void Delete(Guid id)
    {
        var language = GetById(id);
        if (language is null)
        {
            return;
        }
        _context.Languages.Remove(language);
        _context.SaveChanges();
    }

    public Languages? GetLanguageByCode(string code)
    {
        return _context.Languages.FirstOrDefault(language => language.Code == code);
    }

}
