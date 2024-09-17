using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Helper;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<NbbContext>(options => options.UseNpgsql(TempHelper.ConnectionString));
builder.Services.AddControllers(options => options.UseDateOnlyTimeOnlyStringConverters());

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

app.MapControllers();

app.Run();
