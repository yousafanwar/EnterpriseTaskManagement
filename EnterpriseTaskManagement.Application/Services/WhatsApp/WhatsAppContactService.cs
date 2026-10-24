using EnterpriseTaskManagement.Application.DTOs.WhatsApp;
using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Services.WhatsApp;

public class WhatsAppContactService : IWhatsAppContactService
{
    private readonly IWhatsAppContactRepository _contactRepository;
    private readonly IProjectRepository _projectRepository;

    public WhatsAppContactService(
        IWhatsAppContactRepository contactRepository,
        IProjectRepository projectRepository)
    {
        _contactRepository = contactRepository;
        _projectRepository = projectRepository;
    }

    public async Task<IReadOnlyList<WhatsAppContactDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var contacts = await _contactRepository.GetAllAsync(cancellationToken);
        return contacts.Select(Map).ToList();
    }

    public async Task<WhatsAppContactDto> CreateAsync(
        CreateWhatsAppContactRequest request,
        CancellationToken cancellationToken = default)
    {
        var phoneNumberId = request.PhoneNumberId.Trim();
        if (string.IsNullOrWhiteSpace(phoneNumberId))
        {
            throw new ArgumentException("PhoneNumberId is required.", nameof(request.PhoneNumberId));
        }

        var project = await _projectRepository.GetByIdAsync(request.ProjectId);
        if (project is null)
        {
            throw new KeyNotFoundException($"Project with id {request.ProjectId} was not found.");
        }

        var existing = await _contactRepository.GetByPhoneNumberIdAsync(
            phoneNumberId,
            cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException(
                $"A WhatsApp contact for phone_number_id '{phoneNumberId}' already exists.");
        }

        var contact = new WhatsAppContact(phoneNumberId, request.ProjectId, request.Label);
        try
        {
            await _contactRepository.AddAsync(contact, cancellationToken);
        }
        catch (Exception ex) when (IsUniqueViolation(ex))
        {
            throw new InvalidOperationException(
                $"A WhatsApp contact for phone_number_id '{phoneNumberId}' already exists.",
                ex);
        }

        var created = await _contactRepository.GetByIdAsync(contact.Id, cancellationToken)
            ?? contact;

        return Map(created);
    }

    public async Task<WhatsAppContactDto> UpdateAsync(
        int id,
        UpdateWhatsAppContactRequest request,
        CancellationToken cancellationToken = default)
    {
        var contact = await _contactRepository.GetByIdAsync(id, cancellationToken);
        if (contact is null)
        {
            throw new KeyNotFoundException($"WhatsApp contact with id {id} was not found.");
        }

        var project = await _projectRepository.GetByIdAsync(request.ProjectId);
        if (project is null)
        {
            throw new KeyNotFoundException($"Project with id {request.ProjectId} was not found.");
        }

        contact.Update(request.ProjectId, request.Label, request.IsActive);
        await _contactRepository.UpdateAsync(contact, cancellationToken);

        var updated = await _contactRepository.GetByIdAsync(id, cancellationToken) ?? contact;
        return Map(updated);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var contact = await _contactRepository.GetByIdAsync(id, cancellationToken);
        if (contact is null)
        {
            throw new KeyNotFoundException($"WhatsApp contact with id {id} was not found.");
        }

        await _contactRepository.DeleteAsync(contact, cancellationToken);
    }

    private static WhatsAppContactDto Map(WhatsAppContact contact)
    {
        return new WhatsAppContactDto
        {
            Id = contact.Id,
            PhoneNumberId = contact.PhoneNumberId,
            ProjectId = contact.ProjectId,
            ProjectName = contact.Project?.Name,
            Label = contact.Label,
            IsActive = contact.IsActive,
            CreatedAtUtc = contact.CreatedAtUtc
        };
    }

    private static bool IsUniqueViolation(Exception ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("IX_WhatsAppContacts_PhoneNumberId", StringComparison.OrdinalIgnoreCase)
            || message.Contains("unique", StringComparison.OrdinalIgnoreCase);
    }
}
