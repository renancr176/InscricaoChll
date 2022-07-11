using System.Linq.Expressions;
using InscricaoChll.Api.DbContexts.ChllDbContext.Entities;
using InscricaoChll.Api.DbContexts.ChllDbContext.Enums;

namespace InscricaoChll.Api.DbContexts.ChllDbContext.Interfaces.Repositories;

public interface IRepository<TEntity> : IDisposable where TEntity : Entity<TEntity>
{
    Task InsertAsync(TEntity obj);
    Task InsertRangeAsync(ICollection<TEntity> obj);
    Task<TEntity> GetByIdAsync(Guid id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> GetPagedAsync(int pageIndex, int pageSize,
        Expression<Func<TEntity, bool>> predicate = null,
        Dictionary<OrderByTypeEnum, Expression<Func<TEntity, object>>> ordenations = null);
    Task<IEnumerable<TEntity>> GetPagedAsync(int pageIndex, int pageSize,
        IEnumerable<string> includes, Expression<Func<TEntity, bool>> predicate = null,
        Dictionary<OrderByTypeEnum, Expression<Func<TEntity, object>>> ordenations = null);
    Task UpdateAsync(TEntity obj);
    Task UpdateRangeAsync(IEnumerable<TEntity> obj);
    Task DeleteAsync(Guid id);
    Task DeleteAsync(Expression<Func<TEntity, bool>> predicate);
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
    Task<bool> AllAsync(Expression<Func<TEntity, bool>> predicate);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
    Task<int> SaveChangesAsync();
}

public interface IRepositoryIntId<TEntity> : IRepository<TEntity> where TEntity : EntityIntId<TEntity>
{
    Task<TEntity> GetByIdAsync(long id);
    Task DeleteAsync(long id);
}