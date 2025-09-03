using DocFinder.Domain;

namespace DocFinder.Domain;

public interface IAuditEntryRepository : IRepository<AuditEntry> { }
public interface IDataRepository : IRepository<Data> { }
public interface IFileRepository : IRepository<File> { }
public interface IFileListRepository : IRepository<FileList> { }
public interface IFileListItemRepository : IRepository<FileListItem> { }
public interface IProtocolRepository : IRepository<Protocol> { }
public interface IProtocolListRepository : IRepository<ProtocolList> { }
public interface IProtocolListItemRepository : IRepository<ProtocolListItem> { }
