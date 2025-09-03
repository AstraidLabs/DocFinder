using System;

namespace DocFinder.Domain;

public class AuditEntry
{
    private AuditEntry() { }

    public AuditEntry(Guid documentId, string action, DateTime timestamp, string userName)
    {
        DocumentId = documentId;
        Action = ValidateRequired(action, nameof(action));
        Timestamp = timestamp;
        UserName = ValidateRequired(userName, nameof(userName));
    }

    public int Id { get; private set; }

    public Guid DocumentId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public DateTime Timestamp { get; private set; }

    public string UserName { get; private set; } = string.Empty;

    private static string ValidateRequired(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{name} required", name);
        return value;
    }
}
