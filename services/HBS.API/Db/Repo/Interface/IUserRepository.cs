using HBS.API.Db.models;

namespace HBS.API.Db.Repo.Interface;

public interface IUserRepository
{
    void Add(Users user);
    List<Users> GetAll(int pageNumber, int pageSize);
    Users? GetById(Guid id);
    Users? GetByEmail(string email);
    Users? GetByPhoneNumber(string phoneNumberDialCode, string phoneNumber);
    void Update(Users user);
    void Delete(Users user);
}