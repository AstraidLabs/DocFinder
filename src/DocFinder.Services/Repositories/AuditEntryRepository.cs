using DocFinder.Domain;

namespace DocFinder.Services;

public class AuditEntryRepository : EfRepository<AuditEntry>, IAuditEntryRepository
{
    public AuditEntryRepository(DocumentDbContext context) : base(context) { }
}
