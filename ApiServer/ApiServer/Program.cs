WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(options =>
		options.AllowAnyOrigin()
		.AllowAnyMethod()
		.AllowAnyHeader());
});

builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);

WebApplication app = builder.Build();

app.UseRouting();
app.UseCors();

app.Run();
