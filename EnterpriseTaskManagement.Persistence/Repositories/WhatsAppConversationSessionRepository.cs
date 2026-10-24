using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseTaskManagement.Persistence.Repositories;

public class WhatsAppConversationSessionRepository : IWhatsAppConversationSessionRepository
{
    private readonly TaskDbContext _context;

    public WhatsAppConversationSessionRepository(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<WhatsAppConversationSession?> GetAsync(
        string businessPhoneNumberId,
        string senderWaId,
        CancellationToken cancellationToken = default)
    {
        return await _context.WhatsAppConversationSessions
            .FirstOrDefaultAsync(
                s => s.BusinessPhoneNumberId == businessPhoneNumberId
                    && s.SenderWaId == senderWaId,
                cancellationToken);
    }

    public async Task UpsertAsync(
        WhatsAppConversationSession session,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.WhatsAppConversationSessions
            .FirstOrDefaultAsync(
                s => s.BusinessPhoneNumberId == session.BusinessPhoneNumberId
                    && s.SenderWaId == session.SenderWaId,
                cancellationToken);

        if (existing is null)
        {
            await _context.WhatsAppConversationSessions.AddAsync(session, cancellationToken);
        }
        else
        {
            existing.SelectProject(
                session.SelectedProjectId,
                session.SelectedProjectName);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        string businessPhoneNumberId,
        string senderWaId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.WhatsAppConversationSessions
            .FirstOrDefaultAsync(
                s => s.BusinessPhoneNumberId == businessPhoneNumberId
                    && s.SenderWaId == senderWaId,
                cancellationToken);

        if (existing is null)
        {
            return;
        }

        _context.WhatsAppConversationSessions.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
