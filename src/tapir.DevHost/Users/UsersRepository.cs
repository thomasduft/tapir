using System.Collections.Concurrent;

namespace tomware.Tapir.DevHost.Persons;

public interface IUsersRepository
{
  public IEnumerable<User> GetAll();
  public User GetById(Guid id);
  public Guid Add(User person);
  public void Update(User person);
  public void Delete(Guid id);
}

public class UsersRepository : IUsersRepository
{
  private readonly User _alice = new User(new Guid("4f4feb38-4398-4776-97c1-7fbf3db69e1f"), "Alice", 30);
  private readonly User _bob = new User(new Guid("d479b8bf-c895-4587-8658-81dcc8559f2f"), "Bob", 25);
  private readonly User _charlie = new User(new Guid("e7a0ce49-8da6-435e-adfd-d0f8644b4299"), "Charlie", 35);

  private readonly ConcurrentDictionary<Guid, User> _persons;

  public UsersRepository()
  {
    _persons = new ConcurrentDictionary<Guid, User>
    {
      [_alice.Id] = _alice,
      [_bob.Id] = _bob,
      [_charlie.Id] = _charlie
    };
  }

  public IEnumerable<User> GetAll()
  {
    return _persons.Values.OrderBy(p => p.Name);
  }

  public User GetById(Guid id)
  {
    _persons.TryGetValue(id, out var person);
    return person!;
  }

  public Guid Add(User person)
  {
    var newPerson = person with { Id = Guid.NewGuid() };
    _persons.AddOrUpdate(newPerson.Id, newPerson, (id, existing) => newPerson);
    return newPerson.Id;
  }

  public void Update(User person)
  {
    if (_persons.ContainsKey(person.Id))
    {
      _persons[person.Id] = person;
    }
  }

  public void Delete(Guid id)
  {
    _persons.TryRemove(id, out _);
  }
}