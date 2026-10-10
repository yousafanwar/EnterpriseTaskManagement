namespace EnterpriseTaskManagement.Application.Configuration;

public class ImageUploadSettings
{
    public const string SectionName = "ImageUpload";

    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
}
