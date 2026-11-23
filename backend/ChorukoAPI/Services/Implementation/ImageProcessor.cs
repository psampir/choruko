using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Extensions;
using ChorukoAPI.Services.Abstraction;
using Microsoft.Extensions.Options;
using SkiaSharp;
using System.Net;

namespace ChorukoAPI.Services.Implementation;

public class ImageProcessor(
    IWebHostEnvironment webHostEnvironment,
    IOptions<ImageSettings> options) : IImageProcessor
{
    // Formats that may be decoded from uploaded content. The extension check is
    // not enough: the decoder detects the format from the bytes, so a TIFF named
    // ".png" must not reach a decoder outside this list.
    private static readonly HashSet<SKEncodedImageFormat> AllowedContentFormats =
    [
        SKEncodedImageFormat.Png,
        SKEncodedImageFormat.Jpeg,
        SKEncodedImageFormat.Webp,
        SKEncodedImageFormat.Gif,
    ];

    public async Task<ServiceResponse<string>> ProcessAsync(
        IFormFile? file,
        string subfolder,
        CancellationToken cancellationToken = default)
    {
        if (file == null)
        {
            return ServiceResponseFactory.Success<string>(null, "No profile picture provided.", HttpStatusCode.OK);
        }

        var settings = options.Value;

        if (file.Length > settings.MaxFileSize)
        {
            return ServiceResponseFactory.Error<string>(
                null,
                $"File size too large. Maximum allowed size is {settings.MaxFileSize / 1024 / 1024}MB.",
                HttpStatusCode.RequestEntityTooLarge);
        }

        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!settings.AllowedFormats.Contains(fileExtension))
        {
            return ServiceResponseFactory.Error<string>(
                null,
                $"Invalid file format. Only {string.Join(", ", settings.AllowedFormats)} are allowed.",
                HttpStatusCode.UnsupportedMediaType);
        }

        var uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "uploads", subfolder);
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}.jpg";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        try
        {
            await using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            using var encodedData = SKData.CreateCopy(memoryStream.ToArray());
            using var codec = SKCodec.Create(encodedData);

            if (codec == null || !AllowedContentFormats.Contains(codec.EncodedFormat))
            {
                return ServiceResponseFactory.Error<string>(
                    null,
                    "The file is not a valid or recognizable image.",
                    HttpStatusCode.UnsupportedMediaType);
            }

            if (codec.Info.Width < settings.MinDimension || codec.Info.Height < settings.MinDimension)
            {
                return ServiceResponseFactory.Error<string>(
                    null,
                    $"Image dimensions are too small. Minimum required: {settings.MinDimension}x{settings.MinDimension} pixels.",
                    HttpStatusCode.BadRequest);
            }

            using var decoded = SKBitmap.Decode(codec);
            if (decoded == null)
            {
                return ServiceResponseFactory.Error<string>(
                    null,
                    "The file is not a valid or recognizable image.",
                    HttpStatusCode.UnsupportedMediaType);
            }

            using var output = ResizeIfNeeded(decoded, settings.MaxDimension);
            using var image = SKImage.FromBitmap(output);
            using var jpeg = image.Encode(SKEncodedImageFormat.Jpeg, 100);

            if (jpeg == null)
            {
                return ServiceResponseFactory.Error<string>(
                    null,
                    "Failed to process image.",
                    HttpStatusCode.InternalServerError);
            }

            await using var fileStream = File.Create(filePath);
            jpeg.SaveTo(fileStream);

            var relativePath = Path.Combine("uploads", subfolder, uniqueFileName).Replace("\\", "/");
            return ServiceResponseFactory.Success(relativePath, "Profile picture processed successfully.", HttpStatusCode.OK);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or IOException)
        {
            return ServiceResponseFactory.Error<string>(
                null,
                "The file is not a valid or recognizable image.",
                HttpStatusCode.UnsupportedMediaType);
        }
        catch (Exception ex)
        {
            return ServiceResponseFactory.Error<string>(
                null,
                $"Failed to process image: {ex.Message}",
                HttpStatusCode.InternalServerError);
        }
    }

    public void Delete(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

        var fullPath = Path.Combine(webHostEnvironment.WebRootPath, relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    private static SKBitmap ResizeIfNeeded(SKBitmap source, int maxDimension)
    {
        if (source.Width <= maxDimension && source.Height <= maxDimension)
        {
            return source.Copy();
        }

        // Equivalent of ImageSharp's ResizeMode.Crop: take the largest centered
        // square and scale it to maxDimension x maxDimension.
        var side = Math.Min(source.Width, source.Height);
        var left = (source.Width - side) / 2f;
        var top = (source.Height - side) / 2f;

        var info = new SKImageInfo(maxDimension, maxDimension, SKColorType.Rgba8888, SKAlphaType.Opaque);
        var result = new SKBitmap(info);

        using var canvas = new SKCanvas(result);
        canvas.Clear(SKColors.White);
        canvas.DrawBitmap(
            source,
            new SKRect(left, top, left + side, top + side),
            new SKRect(0, 0, maxDimension, maxDimension),
            new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
        canvas.Flush();

        return result;
    }
}
