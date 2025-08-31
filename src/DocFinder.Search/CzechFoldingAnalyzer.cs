using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Cz;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;

namespace DocFinder.Search;

/// <summary>
/// Analyzer tailored for Czech language that also folds diacritics to ASCII.
/// </summary>
public sealed class CzechFoldingAnalyzer : Analyzer
{
    private readonly LuceneVersion _version;
    private readonly CharArraySet _stopWords;

    public CzechFoldingAnalyzer(LuceneVersion version, IEnumerable<string>? stopWords = null)
    {
        _version = version;
        _stopWords = stopWords != null
            ? StopFilter.MakeStopSet(version, stopWords.ToArray())
            : CzechAnalyzer.DefaultStopSet;
    }

    protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
    {
        var source = new StandardTokenizer(_version, reader);
        TokenStream result = new LowerCaseFilter(_version, source);
        result = new StopFilter(_version, result, _stopWords);
        result = new CzechStemFilter(result);
        result = new ASCIIFoldingFilter(result);
        return new TokenStreamComponents(source, result);
    }
}
