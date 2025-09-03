using DocFinder.Domain;

namespace DocFinder.Application.Commands;

public sealed record AddAuditEntryCommand(AuditEntry Entity) : AddEntityCommand<AuditEntry>(Entity);
