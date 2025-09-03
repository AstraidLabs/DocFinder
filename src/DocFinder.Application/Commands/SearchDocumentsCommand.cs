using DocFinder.Domain;

namespace DocFinder.Application.Commands;

public sealed record SearchDocumentsCommand(string Query, SearchFilter Filter) : ICommand<SearchResult>;
