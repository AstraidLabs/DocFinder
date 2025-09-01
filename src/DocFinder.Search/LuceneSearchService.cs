using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocFinder.Domain;
using Lucene.Net.Analysis.Standard;
using LuceneDocument = Lucene.Net.Documents.Document;
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
    private int _pending;
    private const int CommitThreshold = 1000;

    public LuceneSearchService(LuceneDirectory? directory = null)
    {
        _directory = directory ?? new RAMDirectory();
        _analyzer = new CzechFoldingAnalyzer(LuceneVersion.LUCENE_48);
        var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, _analyzer);
        _writer = new IndexWriter(_directory, config);
        _manager = new SearcherManager(_writer, true, null);
    }

    public Task IndexAsync(IndexDocument doc, CancellationToken ct = default)
    {
        var content = doc.Content ?? string.Empty;
        if (content.Length > 1000)
            content = content.Substring(0, 1000);
        var document = new LuceneDocument
        {
            new StringField("fileId", doc.FileId.ToString(), Field.Store.YES),
            new StringField("path", doc.Path, Field.Store.YES),
            new TextField("filename", doc.FileName, Field.Store.YES),
            new StringField("ext", doc.Ext, Field.Store.YES),
            new Int64Field("sizeBytes", doc.SizeBytes, Field.Store.YES),
            new Int64Field("createdTicks", doc.CreatedUtc.Ticks, Field.Store.YES),
            new Int64Field("modifiedTicks", doc.ModifiedUtc.Ticks, Field.Store.YES),
            new StringField("sha256", doc.Sha256, Field.Store.YES),
            new TextField("content", content, Field.Store.YES)
        };
        if (!string.IsNullOrEmpty(doc.Author))
            document.Add(new StringField("author", doc.Author, Field.Store.YES));
        if (!string.IsNullOrEmpty(doc.Version))
            document.Add(new StringField("version", doc.Version, Field.Store.YES));
        if (!string.IsNullOrEmpty(doc.CaseNumber))
            document.Add(new StringField("caseNumber", doc.CaseNumber, Field.Store.YES));
        if (!string.IsNullOrEmpty(doc.ParcelId))
            document.Add(new StringField("parcelId", doc.ParcelId, Field.Store.YES));
        if (!string.IsNullOrEmpty(doc.Address))
            document.Add(new TextField("address", doc.Address, Field.Store.YES));
        if (!string.IsNullOrEmpty(doc.Tags))
            document.Add(new TextField("tags", doc.Tags, Field.Store.YES));
        foreach (var kv in doc.Metadata)
            document.Add(new StringField($"meta_{kv.Key}", kv.Value, Field.Store.YES));

        _writer.UpdateDocument(new Term("fileId", doc.FileId.ToString()), document);
        _writer.Flush(triggerMerge: false, applyAllDeletes: false);
        _manager.MaybeRefreshBlocking();
        CommitIfNeeded();
        return Task.CompletedTask;
    }

    public Task DeleteByFileIdAsync(Guid fileId, CancellationToken ct = default)
    {
        _writer.DeleteDocuments(new Term("fileId", fileId.ToString()));
        _writer.Flush(triggerMerge: false, applyAllDeletes: false);
        _manager.MaybeRefreshBlocking();
        CommitIfNeeded();
        return Task.CompletedTask;
    }

    public ValueTask<SearchResult> QueryAsync(UserQuery query, CancellationToken ct = default)
    {
        var boolean = new BooleanQuery();
        if (!string.IsNullOrWhiteSpace(query.FreeText))
        {
            var parser = new MultiFieldQueryParser(LuceneVersion.LUCENE_48, new[] { "content", "filename" }, _analyzer);
            Query parsed;
            if (query.UseFuzzy)
            {
                var tokens = query.FreeText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var fuzzy = string.Join(' ', tokens.Select(t => t + "~"));
                parsed = parser.Parse(fuzzy);
            }
            else
            {
                parsed = parser.Parse(query.FreeText);
            }
            boolean.Add(parsed, Occur.MUST);
        }

        foreach (var kv in query.Filters)
        {
            ct.ThrowIfCancellationRequested();
            if (string.Equals(kv.Key, "type", StringComparison.OrdinalIgnoreCase))
            {
                boolean.Add(new TermQuery(new Term("ext", kv.Value)), Occur.MUST);
            }
            else if (kv.Key is "caseNumber" or "parcelId" or "address" or "tags")
            {
                boolean.Add(new TermQuery(new Term(kv.Key, kv.Value)), Occur.MUST);
            }
            else
            {
                boolean.Add(new TermQuery(new Term($"meta_{kv.Key}", kv.Value)), Occur.MUST);
            }
        }

        if (query.FromUtc.HasValue || query.ToUtc.HasValue)
        {
            var from = query.FromUtc?.UtcDateTime.Ticks;
            var to = query.ToUtc?.UtcDateTime.Ticks;
            var range = NumericRangeQuery.NewInt64Range("modifiedTicks", from, to, true, true);
            boolean.Add(range, Occur.MUST);
        }

        var searcher = _manager.Acquire();
        try
        {
            var collector = TopScoreDocCollector.Create(query.Page * query.PageSize, true);
            searcher.Search(boolean, collector);
            var top = collector.GetTopDocs((query.Page - 1) * query.PageSize, query.PageSize);
            var hits = new List<SearchHit>();
            var formatter = new SimpleHTMLFormatter("<strong>", "</strong>");
            var highlighter = new Highlighter(formatter, new QueryScorer(boolean));

            foreach (var scoreDoc in top.ScoreDocs)
            {
                ct.ThrowIfCancellationRequested();
                var doc = searcher.Doc(scoreDoc.Doc);
                var content = doc.Get("content") ?? string.Empty;
                using var tokenStream = _analyzer.GetTokenStream("content", content);
                var snippet = highlighter.GetBestFragment(tokenStream, content);
                tokenStream.Dispose();
                snippet ??= content.Substring(0, Math.Min(200, content.Length));
                var meta = doc.Fields.Where(f => f.Name.StartsWith("meta_"))
                    .ToDictionary(f => f.Name.Substring(5), f => f.GetStringValue());
                hits.Add(new SearchHit(
                    Guid.Parse(doc.Get("fileId")),
                    doc.Get("filename"),
                    doc.Get("path"),
                    doc.Get("ext"),
                    long.Parse(doc.Get("sizeBytes") ?? "0"),
                    new DateTime(long.Parse(doc.Get("createdTicks") ?? "0")),
                    new DateTime(long.Parse(doc.Get("modifiedTicks") ?? "0")),
                    doc.Get("sha256") ?? string.Empty,
                    doc.Get("author"),
                    doc.Get("version"),
                    scoreDoc.Score,
                    snippet,
                    meta));
            }

            var result = new SearchResult(top.TotalHits, hits, new Dictionary<string, int>());
            return new ValueTask<SearchResult>(result);
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
        if (_pending > 0)
        {
            _writer.Commit();
            _manager.MaybeRefreshBlocking();
        }
        _manager.Dispose();
        _writer.Dispose();
        _directory.Dispose();
        _analyzer.Dispose();
    }

    private void CommitIfNeeded()
    {
        _pending++;
        if (_pending >= CommitThreshold)
        {
            _writer.Commit();
            _manager.MaybeRefreshBlocking();
            _pending = 0;
        }
    }
}
