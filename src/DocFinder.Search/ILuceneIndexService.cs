using System;
using System.Collections.Generic;
using DocFinder.Domain;

namespace DocFinder.Search;

public interface ILuceneIndexService
{
    void IndexDocument(Protocol doc);
    void DeleteDocument(Guid docId);
    IEnumerable<Protocol> Search(string query);
}
