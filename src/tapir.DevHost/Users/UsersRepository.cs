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
  private readonly User _alice = new User(Guid.NewGuid(), "Alice", 30);
  private readonly User _bob = new User(Guid.NewGuid(), "Bob", 25);
  private readonly User _charlie = new User(Guid.NewGuid(), "Charlie", 35);

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
