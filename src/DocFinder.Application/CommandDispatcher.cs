using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using DocFinder.Application.Commands;

namespace DocFinder.Application;

public sealed class CommandDispatcher
{
    private readonly IServiceProvider _provider;
    public CommandDispatcher(IServiceProvider provider) => _provider = provider;

    public Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken ct)
        where TCommand : ICommand<TResult>
    {
        var handler = _provider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        return handler.HandleAsync(command, ct);
    }
}
