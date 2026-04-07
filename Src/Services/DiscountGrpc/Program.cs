using DiscountGrpc.Data;
using DiscountGrpc.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddDbContext<DiscountContext>(opts =>
        opts.UseSqlite(builder.Configuration.GetConnectionString("Database")));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMigration();
// Map the gRPC service to the application's request pipeline, allowing it to handle incoming gRPC requests.
app.MapGrpcService<DiscountService>();

app.Run();
