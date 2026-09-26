using HBS.API.Db.models;
using HBS.API.Db.Repo.Interface;
using HBS.API.Shared.enums;

namespace HBS.API.Db.Repo.Impl;

public class OtpRepository : IOtpRepository
{
    private readonly AppDbContext _context;
    public OtpRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Otp otp)
    {
        _context.Otp.Add(otp);
        _context.SaveChanges();
    }

    public List<Otp> GetAll(int pageNumber, int pageSize)
    {
        return _context.Otp
            .OrderBy(otp => otp.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public Otp? GetById(Guid id)
    {
        return _context.Otp.FirstOrDefault(otp => otp.Id == id);
    }

    public void Update(Otp otp)
    {
        _context.Otp.Update(otp);
        _context.SaveChanges();
    }

    public void Delete(Guid id)
    {
        var otp = GetById(id);

        if (otp is null)
        {
            return;
        }
        _context.Otp.Remove(otp);
        _context.SaveChanges();
    }

    public Otp? GetLatestRegistrationOtp(Guid userId)
    {
        return _context.Otp
            .Where(otp => otp.UserId == userId &&
                          otp.Type == OtpType.Registration &&
                          otp.Target == OtpTarget.Email)
            .OrderByDescending(otp => otp.GeneratedAt)
            .FirstOrDefault();
    }

    public Otp? GetLatestForgotPasswordOtp(Guid userId)
    {
        return _context.Otp
            .Where(otp =>
                otp.UserId == userId &&
                otp.Type == OtpType.ForgotPassword &&
                otp.Target == OtpTarget.Email)
            .OrderByDescending(otp => otp.GeneratedAt)
            .FirstOrDefault();
    }
}