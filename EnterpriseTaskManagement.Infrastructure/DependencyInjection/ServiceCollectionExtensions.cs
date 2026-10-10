using EnterpriseTaskManagement.Application.Configuration;
using EnterpriseTaskManagement.Application.Services.AIProviderService;
using EnterpriseTaskManagement.Application.Services.ImageStorage;
using EnterpriseTaskManagement.Infrastructure.AI;
using EnterpriseTaskManagement.Infrastructure.ImageStorage;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseTaskManagement.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static bool IsSupportedAiProvider(string provider)
    {
        return provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase)
            || provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase);
    }

    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        AISettings aiSettings)
    {
        services.AddSingleton(aiSettings);

        // Typed clients own the configured HttpClient. Resolve IAIProvider from them
        // so the timeout below is applied (a separate AddScoped<T, TImpl> would inject
        // a default HttpClient with the 100s framework timeout instead).
        services.AddHttpClient<GeminiAiProvider>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        services.AddHttpClient<OpenAiAiProvider>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        var provider = aiSettings.Provider.Trim();
        if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IAIProvider>(sp => sp.GetRequiredService<GeminiAiProvider>());
        }
        else if (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IAIProvider>(sp => sp.GetRequiredService<OpenAiAiProvider>());
        }
        else
        {
            throw new InvalidOperationException(
                $"Unsupported AI provider '{aiSettings.Provider}'. Supported values: Gemini, OpenAI.");
        }

        services.AddScoped<IImageStorageService, CloudinaryImageStorageService>();

        return services;
    }

    public static void ValidateAiSettings(AISettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new InvalidOperationException(
                "AI_API_KEY is required.");
        }

        if (string.IsNullOrWhiteSpace(settings.Model))
        {
            throw new InvalidOperationException(
                "AI_MODEL is required.");
        }

        if (string.IsNullOrWhiteSpace(settings.AgentPromptPath)
            || !File.Exists(settings.AgentPromptPath))
        {
            throw new InvalidOperationException(
                "AI agent prompt file (agent.md) was not found at the configured path.");
        }
    }

    public static void ValidateCloudinarySettings(CloudinarySettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.CloudName)
            || string.IsNullOrWhiteSpace(settings.ApiKey)
            || string.IsNullOrWhiteSpace(settings.ApiSecret))
        {
            throw new InvalidOperationException(
                "Cloudinary credentials (CLOUDINARY_CLOUD_NAME, CLOUDINARY_API_KEY, CLOUDINARY_API_SECRET) are required.");
        }
    }
}
