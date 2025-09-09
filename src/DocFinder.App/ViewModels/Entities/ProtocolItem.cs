using System;

namespace DocFinder.App.ViewModels.Entities;

/// <summary>
/// Lightweight protocol representation for lists.
/// </summary>
public sealed record ProtocolItem(Guid Id, string Title, string ReferenceNumber);

