using DocFinder.Domain;

namespace DocFinder.Services;

public class ProtocolListItemRepository : EfRepository<ProtocolListItem>, IProtocolListItemRepository
{
    public ProtocolListItemRepository(DocumentDbContext context) : base(context) { }
}
