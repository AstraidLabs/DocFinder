using DocFinder.Domain;

namespace DocFinder.Application.Commands;

public sealed record AddProtocolCommand(Protocol Entity) : AddEntityCommand<Protocol>(Entity);
