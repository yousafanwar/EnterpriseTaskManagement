using EnterpriseTaskManagement.Application.Configuration;
using EnterpriseTaskManagement.Application.Services.WhatsApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseTaskManagement.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/integrations/whatsapp")]
public class WhatsAppWebhookController : ControllerBase
{
    private readonly WhatsAppSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WhatsAppWebhookController> _logger;

    public WhatsAppWebhookController(
        WhatsAppSettings settings,
        IServiceScopeFactory scopeFactory,
        ILogger<WhatsAppWebhookController> logger)
    {
        _settings = settings;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Meta webhook verification challenge (Callback URL + Verify token).
    /// </summary>
    [HttpGet("webhook")]
    public IActionResult Verify(
        [FromQuery(Name = "hub.mode")] string? mode,
        [FromQuery(Name = "hub.verify_token")] string? verifyToken,
        [FromQuery(Name = "hub.challenge")] string? challenge)
    {
        if (!_settings.Enabled)
        {
            return NotFound();
        }

        if (string.Equals(mode, "subscribe", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrEmpty(verifyToken)
            && string.Equals(verifyToken, _settings.VerifyToken, StringComparison.Ordinal)
            && !string.IsNullOrEmpty(challenge))
        {
            return Content(challenge, "text/plain");
        }

        return Unauthorized();
    }

    /// <summary>
    /// Receives inbound WhatsApp messages. Returns 200 quickly; processes asynchronously.
    /// </summary>
    [HttpPost("webhook")]
    public async Task<IActionResult> Receive(CancellationToken cancellationToken)
    {
        if (!_settings.Enabled)
        {
            return NotFound();
        }

        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);
        Request.Body.Position = 0;

        var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
        if (!WhatsAppSignatureVerifier.IsValidSignature(rawBody, signature, _settings.AppSecret))
        {
            _logger.LogWarning("Rejected WhatsApp webhook with invalid signature.");
            return Unauthorized();
        }

        // Acknowledge immediately; Meta retries on slow/failed responses.
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IWhatsAppInboundHandler>();
                await handler.HandleWebhookPayloadAsync(rawBody, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background WhatsApp webhook processing failed.");
            }
        }, CancellationToken.None);

        return Ok();
    }
}
