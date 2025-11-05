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
    private readonly ConcurrentDictionary<Guid, User> _persons = new()
    {
        [Guid.NewGuid()] = new User(Guid.NewGuid(), "Alice", 30),
        [Guid.NewGuid()] = new User(Guid.NewGuid(), "Bob", 25),
        [Guid.NewGuid()] = new User(Guid.NewGuid(), "Charlie", 35)
    };

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
