using System;

namespace DocFinder.Application.Commands;

public sealed record DeleteDocumentCommand(Guid FileId) : ICommand<Unit>;
