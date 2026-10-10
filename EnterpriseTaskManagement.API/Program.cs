using EnterpriseTaskManagement.Application.Configuration;
using EnterpriseTaskManagement.Application.Interfaces.Repositories;
using EnterpriseTaskManagement.Application.Interfaces.Services;
using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Application.Services;
using EnterpriseTaskManagement.Application.Services.AuthService;
using EnterpriseTaskManagement.Application.Services.CommentService;
using EnterpriseTaskManagement.Application.Services.Interfaces;
using EnterpriseTaskManagement.Application.Services.RoleService;
using EnterpriseTaskManagement.Application.Services.UserService;
using EnterpriseTaskManagement.Persistence;
using EnterpriseTaskManagement.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using EnterpriseTaskManagement.Application.Services.TaskAnalysisService;
using EnterpriseTaskManagement.Infrastructure.DependencyInjection;
using DotNetEnv;

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
{
    Env.TraversePath().Load();
}

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    Console.WriteLine(
        $"AI provider configured: {!string.IsNullOrWhiteSpace(builder.Configuration["AI_PROVIDER"])}"
    );
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
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});


var aiSettings = new AISettings
{
    Provider = builder.Configuration["AI_PROVIDER"] ?? "Gemini",
    ApiKey = builder.Configuration["AI_API_KEY"] ?? string.Empty,
    Model = builder.Configuration["AI_MODEL"] ?? string.Empty,
    AgentPromptPath = Path.Combine(
        AppContext.BaseDirectory,
        "AI",
        "agent.md")
};

ServiceCollectionExtensions.ValidateAiSettings(aiSettings);

builder.Services.Configure<CloudinarySettings>(options =>
{
    options.CloudName = builder.Configuration["CLOUDINARY_CLOUD_NAME"] ?? string.Empty;
    options.ApiKey = builder.Configuration["CLOUDINARY_API_KEY"] ?? string.Empty;
    options.ApiSecret = builder.Configuration["CLOUDINARY_API_SECRET"] ?? string.Empty;
});

builder.Services.Configure<ImageUploadSettings>(_ => { });

ServiceCollectionExtensions.ValidateCloudinarySettings(
    new CloudinarySettings
    {
        CloudName = builder.Configuration["CLOUDINARY_CLOUD_NAME"] ?? string.Empty,
        ApiKey = builder.Configuration["CLOUDINARY_API_KEY"] ?? string.Empty,
        ApiSecret = builder.Configuration["CLOUDINARY_API_SECRET"] ?? string.Empty
    });

builder.Services.AddInfrastructureServices(aiSettings);
builder.Services.AddScoped<ITaskAnalysisService, EnterpriseTaskManagement.Application.Services.TaskAnalysisService.TaskAnalysisService>();
builder.Services.AddScoped<IAiTaskWorkflowService, AiTaskWorkflowService>();

var app = builder.Build();

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