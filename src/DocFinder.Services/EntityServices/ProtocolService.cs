using DocFinder.Domain;

namespace DocFinder.Services;

public class ProtocolService : RepositoryService<Protocol>
{
    public ProtocolService(IProtocolRepository repository) : base(repository) { }
}
