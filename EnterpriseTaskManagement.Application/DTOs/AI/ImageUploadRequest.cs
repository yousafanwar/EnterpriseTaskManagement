namespace EnterpriseTaskManagement.Application.DTOs.AI;

public class ImageUploadRequest
{
    public Stream Content { get; init; } = Stream.Null;

    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public long Length { get; init; }
}
