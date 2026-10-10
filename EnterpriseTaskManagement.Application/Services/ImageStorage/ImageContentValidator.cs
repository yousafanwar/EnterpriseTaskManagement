namespace EnterpriseTaskManagement.Application.Services.ImageStorage;

public static class ImageContentValidator
{
    private static readonly Dictionary<string, byte[][]> Signatures = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = [[0xFF, 0xD8, 0xFF]],
        ["image/png"] = [[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]],
        ["image/gif"] = [[0x47, 0x49, 0x46, 0x38, 0x37, 0x61], [0x47, 0x49, 0x46, 0x38, 0x39, 0x61]],
        ["image/webp"] = [[0x52, 0x49, 0x46, 0x46]]
    };

    public static bool TryDetectContentType(Stream stream, out string contentType)
    {
        contentType = string.Empty;
        if (!stream.CanRead)
        {
            return false;
        }

        var originalPosition = stream.CanSeek ? stream.Position : 0;
        Span<byte> header = stackalloc byte[12];
        var read = stream.Read(header);
        if (stream.CanSeek)
        {
            stream.Position = originalPosition;
        }

        if (read < 4)
        {
            return false;
        }

        foreach (var (mime, patterns) in Signatures)
        {
            foreach (var pattern in patterns)
            {
                if (read >= pattern.Length && header[..pattern.Length].SequenceEqual(pattern))
                {
                    if (mime == "image/webp" && read >= 12)
                    {
                        var webp = header[8..12];
                        if (!webp.SequenceEqual("WEBP"u8))
                        {
                            continue;
                        }
                    }

                    contentType = mime;
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsAllowedContentType(string contentType)
    {
        return Signatures.ContainsKey(contentType);
    }
}
