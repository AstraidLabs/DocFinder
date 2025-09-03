using DocFinder.Domain;

namespace DocFinder.Services;

public class ProtocolListService : RepositoryService<ProtocolList>
{
    public ProtocolListService(IProtocolListRepository repository) : base(repository) { }
}
