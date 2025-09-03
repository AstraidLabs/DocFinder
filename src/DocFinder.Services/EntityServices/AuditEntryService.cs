using DocFinder.Domain;

namespace DocFinder.Services;

public class AuditEntryService : RepositoryService<AuditEntry>
{
    public AuditEntryService(IAuditEntryRepository repository) : base(repository) { }
}
