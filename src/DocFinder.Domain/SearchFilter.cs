using System;

namespace DocFinder.Domain;

public sealed record SearchFilter(
    string? FileType = null,
    string? Author = null,
    string? Version = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null);
