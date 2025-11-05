namespace tomware.Tapir.DevHost.Persons;

internal static class Endpoints
{
  public static void MapPersonsEndpoints(this IEndpointRouteBuilder builder)
  {
    builder
      .MapGet("/persons", PersonHandlers.GetAllPersons)
      .WithTags("Persons")
      .WithDescription("Retrieves all persons from the repository.");
    builder
      .MapGet("/persons/{id:guid}", PersonHandlers.GetPersonById)
      .WithTags("Persons")
      .WithDescription("Retrieves a person by their ID.");
    builder
      .MapPost("/persons", PersonHandlers.CreatePerson)
      .WithTags("Persons")
      .WithDescription("Creates a new person.");
    builder
      .MapPut("/persons/{id:guid}", PersonHandlers.UpdatePerson)
      .WithTags("Persons")
      .WithDescription("Updates an existing person.");
    builder
      .MapDelete("/persons/{id:guid}", PersonHandlers.DeletePerson)
      .WithTags("Persons")
      .WithDescription("Deletes a person by their ID.");
  }
}
