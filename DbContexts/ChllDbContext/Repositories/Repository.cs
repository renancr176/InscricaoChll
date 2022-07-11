using System.Linq.Expressions;
using InscricaoChll.Api.DbContexts.ChllDbContext.Entities;
using InscricaoChll.Api.DbContexts.ChllDbContext.Enums;
using InscricaoChll.Api.DbContexts.ChllDbContext.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InscricaoChll.Api.DbContexts.ChllDbContext.Repositories;

public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity<TEntity>
{
    protected DbContext Db;
    protected DbSet<TEntity> DbSet;
    protected IQueryable<TEntity> BaseQuery => DbSet.AsNoTracking().Where(e => e.DeletedAt == null);

    protected Repository(DbContext context)
    {
        Db = context;
        DbSet = Db.Set<TEntity>();
    }

    public virtual async Task InsertAsync(TEntity obj)
    {
        await DbSet.AddAsync(obj);
    }

    public async Task InsertRangeAsync(ICollection<TEntity> obj)
    {
        await DbSet.AddRangeAsync(obj);
    }

    public virtual async Task UpdateAsync(TEntity obj)
    {
        obj.UpdatedAt = DateTime.UtcNow;
        DbSet.Update(obj);
    }

    public async Task UpdateRangeAsync(IEnumerable<TEntity> obj)
    {
        foreach (var entity in obj)
        {
            entity.UpdatedAt = DateTime.UtcNow;
        }
        DbSet.UpdateRange(obj);
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await BaseQuery.Where(predicate).ToListAsync();
    }

    public virtual async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await BaseQuery.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await BaseQuery.AnyAsync(predicate);
    }

    public virtual async Task<bool> AllAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await BaseQuery.AllAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await BaseQuery.CountAsync(predicate);
    }

    public virtual async Task<TEntity> GetByIdAsync(Guid id)
    {
        return await BaseQuery.FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await BaseQuery.ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> GetPagedAsync(int pageIndex, int pageSize,
        Expression<Func<TEntity, bool>> predicate = null,
        Dictionary<OrderByTypeEnum, Expression<Func<TEntity, object>>> ordenations = null)
    {
        return await GetPagedAsync(pageIndex, pageSize, null, predicate, ordenations);
    }

    public virtual async Task<IEnumerable<TEntity>> GetPagedAsync(int pageIndex, int pageSize,
        IEnumerable<string> includes, Expression<Func<TEntity, bool>> predicate = null,
        Dictionary<OrderByTypeEnum, Expression<Func<TEntity, object>>> ordenations = null)
    {
        var query = BaseQuery
            .Where(predicate ?? (entity => true));

        if (ordenations != null && ordenations.Any())
        {
            query = (IOrderedQueryable<TEntity>)query;
            var firstOrder = true;
            foreach (var orderBy in ordenations)
            {
                switch (orderBy.Key)
                {
                    case OrderByTypeEnum.Ascending:
                        if (firstOrder)
                        {
                            query = query.OrderBy(orderBy.Value);
                        }
                        else
                        {
                            query = ((IOrderedQueryable<TEntity>)query).ThenBy(orderBy.Value);
                        }

                        break;
                    case OrderByTypeEnum.Descending:
                        if (firstOrder)
                        {
                            query = query.OrderByDescending(orderBy.Value);
                        }
                        else
                        {
                            query = ((IOrderedQueryable<TEntity>)query).ThenByDescending(orderBy.Value);
                        }
                        break;
                }

                firstOrder = false;
            }
        }

        if (includes != null && includes.Any())
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        query = query.Skip((pageIndex - 1) * pageSize)
            .Take(pageSize);

        return await query
            .ToListAsync();
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await DbSet.FindAsync(id);
        entity.DeletedAt = DateTime.UtcNow;
        DbSet.Update(entity);
    }

    public virtual async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
    {
        var entities = await BaseQuery.Where(predicate)
            .ToListAsync();
        entities.ForEach(entity => entity.DeletedAt = DateTime.UtcNow);
        DbSet.UpdateRange(entities);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await Db.SaveChangesAsync();
    }

    public void Dispose()
    {
        Db.Dispose();
    }
}

public abstract class RepositoryIntId<TEntity> : Repository<TEntity>, IRepositoryIntId<TEntity> where TEntity : EntityIntId<TEntity>
{
    protected RepositoryIntId(DbContext context)
        : base(context)
    {
    }

    public virtual async Task<TEntity> GetByIdAsync(long id)
    {
        return await BaseQuery.FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual async Task DeleteAsync(long id)
    {
        var entity = DbSet.Find(id);
        entity.DeletedAt = DateTime.UtcNow;
        DbSet.Update(entity);
    }
}