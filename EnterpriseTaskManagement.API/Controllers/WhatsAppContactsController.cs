using EnterpriseTaskManagement.Application.Configuration;
using EnterpriseTaskManagement.Application.DTOs.WhatsApp;
using EnterpriseTaskManagement.Application.Services.WhatsApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseTaskManagement.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/whatsapp/contacts")]
public class WhatsAppContactsController : ControllerBase
{
    private readonly IWhatsAppContactService _contactService;
    private readonly WhatsAppSettings _settings;

    public WhatsAppContactsController(
        IWhatsAppContactService contactService,
        WhatsAppSettings settings)
    {
        _contactService = contactService;
        _settings = settings;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WhatsAppContactDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        EnsureEnabled();
        var contacts = await _contactService.GetAllAsync(cancellationToken);
        return Ok(contacts);
    }

    [HttpPost]
    public async Task<ActionResult<WhatsAppContactDto>> Create(
        [FromBody] CreateWhatsAppContactRequest request,
        CancellationToken cancellationToken)
    {
        EnsureEnabled();
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var created = await _contactService.CreateAsync(request, cancellationToken);
        return Created($"/api/whatsapp/contacts/{created.Id}", created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<WhatsAppContactDto>> Update(
        int id,
        [FromBody] UpdateWhatsAppContactRequest request,
        CancellationToken cancellationToken)
    {
        EnsureEnabled();
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var updated = await _contactService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        EnsureEnabled();
        await _contactService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private void EnsureEnabled()
    {
        if (!_settings.Enabled)
        {
            throw new InvalidOperationException(
                "WhatsApp integration is disabled. Set WHATSAPP_ENABLED=true to manage contacts.");
        }
    }
}
