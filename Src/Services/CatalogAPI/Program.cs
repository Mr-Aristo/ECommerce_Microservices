
var builder = WebApplication.CreateBuilder(args);

//Add services  to the container.

//The reason of the typeof(Program).Assembly to provide to run commands and queriesd only in this progran.cs
var assembly = typeof(Program).Assembly;

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    //Registration of Pipeline behavior
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));//<,> that means generic
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(assembly);
builder.Services.AddCarter();

builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("PostgreDataBase")!);
}).UseLightweightSessions();

//Data seeding
if (builder.Environment.IsDevelopment())
    builder.Services.InitializeMartenWith<CatalogInitialData>();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

//HealthCheck
builder.Services.AddHealthChecks().AddNpgSql(builder.Configuration.GetConnectionString("PostgreDataBase")!);

var app = builder.Build();
// Configure the HTTP request pipline.

app.MapCarter();

app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{   //HealthCheck with psql
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseExceptionHandler(options => { });

app.Run();
