using DocFinder.Domain;

namespace DocFinder.Services;

public class DataRepository : EfRepository<Data>, IDataRepository
{
    public DataRepository(DocumentDbContext context) : base(context) { }
}
