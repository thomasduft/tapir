namespace tomware.Tapir.DevHost.Users;

internal static class Endpoints
{
  public static void MapUsersEndpoints(this IEndpointRouteBuilder builder)
  {
    builder
      .MapGet("/users", UsersHandlers.GetAllUsers)
      .WithTags("Users")
      .WithDescription("Retrieves all users from the repository.");

    builder
      .MapGet("/users/{id:guid}", UsersHandlers.GetUserById)
      .WithTags("Users")
      .WithDescription("Retrieves a user by their ID.");

    builder
      .MapPost("/users", UsersHandlers.CreateUser)
      .WithTags("Users")
      .WithDescription("Creates a new user.");

    builder
      .MapGet("/users/xml", UsersHandlers.GetAllUsersXml)
      .WithTags("Users")
      .WithDescription("Retrieves all users as XML.");

    builder
      .MapPost("/users/xml", UsersHandlers.CreateUserXml)
      .Accepts<string>("application/xml")
      .Produces(StatusCodes.Status201Created, typeof(string), "application/xml")
      .WithTags("Users")
      .WithDescription("Creates a user from XML and returns XML.");

    builder
      .MapPut("/users/{id:guid}", UsersHandlers.UpdateUser)
      .WithTags("Users")
      .WithDescription("Updates an existing user.");

    builder
      .MapDelete("/users/{id:guid}", UsersHandlers.DeleteUser)
      .WithTags("Users")
      .WithDescription("Deletes a user by their ID.");
  }
}
