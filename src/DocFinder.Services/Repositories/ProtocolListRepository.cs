using DocFinder.Domain;

namespace DocFinder.Services;

public class ProtocolListRepository : EfRepository<ProtocolList>, IProtocolListRepository
{
    public ProtocolListRepository(DocumentDbContext context) : base(context) { }
}
