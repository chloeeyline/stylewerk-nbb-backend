using System.Text;

using StyleWerk.NBB.AWS;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace StyleWerk.NBB.Authentication;

public static class Extension
{
	public static void AddJWT(this WebApplicationBuilder builder, SecretData secretData)
	{
		builder.Services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
		}).AddJwtBearer(options =>
		{
			options.TokenValidationParameters = new TokenValidationParameters()
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidAudience = secretData.JwtIssuer,
				ValidIssuer = secretData.JwtAudience,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretData.JwtKey)),
			};
		});
		builder.Services.AddAuthorization();
		builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
	}

	public static string GetUserAgentString(ControllerBase controller) => controller.Request.Headers.UserAgent.ToString();
}