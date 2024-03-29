using System.Text;
using dotenv.net;

namespace TechNews.Auth.Api.Configurations;

public static class EnvironmentVariables
{
    public static string? DatabaseConnectionString { get; private set; }
    public static string? DiscordWebhookId { get; private set; }
    public static string? DiscordWebhookToken { get; private set; }
    public static int TokenExpirationInMinutes { get; private set; } = 10;
    public static int KeyRotatorExecutionInMinutes { get; private set; } = 5;
    public static int KeyCreationSizeInBits { get; private set; } = 2048;
    public static int KeyExpirationInDays { get; private set; } = 30;
    public static string? CryptographicAlgorithm { get; private set; }
    public static string AzureKeyVaultUrl { get; private set; } = string.Empty;
    public static string BrokerHostName { get; private set; } = string.Empty;
    public static string BrokerVirtualHost { get; private set; } = string.Empty;
    public static string BrokerUserName { get; private set; } = string.Empty;
    public static string BrokerPassword { get; private set; } = string.Empty;
    public static string EventStoreConnectionString { get; private set; } = string.Empty;
    public static string? IssuerName { get; private set; } = string.Empty;

    public static IServiceCollection AddEnvironmentVariables(this IServiceCollection services, IWebHostEnvironment environment)
    {
        try
        {
            DotEnv.Fluent()
                .WithExceptions()
                .WithEnvFiles()
                .WithTrimValues()
                .WithEncoding(Encoding.UTF8)
                .WithOverwriteExistingVars()
                .WithProbeForEnv(probeLevelsToSearch: 6)
                .Load();
        }
        catch (Exception)
        {
            if (environment.IsEnvironment("Local"))
            {
                throw new ApplicationException("Environment File (.env) not found. The application needs a .env file to run locally.\nPlease check the section Environment Variables of the README.");
            }

            // Ignored if other environments because it is using runtime environment variables
        }

        LoadVariables();

        return services;
    }

    private static void LoadVariables()
    {
        DatabaseConnectionString = Environment.GetEnvironmentVariable("TECHNEWS_AUTH_API_DATABASE_CONNECTION_STRING");
        
        DiscordWebhookId = Environment.GetEnvironmentVariable("TECHNEWS_AUTH_API_DISCORD_WEBHOOK_ID");
        DiscordWebhookToken = Environment.GetEnvironmentVariable("TECHNEWS_AUTH_API_DISCORD_WEBHOOK_TOKEN");
        
        CryptographicAlgorithm = Environment.GetEnvironmentVariable("TECHNEWS_AUTH_API_CRYPTOGRAPHIC_ALGORITHM");
        AzureKeyVaultUrl = Environment.GetEnvironmentVariable("TECHNEWS_AUTH_API_AZURE_KEY_VAULT_URL") ?? string.Empty;

        IssuerName = Environment.GetEnvironmentVariable("TECHNEWS_AUTH_API_ISSUER_NAME");
                
        BrokerHostName = Environment.GetEnvironmentVariable("TECHNEWS_AUTH_API_BROKER_HOST_NAME") ?? string.Empty;
        BrokerVirtualHost = Environment.GetEnvironmentVariable("TECHNEWS_AUTH_API_BROKER_VIRTUAL_HOST") ?? string.Empty;
        BrokerUserName = Environment.GetEnvironmentVariable("TECHNEWS_AUTH_API_BROKER_USER_NAME") ?? string.Empty;
        BrokerPassword = Environment.GetEnvironmentVariable("TECHNEWS_AUTH_API_BROKER_PASSWORD") ?? string.Empty;
        
        EventStoreConnectionString = Environment.GetEnvironmentVariable("TECHNEWS_AUTH_API_EVENT_STORE_CONNECTION_STRING") ?? string.Empty;

        int.TryParse(Environment.GetEnvironmentVariable("TECHNEWS_AUTH_API_TOKEN_EXPIRATION_IN_MINUTES"), out var parsedExpiration);

        if (parsedExpiration > 0)
        {
            TokenExpirationInMinutes = parsedExpiration;
        }

        int.TryParse(Environment.GetEnvironmentVariable("TECHNEWS_AUTH_API_KEY_ROTATOR_EXECUTION_IN_MINUTES"), out var parsedExecution);

        if (parsedExecution > 0)
        {
            KeyRotatorExecutionInMinutes = parsedExecution;
        }

        int.TryParse(Environment.GetEnvironmentVariable("TECHNEWS_AUTH_API_KEY_CREATION_SIZE_IN_BITS"), out var parsedSize);

        if (parsedSize > 0)
        {
            KeyCreationSizeInBits = parsedSize;
        }

        int.TryParse(Environment.GetEnvironmentVariable("TECHNEWS_AUTH_API_KEY_EXPIRATION_IN_DAYS"), out var parsedDays);

        if (parsedDays > 0)
        {
            KeyExpirationInDays = parsedDays;
        }
    }
}