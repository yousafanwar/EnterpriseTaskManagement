using EnterpriseTaskManagement.Application.Configuration;
using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EnterpriseTaskManagement.Application.Services.WhatsApp;

public interface IWhatsAppBotUserSeeder
{
    Task EnsureBotUserExistsAsync(CancellationToken cancellationToken = default);
}

public class WhatsAppBotUserSeeder : IWhatsAppBotUserSeeder
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<WhatsAppBotUserSeeder> _logger;

    public WhatsAppBotUserSeeder(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ILogger<WhatsAppBotUserSeeder> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _logger = logger;
    }

    public async Task EnsureBotUserExistsAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.GetByEmailAsync(WhatsAppSettings.BotEmail);
        if (existing is not null)
        {
            return;
        }

        var roles = await _roleRepository.GetAllAsync();
        var adminRole = roles.FirstOrDefault(r =>
            string.Equals(r.Name, "Admin", StringComparison.OrdinalIgnoreCase));

        if (adminRole is null)
        {
            throw new InvalidOperationException(
                "Cannot seed WhatsApp bot user because the Admin role was not found.");
        }

        // Unusable random hash — this account is not meant for interactive login.
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N"));

        var bot = new User(
            WhatsAppSettings.BotFirstName,
            WhatsAppSettings.BotLastName,
            WhatsAppSettings.BotEmail,
            adminRole.Id,
            passwordHash);

        await _userRepository.AddNewAsync(bot);

        _logger.LogInformation(
            "Seeded WhatsApp bot user {Email} with id {UserId}.",
            WhatsAppSettings.BotEmail,
            bot.Id);
    }
}
