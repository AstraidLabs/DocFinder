using DocFinder.Domain;

namespace DocFinder.Application.Commands;

public sealed record AddProtocolListCommand(ProtocolList Entity) : AddEntityCommand<ProtocolList>(Entity);
