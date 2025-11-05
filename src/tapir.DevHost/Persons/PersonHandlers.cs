using Microsoft.AspNetCore.Mvc;

namespace tomware.Tapir.DevHost.Persons;

internal static class PersonHandlers
{
  public static IResult GetAllPersons(IPersonRepository repository)
  {
    var persons = repository.GetAll();
    return Results.Ok(persons);
  }

  public static IResult GetPersonById(
    Guid id,
    IPersonRepository repository
  )
  {
    var person = repository.GetById(id);
    if (person == null)
    {
      return Results.NotFound($"Person with ID {id} not found.");
    }

    return Results.Ok(person);
  }

  public static IResult CreatePerson(
    [FromBody] CreatePersonRequest request,
    IPersonRepository repository
  )
  {
    if (string.IsNullOrWhiteSpace(request.Name))
    {
      return Results.BadRequest("Name is required.");
    }

    if (request.Age <= 0)
    {
      return Results.BadRequest("Age must be a positive number.");
    }

    var person = new Person(Guid.Empty, request.Name, request.Age);
    var id = repository.Add(person);

    var createdPerson = repository.GetById(id);
    return Results.Created($"/persons/{id}", createdPerson);
  }

  public static IResult UpdatePerson(
    Guid id,
    [FromBody] UpdatePersonRequest request,
    IPersonRepository repository
  )
  {
    var existingPerson = repository.GetById(id);
    if (existingPerson == null)
    {
      return Results.NotFound($"Person with ID {id} not found.");
    }

    if (string.IsNullOrWhiteSpace(request.Name))
    {
      return Results.BadRequest("Name is required.");
    }

    if (request.Age <= 0)
    {
      return Results.BadRequest("Age must be a positive number.");
    }

    var updatedPerson = existingPerson with { Name = request.Name, Age = request.Age };
    repository.Update(updatedPerson);

    return Results.Ok(updatedPerson);
  }

  public static IResult DeletePerson(
    Guid id,
    IPersonRepository repository
  )
  {
    var person = repository.GetById(id);
    if (person == null)
    {
      return Results.NotFound($"Person with ID {id} not found.");
    }

    repository.Delete(id);
    return Results.NoContent();
  }
}

public record CreatePersonRequest(string Name, int Age);
public record UpdatePersonRequest(string Name, int Age);
