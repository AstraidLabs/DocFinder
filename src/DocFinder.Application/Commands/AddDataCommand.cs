using DocFinder.Domain;

namespace DocFinder.Application.Commands;

public sealed record AddDataCommand(Data Entity) : AddEntityCommand<Data>(Entity);
