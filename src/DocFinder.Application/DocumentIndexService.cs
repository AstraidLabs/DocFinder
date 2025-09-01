using System.Collections.Generic;
using DocFinder.Domain;
using DocFinder.Search;

namespace DocFinder.Application;

public sealed class DocumentIndexService : IDocumentIndexService
{
    private readonly ILuceneIndexService _inner;

    public DocumentIndexService(ILuceneIndexService inner)
    {
        _inner = inner;
    }

    public void IndexDocument(Document doc) => _inner.IndexDocument(doc);

    public void DeleteDocument(int docId) => _inner.DeleteDocument(docId);

    public IEnumerable<Document> Search(string query) => _inner.Search(query);
}
