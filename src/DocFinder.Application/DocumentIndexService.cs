using System;
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

    public void IndexDocument(Protocol doc) => _inner.IndexDocument(doc);

    public void DeleteDocument(Guid docId) => _inner.DeleteDocument(docId);

    public IEnumerable<Protocol> Search(string query) => _inner.Search(query);
}
