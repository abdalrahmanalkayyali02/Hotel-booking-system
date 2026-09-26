using HBS.API.Db.models;
using HBS.API.Db.Repo.Interface;

namespace HBS.API.Db.Repo.Impl;

public class CitiesRepository : ICitiesRepository
{
    private readonly AppDbContext _context;
    public CitiesRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Cities city)
    {
        _context.Cities.Add(city);
        _context.SaveChanges();
    }

    public List<Cities> GetAll(int pageNumber, int pageSize)
    {
        return _context.Cities
            .OrderBy(city => city.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public Cities? GetById(Guid id)
    {
        return _context.Cities.FirstOrDefault(city => city.Id == id);
    }

    public void Update(Cities city)
    {
        _context.Cities.Update(city);
        _context.SaveChanges();
    }

    public void Delete(Guid id)
    {
        var city = GetById(id);

        if (city is null)
        {
            return;
        }
        _context.Cities.Remove(city);
        _context.SaveChanges();
    }

    public bool CityBelongsToCountry(Guid countryId, Guid cityId)
    {
        return _context.Cities.Any(city => city.Id == cityId && city.CountryId == countryId);
    }
    
    public string? GetName(Guid id, Guid languageId)
    {
        return _context.CitiesTranslations
            .Where(translation => translation.CityId == id && translation.LanguageId == languageId)
            .Select(translation => translation.Name)
            .FirstOrDefault();
    }
}