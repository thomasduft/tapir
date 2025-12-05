using Scalar.AspNetCore;

using tomware.Tapir.DevHost.Users;
using tomware.Tapir.DevHost.Documents;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IUsersRepository, UsersRepository>();
builder.Services.AddSingleton<IDocumentsRepository, DocumentsRepository>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

// Configure form options for file uploads
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});

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
app.UseStaticFiles();

// Map Endpoints
app.MapUsersEndpoints();
app.MapDocumentsEndpoints();

app.Run();
