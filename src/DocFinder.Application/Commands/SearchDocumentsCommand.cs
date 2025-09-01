using DocFinder.Domain;

namespace DocFinder.Application.Commands;

public sealed record SearchDocumentsCommand(UserQuery Query) : ICommand<SearchResult>;
