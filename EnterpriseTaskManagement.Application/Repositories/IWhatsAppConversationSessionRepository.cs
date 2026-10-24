using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Repositories;

public interface IWhatsAppConversationSessionRepository
{
    Task<WhatsAppConversationSession?> GetAsync(
        string businessPhoneNumberId,
        string senderWaId,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(
        WhatsAppConversationSession session,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string businessPhoneNumberId,
        string senderWaId,
        CancellationToken cancellationToken = default);
}
