namespace EnterpriseTaskManagement.Application.Configuration;

public class ImageUploadSettings
{
    public const string SectionName = "ImageUpload";

    public long MaxFileSizeBytes { get; set; } = 15 * 1024 * 1024;
}
