using DocFinder.Application.Commands;
using DocFinder.Domain;
using DocFinder.Services;

namespace DocFinder.Application.Handlers;

public sealed class AddProtocolListHandler : AddEntityHandler<ProtocolList>
{
    public AddProtocolListHandler(ProtocolListService service) : base(service) { }
}
