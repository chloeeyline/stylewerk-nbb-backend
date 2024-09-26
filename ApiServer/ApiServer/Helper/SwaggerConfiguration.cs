using System.Reflection;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace StyleWerk.NBB.Helper;

public static class SwaggerConfiguration
{
    public static void AddSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SupportNonNullableReferenceTypes();

            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Stylewerk - NBB API", Version = "v1" });

            options.TagActionsBy(api =>
            {
                string group = string.IsNullOrWhiteSpace(api.GroupName) ? "" : $" - {api.GroupName}";
                return api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor
                    ? new[] { $"{controllerActionDescriptor.ControllerName}{group}" }
                    : throw new InvalidOperationException("Unable to determine tag for endpoint.");
            });
            options.DocInclusionPredicate((name, api) => true);

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            options.OperationFilter<AuthorizeCheckOperationFilter>();

            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });
    }
}

public class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check if the endpoint has an [Authorize] attribute
        bool hasAuthorize = false;
        if (context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()) hasAuthorize = true;
        else if (context.MethodInfo.DeclaringType is not null && context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()) hasAuthorize = true;

        if (hasAuthorize)
        {
            // Add the security requirement to Swagger UI for this endpoint
            operation.Security =
            [
                new() {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                }
            ];
        }
    }
}
