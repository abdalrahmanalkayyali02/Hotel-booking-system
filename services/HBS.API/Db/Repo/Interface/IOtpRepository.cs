using HBS.API.Db.models;

namespace HBS.API.Db.Repo.Interface;

public interface IOtpRepository
{
    void Add(Otp otp);
    List<Otp> GetAll(int pageNumber, int pageSize);
    Otp? GetById(Guid id);
    void Update(Otp otp);
    void Delete(Guid id);
    Otp? GetLatestRegistrationOtp(Guid userId);
    Otp? GetLatestForgotPasswordOtp(Guid userId);
}