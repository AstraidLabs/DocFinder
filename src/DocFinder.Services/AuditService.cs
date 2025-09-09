using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DocFinder.Domain;

namespace DocFinder.Services;

public interface IAuditService
{
    Task WriteAsync(IEnumerable<AuditEntry> entries, CancellationToken ct = default);
}

public sealed class AuditService : IAuditService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AuditService(IServiceScopeFactory scopeFactory)
        => _scopeFactory = scopeFactory;

    public async Task WriteAsync(IEnumerable<AuditEntry> entries, CancellationToken ct = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<DocumentDbContext>>();
        await using var db = await factory.CreateDbContextAsync(ct);
        await db.AddRangeAsync(entries, ct);
        await db.SaveChangesAsync(ct);
    }
}
