using System.Text.Json.Serialization;
using SmartX.Api.Services;

//ST10445500 - PROG7312 - SmartX POE
//Program

//.....................................o0oSTART OF FILEo0o........................................//

// Program starts the API, registers services and configures the request pipeline.

var builder = WebApplication.CreateBuilder(args);

// Application services
builder.Services.AddSingleton<SensorStore>();
builder.Services.AddSingleton<DeploymentTreeValidator>();
builder.Services.AddSingleton<TelemetryStore>();
builder.Services.AddSingleton<TelemetryHealth>();

// Controllers and JSON
// The client sends category names, not numbers, so enums need to bind from strings.
builder.Services.AddControllers()
	.AddJsonOptions(options =>
		options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// The Blazor client runs as its own standalone app, so its origin needs to
// be allowed in explicitly. Origins come from config, not hardcoded here.
var allowedOrigins = builder.Configuration
	.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
	options.AddPolicy("SmartXClient", policy =>
		policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();

// Has to come before MapControllers, otherwise it just gets ignored.
app.UseCors("SmartXClient");

app.UseAuthorization();

app.MapControllers();

app.Run();

//.....................................o0oEND OF FILEo0o..........................................//
