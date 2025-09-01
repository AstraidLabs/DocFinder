using System;
using DocFinder.Search;
using Xunit;

namespace DocFinder.Tests;

public class QueryParserTests
{
    [Fact]
    public void ParsesFiltersAndDates()
    {
        var query = UserQueryParser.Parse("test type:pdf from:2023-01-01 author:\"John Doe\"");
        Assert.Equal("test", query.FreeText);
        Assert.Equal("pdf", query.Filters["type"]);
        Assert.Equal("John Doe", query.Filters["author"]);
        Assert.Equal(new DateTimeOffset(2023,1,1,0,0,0,TimeSpan.Zero), query.FromUtc);
        Assert.Null(query.ToUtc);
    }
}
