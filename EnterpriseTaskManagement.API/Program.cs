using EnterpriseTaskManagement.Application.Configuration;
using EnterpriseTaskManagement.Application.Interfaces.Repositories;
using EnterpriseTaskManagement.Application.Interfaces.Services;
using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Application.Services;
using EnterpriseTaskManagement.Application.Services.AIProviderService;
using EnterpriseTaskManagement.Application.Services.AuthService;
using EnterpriseTaskManagement.Application.Services.CommentService;
using EnterpriseTaskManagement.Application.Services.ImageStorage;
using EnterpriseTaskManagement.Application.Services.Interfaces;
using EnterpriseTaskManagement.Application.Services.RoleService;
using EnterpriseTaskManagement.Application.Services.TaskAnalysisService;
using EnterpriseTaskManagement.Application.Services.UserService;
using EnterpriseTaskManagement.Application.Services.WhatsApp;
using EnterpriseTaskManagement.Infrastructure.AI;
using EnterpriseTaskManagement.Infrastructure.ImageStorage;
using EnterpriseTaskManagement.Infrastructure.WhatsApp;
using EnterpriseTaskManagement.Persistence;
using EnterpriseTaskManagement.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using DotNetEnv;

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
{
    Env.TraversePath().Load();
}

var builder = WebApplication.CreateBuilder(args);

var aiEnabled = IsFeatureFlagEnabled(builder.Configuration["AI_ENABLED"]);
var whatsAppEnabled = IsFeatureFlagEnabled(builder.Configuration["WHATSAPP_ENABLED"]);

if (builder.Environment.IsDevelopment())
{
    Console.WriteLine($"AI features enabled: {aiEnabled}");
    Console.WriteLine($"WhatsApp integration enabled: {whatsAppEnabled}");
}

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ITaskHistoryRepository, TaskHistoryRepository>();
builder.Services.AddScoped<ITaskHistoryService, TaskHistoryService>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectSkillRepository, ProjectSkillRepository>();
builder.Services.AddScoped<IProjectUserRepository, ProjectUserRepository>();
builder.Services.AddScoped<IUserSkillRepository, UserSkillRepository>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IWhatsAppContactRepository, WhatsAppContactRepository>();
builder.Services.AddScoped<IProcessedWhatsAppMessageRepository, ProcessedWhatsAppMessageRepository>();
builder.Services.AddScoped<IWhatsAppConversationSessionRepository, WhatsAppConversationSessionRepository>();
builder.Services.AddScoped<IWhatsAppContactService, WhatsAppContactService>();
builder.Services.AddScoped<IWhatsAppInboundHandler, WhatsAppInboundHandler>();
builder.Services.AddScoped<IWhatsAppBotUserSeeder, WhatsAppBotUserSeeder>();

// Register Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TaskDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]!
                )
            )
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(
                  "http://localhost:5173",
                  "https://projectmanagementfrontend.omnisoft.info")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var aiSettings = new AISettings
{
    Enabled = aiEnabled,
    Provider = builder.Configuration["AI_PROVIDER"] ?? "Gemini",
    ApiKey = builder.Configuration["AI_API_KEY"] ?? string.Empty,
    Model = builder.Configuration["AI_MODEL"] ?? string.Empty,
    AgentPromptPath = Path.Combine(
        AppContext.BaseDirectory,
        "AI",
        "agent.md")
};

builder.Services.AddSingleton(aiSettings);
builder.Services.Configure<ImageUploadSettings>(_ => { });

