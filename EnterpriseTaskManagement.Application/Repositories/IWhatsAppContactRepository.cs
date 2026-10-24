using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Repositories;

public interface IWhatsAppContactRepository
{
    Task<WhatsAppContact?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<WhatsAppContact?> GetByPhoneNumberIdAsync(
        string phoneNumberId,
        CancellationToken cancellationToken = default);

    Task<WhatsAppContact?> GetActiveByPhoneNumberIdAsync(
        string phoneNumberId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WhatsAppContact>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(WhatsAppContact contact, CancellationToken cancellationToken = default);

    Task UpdateAsync(WhatsAppContact contact, CancellationToken cancellationToken = default);

    Task DeleteAsync(WhatsAppContact contact, CancellationToken cancellationToken = default);
}
