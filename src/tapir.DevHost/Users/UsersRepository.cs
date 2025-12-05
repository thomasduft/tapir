using System.Collections.Concurrent;

namespace tomware.Tapir.DevHost.Users;

public interface IUsersRepository
{
  public IEnumerable<User> GetAll();
  public User GetById(Guid id);
  public Guid Add(User user);
  public void Update(User user);
  public void Delete(Guid id);
}

public class UsersRepository : IUsersRepository
{
  private readonly User _alice = new() { Id = new Guid("4f4feb38-4398-4776-97c1-7fbf3db69e1f"), Name = "Alice", Age = 30 };
  private readonly User _bob = new() { Id = new Guid("d479b8bf-c895-4587-8658-81dcc8559f2f"), Name = "Bob", Age = 25 };
  private readonly User _charlie = new() { Id = new Guid("e7a0ce49-8da6-435e-adfd-d0f8644b4299"), Name = "Charlie", Age = 35 };

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

  public Guid Add(User user)
  {
    _persons.AddOrUpdate(user.Id, user, (id, existing) => user);
    return user.Id;
  }

  public void Update(User user)
  {
    if (_persons.ContainsKey(user.Id))
    {
      _persons[user.Id] = user;
    }
  }

  public void Delete(Guid id)
  {
    _persons.TryRemove(id, out _);
  }
}
