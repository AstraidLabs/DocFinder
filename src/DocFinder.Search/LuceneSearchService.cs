using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocFinder.Domain;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using LuceneDirectory = Lucene.Net.Store.Directory;
using Lucene.Net.Util;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Analysis;

namespace DocFinder.Search;

public sealed class LuceneSearchService : ISearchService, IDisposable
{
    private readonly Analyzer _analyzer;
    private readonly LuceneDirectory _directory;
    private readonly IndexWriter _writer;
    private readonly SearcherManager _manager;

    public LuceneSearchService(LuceneDirectory? directory = null)
    {
        _directory = directory ?? new RAMDirectory();
        _analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);
        var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, _analyzer);
        _writer = new IndexWriter(_directory, config);
        _manager = new SearcherManager(_writer, true, null);
    }

    public Task IndexAsync(IndexDocument doc, CancellationToken ct = default)
    {
        var document = new Document
        {
            new StringField("fileId", doc.FileId.ToString(), Field.Store.YES),
            new StringField("path", doc.Path, Field.Store.YES),
            new TextField("filename", doc.FileName, Field.Store.YES),
            new StringField("ext", doc.Ext, Field.Store.YES),
            new TextField("content", doc.Content ?? string.Empty, Field.Store.YES),
            new Int64Field("modifiedTicks", doc.ModifiedUtc.Ticks, Field.Store.YES)
        };
        foreach (var kv in doc.Metadata)
            document.Add(new StringField($"meta_{kv.Key}", kv.Value, Field.Store.YES));

        _writer.UpdateDocument(new Term("fileId", doc.FileId.ToString()), document);
        _writer.Commit();
        _manager.MaybeRefreshBlocking();
        return Task.CompletedTask;
    }

    public Task DeleteByFileIdAsync(Guid fileId, CancellationToken ct = default)
    {
        _writer.DeleteDocuments(new Term("fileId", fileId.ToString()));
        _writer.Commit();
        _manager.MaybeRefreshBlocking();
        return Task.CompletedTask;
    }

    public Task<SearchResult> QueryAsync(UserQuery query, CancellationToken ct = default)
    {
        var boolean = new BooleanQuery();
        if (!string.IsNullOrWhiteSpace(query.FreeText))
        {
            var parser = new MultiFieldQueryParser(LuceneVersion.LUCENE_48, new[] { "content", "filename" }, _analyzer);
            var parsed = parser.Parse(query.FreeText);
            boolean.Add(parsed, Occur.MUST);
        }

        if (query.Filters != null && query.Filters.TryGetValue("type", out var ext))
        {
            boolean.Add(new TermQuery(new Term("ext", ext)), Occur.MUST);
        }

        var searcher = _manager.Acquire();
        try
        {
            var collector = TopScoreDocCollector.Create(query.Page * query.PageSize, true);
            searcher.Search(boolean, collector);
            var top = collector.GetTopDocs((query.Page - 1) * query.PageSize, query.PageSize);
            var hits = new List<SearchHit>();
            var formatter = new SimpleHTMLFormatter("<b>", "</b>");
            var highlighter = new Highlighter(formatter, new QueryScorer(boolean));

            foreach (var scoreDoc in top.ScoreDocs)
            {
                var doc = searcher.Doc(scoreDoc.Doc);
                var content = doc.Get("content") ?? string.Empty;
                var tokenStream = _analyzer.GetTokenStream("content", content);
                var snippet = highlighter.GetBestFragment(tokenStream, content) ?? content.Substring(0, Math.Min(200, content.Length));
                var meta = doc.Fields.Where(f => f.Name.StartsWith("meta_")).ToDictionary(f => f.Name.Substring(5), f => f.GetStringValue());
                hits.Add(new SearchHit(
                    Guid.Parse(doc.Get("fileId")),
                    doc.Get("filename"),
                    doc.Get("path"),
                    doc.Get("ext"),
                    new DateTime(long.Parse(doc.Get("modifiedTicks") ?? "0")),
                    scoreDoc.Score,
                    snippet,
                    meta));
            }

            var result = new SearchResult(top.TotalHits, hits, new Dictionary<string, int>());
            return Task.FromResult(result);
        }
        finally
        {
            _manager.Release(searcher);
        }
    }

    public Task OptimizeAsync(CancellationToken ct = default)
    {
        _writer.ForceMerge(1);
        _writer.Commit();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _manager.Dispose();
        _writer.Dispose();
        _directory.Dispose();
        _analyzer.Dispose();
    }
}
