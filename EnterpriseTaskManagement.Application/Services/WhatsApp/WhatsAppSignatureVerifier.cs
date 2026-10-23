using System.Security.Cryptography;
using System.Text;

namespace EnterpriseTaskManagement.Application.Services.WhatsApp;

public static class WhatsAppSignatureVerifier
{
    public static bool IsValidSignature(string rawBody, string? signatureHeader, string appSecret)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader)
            || string.IsNullOrWhiteSpace(appSecret)
            || !signatureHeader.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var providedHex = signatureHeader["sha256=".Length..].Trim();
        if (providedHex.Length == 0)
        {
            return false;
        }

        byte[] providedBytes;
        try
        {
            providedBytes = Convert.FromHexString(providedHex);
        }
        catch (FormatException)
        {
            return false;
        }

        var key = Encoding.UTF8.GetBytes(appSecret);
        var bodyBytes = Encoding.UTF8.GetBytes(rawBody);
        var computed = HMACSHA256.HashData(key, bodyBytes);

        return CryptographicOperations.FixedTimeEquals(computed, providedBytes);
    }
}
