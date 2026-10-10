using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EnterpriseTaskManagement.Application.Configuration;
using EnterpriseTaskManagement.Application.DTOs.AI;
using EnterpriseTaskManagement.Application.Services.ImageStorage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnterpriseTaskManagement.Infrastructure.ImageStorage;

public class CloudinaryImageStorageService : IImageStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;
    private readonly ImageUploadSettings _uploadSettings;
    private readonly ILogger<CloudinaryImageStorageService> _logger;

    public CloudinaryImageStorageService(
        IOptions<CloudinarySettings> settings,
        IOptions<ImageUploadSettings> uploadSettings,
        ILogger<CloudinaryImageStorageService> logger)
    {
        _settings = settings.Value;
        _uploadSettings = uploadSettings.Value;
        _logger = logger;

        var account = new Account(
            _settings.CloudName,
            _settings.ApiKey,
            _settings.ApiSecret);

        _cloudinary = new Cloudinary(account);
    }

    public async Task<Application.DTOs.AI.ImageUploadResult> UploadAsync(
        ImageUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Length <= 0)
        {
            throw new ArgumentException("Image file is empty.");
        }

        if (request.Length > _uploadSettings.MaxFileSizeBytes)
        {
            throw new ArgumentException(
                $"Image exceeds the maximum allowed size of {_uploadSettings.MaxFileSizeBytes} bytes.");
        }

        if (!ImageContentValidator.TryDetectContentType(request.Content, out var detectedType)
            || !ImageContentValidator.IsAllowedContentType(detectedType))
        {
            throw new ArgumentException(
                "Unsupported or invalid image format. Allowed formats: JPEG, PNG, GIF, WebP.");
        }

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(request.FileName, request.Content),
            Folder = _settings.UploadFolder
        };

        cancellationToken.ThrowIfCancellationRequested();
        var response = await _cloudinary.UploadAsync(uploadParams);

        if (response.Error is not null)
        {
            _logger.LogWarning(
                "Cloudinary upload failed with status {StatusCode}: {ErrorMessage}",
                response.StatusCode,
                response.Error.Message);

            throw new InvalidOperationException(
                $"Image upload failed: {response.Error.Message}");
        }

        return new Application.DTOs.AI.ImageUploadResult
        {
            Url = response.SecureUrl?.ToString() ?? string.Empty,
            PublicId = response.PublicId ?? string.Empty
        };
    }

    public async Task DeleteAsync(
        string publicId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));

        if (result.Result != "ok" && result.Result != "not found")
        {
            _logger.LogWarning(
                "Unexpected Cloudinary delete result {Result} for public id.",
                result.Result);
        }
    }
}
