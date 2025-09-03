using DocFinder.Domain;

namespace DocFinder.Application.Commands;

public sealed record AddProtocolListItemCommand(ProtocolListItem Entity) : AddEntityCommand<ProtocolListItem>(Entity);