if (aiSettings.Enabled)
{
    if (string.IsNullOrWhiteSpace(aiSettings.ApiKey))
    {
        throw new InvalidOperationException(
            "AI_API_KEY is required when AI_ENABLED=true.");
    }

    if (string.IsNullOrWhiteSpace(aiSettings.Model))
    {
        throw new InvalidOperationException(
            "AI_MODEL is required when AI_ENABLED=true.");
    }

    if (string.IsNullOrWhiteSpace(aiSettings.AgentPromptPath)
        || !File.Exists(aiSettings.AgentPromptPath))
    {
        throw new InvalidOperationException(
            "AI agent prompt file (agent.md) was not found at the configured path.");
    }

    var cloudinarySettings = new CloudinarySettings
    {
        CloudName = builder.Configuration["CLOUDINARY_CLOUD_NAME"] ?? string.Empty,
        ApiKey = builder.Configuration["CLOUDINARY_API_KEY"] ?? string.Empty,
        ApiSecret = builder.Configuration["CLOUDINARY_API_SECRET"] ?? string.Empty
    };

    if (string.IsNullOrWhiteSpace(cloudinarySettings.CloudName)
        || string.IsNullOrWhiteSpace(cloudinarySettings.ApiKey)
        || string.IsNullOrWhiteSpace(cloudinarySettings.ApiSecret))
    {
        throw new InvalidOperationException(
            "Cloudinary credentials (CLOUDINARY_CLOUD_NAME, CLOUDINARY_API_KEY, CLOUDINARY_API_SECRET) are required when AI_ENABLED=true.");
    }

    builder.Services.Configure<CloudinarySettings>(options =>
    {
        options.CloudName = cloudinarySettings.CloudName;
        options.ApiKey = cloudinarySettings.ApiKey;
        options.ApiSecret = cloudinarySettings.ApiSecret;
    });

    builder.Services.AddHttpClient<GeminiAiProvider>(client =>
    {
        client.Timeout = TimeSpan.FromMinutes(5);
    });

    builder.Services.AddHttpClient<OpenAiAiProvider>(client =>
    {
        client.Timeout = TimeSpan.FromMinutes(5);
    });

    var provider = aiSettings.Provider.Trim();
    if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
    {
        builder.Services.AddScoped<IAIProvider>(sp => sp.GetRequiredService<GeminiAiProvider>());
    }
    else if (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
    {
        builder.Services.AddScoped<IAIProvider>(sp => sp.GetRequiredService<OpenAiAiProvider>());
    }
    else
    {
        throw new InvalidOperationException(
            $"Unsupported AI provider '{aiSettings.Provider}'. Supported values: Gemini, OpenAI.");
    }

    builder.Services.AddScoped<IImageStorageService, CloudinaryImageStorageService>();
}
else
{
    builder.Services.AddScoped<IAIProvider, DisabledAiProvider>();
    builder.Services.AddScoped<IImageStorageService, DisabledImageStorageService>();
}

builder.Services.AddScoped<ITaskAnalysisService, TaskAnalysisService>();
builder.Services.AddScoped<IAiTaskWorkflowService, AiTaskWorkflowService>();

var whatsAppSettings = new WhatsAppSettings
{
    Enabled = whatsAppEnabled,
    VerifyToken = builder.Configuration["WHATSAPP_VERIFY_TOKEN"] ?? string.Empty,
    AppSecret = builder.Configuration["WHATSAPP_APP_SECRET"] ?? string.Empty,
    AccessToken = builder.Configuration["WHATSAPP_ACCESS_TOKEN"] ?? string.Empty,
    GraphApiBaseUrl = builder.Configuration["WHATSAPP_GRAPH_API_BASE_URL"]
        ?? "https://graph.facebook.com/v21.0",
    SendReplies = IsFeatureFlagEnabled(
        builder.Configuration["WHATSAPP_SEND_REPLIES"] ?? "true")
};

builder.Services.AddSingleton(whatsAppSettings);

if (whatsAppSettings.Enabled)
{
    if (string.IsNullOrWhiteSpace(whatsAppSettings.VerifyToken))
    {
        throw new InvalidOperationException(
            "WHATSAPP_VERIFY_TOKEN is required when WHATSAPP_ENABLED=true.");
    }

    if (string.IsNullOrWhiteSpace(whatsAppSettings.AppSecret))
    {
        throw new InvalidOperationException(
            "WHATSAPP_APP_SECRET is required when WHATSAPP_ENABLED=true.");
    }

    if (string.IsNullOrWhiteSpace(whatsAppSettings.AccessToken))
    {
        throw new InvalidOperationException(
            "WHATSAPP_ACCESS_TOKEN is required when WHATSAPP_ENABLED=true.");
    }

    if (!aiEnabled)
    {
        throw new InvalidOperationException(
            "AI_ENABLED must be true when WHATSAPP_ENABLED=true (WhatsApp creates tasks via AI analysis).");
    }

    builder.Services.AddHttpClient<IWhatsAppCloudApiClient, WhatsAppCloudApiClient>(client =>
    {
        client.Timeout = TimeSpan.FromMinutes(2);
    });
}
else
{
    builder.Services.AddScoped<IWhatsAppCloudApiClient, DisabledWhatsAppCloudApiClient>();
}

var app = builder.Build();

if (whatsAppSettings.Enabled)
{
    using var scope = app.Services.CreateScope();
    var botSeeder = scope.ServiceProvider.GetRequiredService<IWhatsAppBotUserSeeder>();
    botSeeder.EnsureBotUserExistsAsync().GetAwaiter().GetResult();
}

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseExceptionHandler("/error");

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseCors("Frontend");

app.Run();

static bool IsFeatureFlagEnabled(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return false;
    }

    return value.Equals("true", StringComparison.OrdinalIgnoreCase)
        || value.Equals("1", StringComparison.OrdinalIgnoreCase)
        || value.Equals("yes", StringComparison.OrdinalIgnoreCase);
}
