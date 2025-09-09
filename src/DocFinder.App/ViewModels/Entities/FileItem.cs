using System;

namespace DocFinder.App.ViewModels.Entities;

/// <summary>
/// Lightweight file representation for list displays.
/// </summary>
public sealed record FileItem(Guid Id, string Path, string Name);

