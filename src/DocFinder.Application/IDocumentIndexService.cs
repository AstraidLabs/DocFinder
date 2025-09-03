using System;
using System.Collections.Generic;
using DocFinder.Domain;

namespace DocFinder.Application;

public interface IDocumentIndexService
{
    void IndexDocument(Protocol doc);
    void DeleteDocument(Guid docId);
    IEnumerable<Protocol> Search(string query);
}
