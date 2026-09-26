using HBS.API.Db.models;
using HBS.API.Db.Repo.Interface;

namespace HBS.API.Db.Repo.Impl;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<Users> GetAll(int pageNumber, int pageSize)
    {
        return _context.Users
            .OrderBy(user => user.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public Users? GetById(Guid id)
    {
        return _context.Users.FirstOrDefault(user => user.Id == id);
    }

    public Users? GetByPhoneNumber(string phoneNumberCountryCode, string phoneNumber)
    {

        return _context.Users.FirstOrDefault(user => user.PhoneNumberCountryCode == phoneNumberCountryCode
                                                     && user.PhoneNumber == phoneNumber);
    }

    public Users? GetByEmail(string email)
    {
        return _context.Users.FirstOrDefault(user => user.Email == email);
    }

    public void Add(Users user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public void Update(Users user)
    {
        _context.Users.Update(user);
        _context.SaveChanges();
    }

    public void Delete(Users user)
    {
        _context.Users.Remove(user);
        _context.SaveChanges();
    }

}