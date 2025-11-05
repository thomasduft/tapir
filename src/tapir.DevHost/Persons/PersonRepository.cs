using System.Collections.Concurrent;

namespace tomware.Tapir.DevHost.Persons;

public interface IPersonRepository
{
    public IEnumerable<Person> GetAll();
    public Person GetById(Guid id);
    public Guid Add(Person person);
    public void Update(Person person);
    public void Delete(Guid id);
}

public class PersonRepository : IPersonRepository
{
    private readonly ConcurrentDictionary<Guid, Person> _persons = new()
    {
        [Guid.NewGuid()] = new Person(Guid.NewGuid(), "Alice", 30),
        [Guid.NewGuid()] = new Person(Guid.NewGuid(), "Bob", 25),
        [Guid.NewGuid()] = new Person(Guid.NewGuid(), "Charlie", 35)
    };

    public IEnumerable<Person> GetAll()
    {
        return _persons.Values.OrderBy(p => p.Name);
    }

    public Person GetById(Guid id)
    {
        _persons.TryGetValue(id, out var person);
        return person!;
    }

    public Guid Add(Person person)
    {
        var newPerson = person with { Id = Guid.NewGuid() };
        _persons.AddOrUpdate(newPerson.Id, newPerson, (id, existing) => newPerson);
        return newPerson.Id;
    }

    public void Update(Person person)
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
