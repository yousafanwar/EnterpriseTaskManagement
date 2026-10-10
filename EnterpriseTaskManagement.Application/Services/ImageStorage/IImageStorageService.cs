using EnterpriseTaskManagement.Application.DTOs.AI;

namespace EnterpriseTaskManagement.Application.Services.ImageStorage;

public interface IImageStorageService
{
    Task<ImageUploadResult> UploadAsync(
        ImageUploadRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string publicId,
        CancellationToken cancellationToken = default);
}
