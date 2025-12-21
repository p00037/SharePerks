using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Admin.Data.Repositories;

public abstract class GenericRepository<TEntity> where TEntity : class
{
    private readonly ApplicationDbContext _context;
    protected DbSet<TEntity> DbSet { get; }

    protected GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        DbSet = context.Set<TEntity>();
    }

    protected Task<List<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = DbSet;

        if (filter is not null)
        {
            query = query.Where(filter);
        }

        if (includes.Length > 0)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (orderBy is not null)
        {
            query = orderBy(query);
        }

        return query.AsNoTracking().ToListAsync();
    }

    protected virtual Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return DbSet.FindAsync(new[] { id }, cancellationToken).AsTask();
    }

    public virtual ValueTask<EntityEntry<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual void Update(TEntity entity)
    {
        DbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public virtual void Remove(TEntity entity)
    {
        DbSet.Remove(entity);

    }
}
