namespace HBS.API.Db.UnitOfWork.Interface;

public interface IUnitOfWork
{
  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
