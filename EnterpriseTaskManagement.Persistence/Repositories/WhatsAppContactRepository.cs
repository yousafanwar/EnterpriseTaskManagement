using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseTaskManagement.Persistence.Repositories;

public class WhatsAppContactRepository : IWhatsAppContactRepository
{
    private readonly TaskDbContext _context;

    public WhatsAppContactRepository(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<WhatsAppContact?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.WhatsAppContacts
            .Include(c => c.Project)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<WhatsAppContact?> GetByPhoneNumberIdAsync(
        string phoneNumberId,
        CancellationToken cancellationToken = default)
    {
        return await _context.WhatsAppContacts
            .Include(c => c.Project)
            .FirstOrDefaultAsync(c => c.PhoneNumberId == phoneNumberId, cancellationToken);
    }

    public async Task<WhatsAppContact?> GetActiveByPhoneNumberIdAsync(
        string phoneNumberId,
        CancellationToken cancellationToken = default)
    {
        return await _context.WhatsAppContacts
            .Include(c => c.Project)
            .FirstOrDefaultAsync(
                c => c.PhoneNumberId == phoneNumberId && c.IsActive,
                cancellationToken);
    }

    public async Task<IReadOnlyList<WhatsAppContact>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.WhatsAppContacts
            .AsNoTracking()
            .Include(c => c.Project)
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        WhatsAppContact contact,
        CancellationToken cancellationToken = default)
    {
        await _context.WhatsAppContacts.AddAsync(contact, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        WhatsAppContact contact,
        CancellationToken cancellationToken = default)
    {
        _context.WhatsAppContacts.Update(contact);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        WhatsAppContact contact,
        CancellationToken cancellationToken = default)
    {
        _context.WhatsAppContacts.Remove(contact);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
