using System.Threading;
using System.Threading.Tasks;

namespace DocFinder.Application.Commands;

public interface ICommand<TResult> { }

public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken ct);
}

public readonly record struct Unit;
