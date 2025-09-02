using System;
using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using LuceneDocument = Lucene.Net.Documents.Document;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Directory = Lucene.Net.Store.Directory;

namespace DocFinder.Search;

public sealed class LuceneIndexService : ILuceneIndexService, IDisposable
{
    private readonly Analyzer _analyzer;
    private readonly Directory _directory;
    private readonly IndexWriter _writer;
    private readonly SearcherManager _manager;

    public LuceneIndexService(Directory? directory = null)
    {
        _directory = directory ?? new RAMDirectory();
        _analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);
        var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, _analyzer);
        _writer = new IndexWriter(_directory, config);
        _manager = new SearcherManager(_writer, true, null);
    }

    public void IndexDocument(DocFinder.Domain.Document doc)
    {
        var document = new LuceneDocument
        {
            new Int32Field("id", doc.Id, Field.Store.YES),
            new TextField("building", doc.BuildingName ?? string.Empty, Field.Store.YES),
            new TextField("name", doc.Name ?? string.Empty, Field.Store.YES),
            new TextField("author", doc.Author ?? string.Empty, Field.Store.YES),
            new StringField("type", doc.Type ?? string.Empty, Field.Store.YES),
            new StringField("fileLink", doc.FileLink ?? string.Empty, Field.Store.YES),
            new TextField("content", LoadContent(doc.FileLink), Field.Store.NO)
        };
        _writer.UpdateDocument(new Term("id", doc.Id.ToString()), document);
        _writer.Commit();
        _manager.MaybeRefreshBlocking();
    }

    public void DeleteDocument(int docId)
    {
        _writer.DeleteDocuments(new Term("id", docId.ToString()));
        _writer.Commit();
        _manager.MaybeRefreshBlocking();
    }

    public IEnumerable<DocFinder.Domain.Document> Search(string query)
    {
        var parser = new MultiFieldQueryParser(LuceneVersion.LUCENE_48, new[] { "building", "name", "author", "type", "content" }, _analyzer);
        var parsed = parser.Parse(query);
        var searcher = _manager.Acquire();
        try
        {
            var hits = searcher.Search(parsed, 20).ScoreDocs;
            foreach (var hit in hits)
            {
                var d = searcher.Doc(hit.Doc);
                yield return new DocFinder.Domain.Document(
                    id: int.Parse(d.Get("id")),
                    buildingName: d.Get("building"),
                    name: d.Get("name"),
                    author: d.Get("author"),
                    modifiedAt: DateTime.MinValue,
                    version: string.Empty,
                    type: d.Get("type"),
                    issuedAt: null,
                    validUntil: null,
                    canPrint: false,
                    isElectronic: false,
                    fileLink: d.Get("fileLink"));
            }
        }
        finally
        {
            _manager.Release(searcher);
        }
    }

    private static string LoadContent(string? path)
    {
        if (string.IsNullOrEmpty(path)) return string.Empty;
        try
        {
            if (File.Exists(path))
            {
                var ext = Path.GetExtension(path).ToLowerInvariant();
                if (ext == ".txt")
                    return File.ReadAllText(path);
            }
        }
        catch
        {
        }
        return string.Empty;
    }

    public void Dispose()
    {
        _manager.Dispose();
        _writer.Dispose();
        _directory.Dispose();
        _analyzer.Dispose();
    }
}
