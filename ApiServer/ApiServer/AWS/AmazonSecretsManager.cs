using System.Text.Json;

using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace StyleWerk.NBB.AWS;

public static class AmazonSecretsManager
{
    public static string GetSecret()
    {
#if Local
		// Use static local values in Local mode based on Windows username until AWS is configurated for the team
		return GetDebugSecret();
#else

        string region = "eu-north-1";
        string secretName = "nbb/dev";

        GetSecretValueRequest request = new()
        {
            SecretId = secretName,
            VersionStage = "AWSCURRENT"
        };

        using AmazonSecretsManagerClient client = new(RegionEndpoint.GetBySystemName(region));
        GetSecretValueResponse response = client.GetSecretValueAsync(request).Result;

        string secretString;
        if (response.SecretString != null)
        {
            secretString = response.SecretString;
        }
        else
        {
            MemoryStream memoryStream = response.SecretBinary;
            StreamReader reader = new(memoryStream);
            secretString = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
        }

        return secretString;
#endif
    }


    private static string GetDebugSecret()
    {
        // Get the current Windows username
        string userName = Environment.UserName;
        SecretData debugSecretData = userName.ToLower() switch
        {
            //If a user has a differnt DB Configuration or wants to test around without the propability to push anything that would break the code for other developers
            "user1" => new SecretData(
                            "postgres",
                            "w9MWcR@j*L9m",
                            "localhost",
                            "5432",
                            "stylewerk",
                            "xxx",
                            "http://localhost:5115",
                            "http://localhost:5115",
                            "xxx"
                        ),
            _ => new SecretData(
                            "postgres",
                            "w9MWcR@j*L9m",
                            "localhost",
                            "5432",
                            "stylewerk",
                            "xxx",
                            "http://localhost:5115",
                            "http://localhost:5115",
                            "xxx"
                        ),
        };
        return JsonSerializer.Serialize(debugSecretData);
    }

    private static void AddAmazonSecretsManagerConfiguration(this IConfigurationBuilder configurationBuilder)
    {
        AmazonSecretsManagerConfigurationSource configurationSource = new();
        configurationBuilder.Add(configurationSource);
    }

    public static SecretData AddAmazonSecretsManager(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddAmazonSecretsManagerConfiguration();
        builder.Services.Configure<SecretData>(builder.Configuration);
        builder.Services.Configure<SecretData>(builder.Configuration.GetSection(nameof(SecretData)));
        SecretData secretData = builder.Configuration.Get<SecretData>() ?? throw new Exception();
        return secretData;
    }
    public static string GetConnectionString(this SecretData secretData)
    {
        return $"Username={secretData.DbUser};Password={secretData.DbPassword};Host={secretData.DbHost};Port={secretData.DbPort};Database={secretData.DbDatabase};Include Error Detail=true;";
    }

    public static SecretData? GetData() => JsonSerializer.Deserialize<SecretData?>(GetSecret());
}

public class AmazonSecretsManagerConfigurationProvider() : ConfigurationProvider
{
    public override void Load()
    {
        string secret = AmazonSecretsManager.GetSecret();
        Dictionary<string, string?>? temp = JsonSerializer.Deserialize<Dictionary<string, string?>>(secret);
        temp ??= [];
        Data = temp;
    }
}

public class AmazonSecretsManagerConfigurationSource() : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder) => new AmazonSecretsManagerConfigurationProvider();
}

public record SecretData(string DbUser, string DbPassword, string DbHost, string DbPort, string DbDatabase, string JwtKey, string JwtIssuer, string JwtAudience, string PasswortPepper)
{
    public SecretData() : this("", "", "", "", "", "", "", "", "") { }
}