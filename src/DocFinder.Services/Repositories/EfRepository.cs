using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DocFinder.Domain;

namespace DocFinder.Services;

/// <summary>
/// Generic EF Core based repository implementing basic CRUD operations
/// with filtering, sorting and transaction support.
/// </summary>
public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly DocumentDbContext Context;

    public EfRepository(DocumentDbContext context)
    {
        Context = context;
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await Context.Set<T>().FirstOrDefaultAsync(predicate, ct);

    public async Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken ct = default)
    {
        IQueryable<T> query = Context.Set<T>();
        if (filter != null) query = query.Where(filter);
        if (orderBy != null) query = orderBy(query);
        return await query.ToListAsync(ct);
    }

    public async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await Context.Set<T>().AddAsync(entity, ct);
        await Context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        Context.Set<T>().Update(entity);
        await Context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        Context.Set<T>().Remove(entity);
        await Context.SaveChangesAsync(ct);
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync(ct);
        try
        {
            await operation(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
