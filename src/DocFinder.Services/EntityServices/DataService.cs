using DocFinder.Domain;

namespace DocFinder.Services;

public class DataService : RepositoryService<Data>
{
    public DataService(IDataRepository repository) : base(repository) { }
}
