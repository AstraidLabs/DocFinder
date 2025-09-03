namespace DocFinder.Application.Commands;

public record AddEntityCommand<T>(T Entity) : ICommand<Unit> where T : class;
