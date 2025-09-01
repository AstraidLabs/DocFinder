using System.Collections.Generic;
using DocFinder.Domain;

namespace DocFinder.Application;

public interface IDocumentIndexService
{
    void IndexDocument(Document doc);
    void DeleteDocument(int docId);
    IEnumerable<Document> Search(string query);
}
