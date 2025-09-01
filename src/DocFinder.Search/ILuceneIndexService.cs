using System.Collections.Generic;
using DocFinder.Domain;

namespace DocFinder.Search;

public interface ILuceneIndexService
{
    void IndexDocument(Document doc);
    void DeleteDocument(int docId);
    IEnumerable<Document> Search(string query);
}
