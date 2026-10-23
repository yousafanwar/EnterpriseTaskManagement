using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Repositories;

public interface IProcessedWhatsAppMessageRepository
{
    Task<bool> ExistsAsync(string messageId, CancellationToken cancellationToken = default);

    Task AddAsync(ProcessedWhatsAppMessage message, CancellationToken cancellationToken = default);
}
