using HBS.API.Db.models;
using HBS.API.Db.Repo.Interface;

namespace HBS.API.Db.Repo.Impl;

public class CountriesRepository : ICountriesRepository
{
    private readonly AppDbContext _context;
    public CountriesRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Countries Country)
    {
        _context.Countries.Add(Country);
        _context.SaveChanges();
    }

    public List<Countries> GetAll(int pageNumber, int pageSize)
    {
        return _context.Countries
            .OrderBy(country => country.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public Countries? GetById(Guid id)
    {
        return _context.Countries.FirstOrDefault(country => country.Id == id);
    }

    public void Update(Countries country)
    {
        _context.Countries.Update(country);
        _context.SaveChanges();
    }

    public void Delete(Guid id)
    {
        var country = GetById(id);

        if (country is null)
        {
            return;
        }
        _context.Countries.Remove(country);
        _context.SaveChanges();
    }
    
    public string? GetName(Guid id, Guid languageId)
    {
        return _context.CountriesTranslations
            .Where(translation => translation.CountryId == id && translation.LanguageId == languageId)
            .Select(translation => translation.Name)
            .FirstOrDefault();
    }
}