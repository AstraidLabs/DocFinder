using DocFinder.Domain;

namespace DocFinder.Services;

public class ProtocolListItemService : RepositoryService<ProtocolListItem>
{
    public ProtocolListItemService(IProtocolListItemRepository repository) : base(repository) { }
}
