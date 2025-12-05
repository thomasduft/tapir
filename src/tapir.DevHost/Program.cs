using Scalar.AspNetCore;

using tomware.Tapir.DevHost.Users;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IUsersRepository, UsersRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.MapScalarApiReference(options =>
  {
    options
     .WithTitle("Tapir DevHost API")
     .WithTheme(ScalarTheme.Mars)
     .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
  });
}

app.UseHttpsRedirection();

// Map Endpoints
app.MapUsersEndpoints();

app.Run();
