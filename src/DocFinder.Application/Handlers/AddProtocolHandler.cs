using DocFinder.Application.Commands;
using DocFinder.Domain;
using DocFinder.Services;

namespace DocFinder.Application.Handlers;

public sealed class AddProtocolHandler : AddEntityHandler<Protocol>
{
    public AddProtocolHandler(ProtocolService service) : base(service) { }
}
