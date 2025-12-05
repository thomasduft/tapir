using System.Collections.Concurrent;

namespace tomware.Tapir.DevHost.Documents;

public interface IDocumentsRepository
{
  public IEnumerable<Document> GetAll();
  public Document GetById(Guid id);
  public Guid Add(Document document);
  public void Update(Document document);
  public void Delete(Guid id);
}

public class DocumentsRepository : IDocumentsRepository
{

  private readonly ConcurrentDictionary<Guid, Document> _documents = new();

  public DocumentsRepository() { }

  public IEnumerable<Document> GetAll()
  {
    return _documents.Values.OrderBy(d => d.Title);
  }

  public Document GetById(Guid id)
  {
    _documents.TryGetValue(id, out var document);
    return document!;
  }

  public Guid Add(Document document)
  {
    _documents.AddOrUpdate(document.Id, document, (id, existing) => document);
    return document.Id;
  }

  public void Update(Document document)
  {
    if (_documents.ContainsKey(document.Id))
    {
      _documents[document.Id] = document;
    }
  }

  public void Delete(Guid id)
  {
    _documents.TryRemove(id, out _);
  }
}
