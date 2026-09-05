var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
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
