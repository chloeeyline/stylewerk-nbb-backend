using System.Reflection;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;

using StyleWerk.NBB.Models;

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
                Description = "Please enter the access Token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });

            options.OperationFilter<AuthorizeCheckOperationFilter>();
            options.OperationFilter<ResultCodesOperationFilter>();
            options.SchemaFilter<EnumSchemaFilter>();

            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
            options.SchemaFilter<GenericSchemaFilter>();
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

public class ResultCodesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ResultCodesResponseAttribute? resultCodesAttr = context.MethodInfo.GetCustomAttribute<ResultCodesResponseAttribute>(false);

        if (resultCodesAttr is null)
            return;

        List<string> resultCodesDescriptions = [.. resultCodesAttr.PossibleCodes.Select(code => $"- {code} = {(int) code}")];
        string formattedResultCodes = "### Possible Result Codes:\n" + string.Join("\n", resultCodesDescriptions);

        foreach (KeyValuePair<string, OpenApiResponse> response in operation.Responses)
            response.Value.Description += "\n\n" + formattedResultCodes;
    }
}

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            // Generate a bullet list of enum names and values
            List<string> enumDescriptions = [.. Enum.GetValues(context.Type).Cast<Enum>()
                .Select(value => $"- **{value}** = {Convert.ChangeType(value, value.GetTypeCode())}")];

            // Add the formatted description to the schema
            schema.Description = $"### Enum Values:\n" + string.Join("\n", enumDescriptions);
        }
    }
}

public class GenericSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsGenericType && context.Type.GetGenericTypeDefinition() == typeof(Model_Result<>))
        {
            Type genericArgument = context.Type.GetGenericArguments()[0];
            if (genericArgument == typeof(string))
            {
                schema.Title = $"Model_Result";
                return;
            }

            string typeName;
            // Check if the generic argument is an array
            if (genericArgument.IsArray)
            {
                string itemType = genericArgument.GetElementType()?.Name ?? "Unknown";
                typeName = $"{itemType}[]";
            }
            // Check if the generic argument is a collection (IEnumerable<>)
            else if (genericArgument.IsGenericType && genericArgument.GetGenericTypeDefinition() == typeof(List<>))
            {
                string itemType = genericArgument.GetGenericArguments()[0].Name;
                typeName = $"{itemType}[]";
            }
            else
            {
                // For non-generic types like string, int, etc.
                typeName = genericArgument.Name;
            }

            // Update the schema title
            schema.Title = $"Model_Result<{typeName}>";
        }
    }
}
