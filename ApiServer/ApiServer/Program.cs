using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.AWS;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Helper;
using StyleWerk.NBB.Queries;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

SecretData secretData = builder.AddAmazonSecretsManager();

builder.Services.AddDbContext<NbbContext>(options => options.UseNpgsql(secretData.GetConnectionString()));
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
