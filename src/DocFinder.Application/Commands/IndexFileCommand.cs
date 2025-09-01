namespace DocFinder.Application.Commands;

public sealed record IndexFileCommand(string Path) : ICommand<Unit>;
