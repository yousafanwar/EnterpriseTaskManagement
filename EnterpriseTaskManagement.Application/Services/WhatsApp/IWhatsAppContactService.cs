using EnterpriseTaskManagement.Application.DTOs.WhatsApp;

namespace EnterpriseTaskManagement.Application.Services.WhatsApp;

public interface IWhatsAppContactService
{
    Task<IReadOnlyList<WhatsAppContactDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<WhatsAppContactDto> CreateAsync(
        CreateWhatsAppContactRequest request,
        CancellationToken cancellationToken = default);

    Task<WhatsAppContactDto> UpdateAsync(
        int id,
        UpdateWhatsAppContactRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
