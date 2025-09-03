using DocFinder.Application.Commands;
using DocFinder.Domain;
using DocFinder.Services;

namespace DocFinder.Application.Handlers;

public sealed class AddAuditEntryHandler : AddEntityHandler<AuditEntry>
{
    public AddAuditEntryHandler(AuditEntryService service) : base(service) { }
}
