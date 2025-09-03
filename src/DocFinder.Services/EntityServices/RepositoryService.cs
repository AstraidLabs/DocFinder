using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DocFinder.Domain;

namespace DocFinder.Services;

public class RepositoryService<T> where T : class
{
    private readonly IRepository<T> _repository;

    public RepositoryService(IRepository<T> repository) => _repository = repository;

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => _repository.FirstOrDefaultAsync(predicate, ct);

    public Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken ct = default)
        => _repository.ListAsync(filter, orderBy, ct);

    public Task AddAsync(T entity, CancellationToken ct = default)
        => _repository.AddAsync(entity, ct);

    public Task UpdateAsync(T entity, CancellationToken ct = default)
        => _repository.UpdateAsync(entity, ct);

    public Task DeleteAsync(T entity, CancellationToken ct = default)
        => _repository.DeleteAsync(entity, ct);

    public Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default)
        => _repository.ExecuteInTransactionAsync(operation, ct);
}
