using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.AWS;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Helper;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

SecretData secretData = builder.AddAmazonSecretsManager();

builder.Services.AddControllers(options =>
{
    options.UseDateOnlyTimeOnlyStringConverters();
    options.Filters.Add(new ProducesAttribute("application/json"));
});
builder.Services.AddDbContext<NbbContext>(options => options.UseNpgsql(secretData.GetConnectionString()));
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

builder.AddJWT(secretData);
builder.AddSwagger();

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

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

app.Run();

