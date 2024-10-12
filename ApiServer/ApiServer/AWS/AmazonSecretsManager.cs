using System.Text.Json;

using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace StyleWerk.NBB.AWS;

public static class AmazonSecretsManager
{
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
}

public class AmazonSecretsManagerConfigurationProvider() : ConfigurationProvider
{
    public override void Load()
    {
        string secret = SecretData.Load();
        Dictionary<string, string?>? temp = JsonSerializer.Deserialize<Dictionary<string, string?>>(secret);
        temp ??= [];
        Data = temp;
    }
}

public class AmazonSecretsManagerConfigurationSource() : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder) => new AmazonSecretsManagerConfigurationProvider();
}

public record SecretData(string DbUser, string DbPassword, string DbHost, string DbPort, string DbDatabase, string JwtKey, string JwtIssuer, string JwtAudience, string FrontendUrl, string PasswortPepper)
{
    public SecretData() : this("", "", "", "", "", "", "", "", "", "") { }

    public string ConnectionString => $"Username={DbUser};Password={DbPassword};Host={DbHost};Port={DbPort};Database={DbDatabase};Include Error Detail=true;";

    public static SecretData GetData(bool isLocal = false) => JsonSerializer.Deserialize<SecretData?>(Load(isLocal)) ?? throw new Exception();

    public static string Load(bool isLocal = false)
    {
#if Local
        isLocal = true;
#endif
        return isLocal ? DebugSecret() : ActiveSecret();
    }

    public static string ActiveSecret()
    {
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
    }

    public static string DebugSecret()
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
                            "",
                            "xxx"
                        ),
            _ => new SecretData(
                            "postgres",
                            "w9MWcR@j*L9m",
                            "localhost",
                            "5432",
                            "stylewerk",
                            "nwhRqucOl04Kgk7IUJ/J+oimkLA0eiqhVKFyayQscBA=",
                            "http://localhost:5115",
                            "http://localhost:5115",
                            "",
                            "xxx"
                        ),
        };
        return JsonSerializer.Serialize(debugSecretData);
    }
}