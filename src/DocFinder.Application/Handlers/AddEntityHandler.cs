using System.Threading;
using System.Threading.Tasks;
using DocFinder.Application.Commands;
using DocFinder.Services;

namespace DocFinder.Application.Handlers;

public class AddEntityHandler<T> : ICommandHandler<AddEntityCommand<T>, Unit> where T : class
{
    private readonly RepositoryService<T> _service;

    public AddEntityHandler(RepositoryService<T> service) => _service = service;

    public async Task<Unit> HandleAsync(AddEntityCommand<T> command, CancellationToken ct)
    {
        await _service.AddAsync(command.Entity, ct);
        return new Unit();
    }
}
