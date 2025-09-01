using System.Threading;
using System.Threading.Tasks;
using DocFinder.Application.Commands;
using DocFinder.Domain;

namespace DocFinder.Application.Handlers;

public sealed class DeleteDocumentHandler : ICommandHandler<DeleteDocumentCommand, Unit>
{
    private readonly ISearchService _search;
    public DeleteDocumentHandler(ISearchService search) => _search = search;

    public async Task<Unit> HandleAsync(DeleteDocumentCommand command, CancellationToken ct)
    {
        await _search.DeleteByFileIdAsync(command.FileId, ct);
        return new Unit();
    }
}
