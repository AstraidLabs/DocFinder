using System;
using System.Collections.Generic;
using System.Linq;
using DocFinder.Domain;

namespace DocFinder.Search;

public sealed class LuceneIndexService : ILuceneIndexService, IDisposable
{
    private readonly Dictionary<Guid, Protocol> _store = new();

    public void IndexDocument(Protocol doc)
    {
        _store[doc.Id] = doc;
    }

    public void DeleteDocument(Guid docId)
    {
        _store.Remove(docId);
    }

    public IEnumerable<Protocol> Search(string query)
    {
        return _store.Values.Where(p =>
            p.Title.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    public void Dispose() { }
}
