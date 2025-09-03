using DocFinder.Domain;

namespace DocFinder.Services;

public class ProtocolRepository : EfRepository<Protocol>, IProtocolRepository
{
    public ProtocolRepository(DocumentDbContext context) : base(context) { }
}
