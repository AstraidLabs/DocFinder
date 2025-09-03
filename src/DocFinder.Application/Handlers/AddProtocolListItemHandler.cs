using DocFinder.Application.Commands;
using DocFinder.Domain;
using DocFinder.Services;

namespace DocFinder.Application.Handlers;

public sealed class AddProtocolListItemHandler : AddEntityHandler<ProtocolListItem>
{
    public AddProtocolListItemHandler(ProtocolListItemService service) : base(service) { }
}
