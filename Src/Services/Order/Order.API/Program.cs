
using Order.Infrastructure.Data.Extentions;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container 

builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseApiServices();

if (app.Environment.IsDevelopment())
{
    // Auto Migrate
    await app.InitialiseDatabaseAsync();
}

// Configure the http request pipeline
app.Run();
