using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseTaskManagement.Persistence.Repositories;

public class ProcessedWhatsAppMessageRepository : IProcessedWhatsAppMessageRepository
{
    private readonly TaskDbContext _context;

    public ProcessedWhatsAppMessageRepository(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(
        string messageId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ProcessedWhatsAppMessages
            .AsNoTracking()
            .AnyAsync(m => m.MessageId == messageId, cancellationToken);
    }

    public async Task AddAsync(
        ProcessedWhatsAppMessage message,
        CancellationToken cancellationToken = default)
    {
        await _context.ProcessedWhatsAppMessages.AddAsync(message, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
