using HBS.API.Db.UnitOfWork.Interface;

namespace HBS.API.Db.UnitOfWork.Impl;

public class UnitOfWork : IUnitOfWork
{
  private readonly AppDbContext _context;

  public UnitOfWork(AppDbContext context)
  {
    _context = context;
  }

  public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    return _context.SaveChangesAsync(cancellationToken);
  }

}
