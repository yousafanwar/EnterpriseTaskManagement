using EnterpriseTaskManagement.Application.DTOs.AI;
using EnterpriseTaskManagement.Application.Services.ImageStorage;

namespace EnterpriseTaskManagement.Infrastructure.ImageStorage;

public sealed class DisabledImageStorageService : IImageStorageService
{
    public Task<ImageUploadResult> UploadAsync(
        ImageUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException(
            "Image upload is disabled because AI features are turned off. Set AI_ENABLED=true and configure Cloudinary credentials.");
    }

    public Task DeleteAsync(
        string publicId,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
