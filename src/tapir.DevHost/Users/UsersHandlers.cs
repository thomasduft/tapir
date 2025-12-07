using Microsoft.AspNetCore.Mvc;

namespace tomware.Tapir.DevHost.Users;

internal static class UsersHandlers
{
  public static IResult GetAllUsers(IUsersRepository repository)
  {
    var persons = repository.GetAll()
      .OrderBy(p => p.Name)
      .ToList();
    return Results.Ok(persons);
  }

  public static IResult GetUserById(
    Guid id,
    IUsersRepository repository
  )
  {
    var user = repository.GetById(id);
    if (user == null)
    {
      return Results.NotFound($"User with ID {id} not found.");
    }

    return Results.Ok(user);
  }

  public static IResult CreateUser(
    [FromBody] CreateUserRequest request,
    IUsersRepository repository
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

    var user = new User { Id = Guid.NewGuid(), Name = request.Name, Age = request.Age };
    var id = repository.Add(user);

    var createdUser = repository.GetById(id);
    return Results.Created($"/users/{id}", createdUser.Id);
  }

  public static IResult UpdateUser(
    Guid id,
    [FromBody] UpdateUserRequest request,
    IUsersRepository repository
  )
  {
    var existingUser = repository.GetById(id);
    if (existingUser == null)
    {
      return Results.NotFound($"User with ID {id} not found.");
    }

    if (string.IsNullOrWhiteSpace(request.Name))
    {
      return Results.BadRequest("Name is required.");
    }

    if (request.Age <= 0)
    {
      return Results.BadRequest("Age must be a positive number.");
    }

    var updatedUser = existingUser with { Name = request.Name, Age = request.Age };
    repository.Update(updatedUser);

    return Results.Ok(updatedUser);
  }

  public static IResult DeleteUser(
    Guid id,
    IUsersRepository repository
  )
  {
    var user = repository.GetById(id);
    if (user == null)
    {
      return Results.NotFound($"User with ID {id} not found.");
    }

    repository.Delete(id);
    return Results.NoContent();
  }
}

public record CreateUserRequest(string Name, int Age);
public record UpdateUserRequest(string Name, int Age);
