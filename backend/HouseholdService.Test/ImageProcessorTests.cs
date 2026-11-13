using System.Net;
using ChorukoAPI.Contracts;
using ChorukoAPI.Services.Implementation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using SkiaSharp;

namespace HouseholdService.Test;

public class ImageProcessorTests : IDisposable
{
    private readonly string _root;
    private readonly ImageProcessor _processor;

    public ImageProcessorTests()
    {
        _root = Path.Combine(Path.GetTempPath(), $"choruko-img-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_root);

        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(x => x.WebRootPath).Returns(_root);

        _processor = new ImageProcessor(environment.Object, Options.Create(new ImageSettings
        {
            MaxFileSize = 1024 * 1024,
            MinDimension = 150,
            MaxDimension = 400,
            AllowedFormats = [".png", ".jpg", ".jpeg", ".webp", ".gif"],
        }));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }
    }

    [Fact]
    public async Task ProcessAsync_ValidPng_ReturnsJpegWithSameDimensions()
    {
        var result = await _processor.ProcessAsync(
            FormFile(CreateImage(300, 300, SKEncodedImageFormat.Png), "avatar.png"),
            "users");

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Result);
        Assert.StartsWith("uploads/users/", result.Result);

        using var codec = OpenCodec(result.Result!);
        Assert.Equal(SKEncodedImageFormat.Jpeg, codec.EncodedFormat);
        Assert.Equal(300, codec.Info.Width);
        Assert.Equal(300, codec.Info.Height);
    }

    [Fact]
    public async Task ProcessAsync_LargeImage_CropsToMaxSquare()
    {
        var result = await _processor.ProcessAsync(
            FormFile(CreateImage(800, 600, SKEncodedImageFormat.Png), "big.png"),
            "households");

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Result);

        using var codec = OpenCodec(result.Result!);
        Assert.Equal(SKEncodedImageFormat.Jpeg, codec.EncodedFormat);
        Assert.Equal(400, codec.Info.Width);
        Assert.Equal(400, codec.Info.Height);
    }

    [Fact]
    public async Task ProcessAsync_SmallImage_ReturnsBadRequest()
    {
        var result = await _processor.ProcessAsync(
            FormFile(CreateImage(100, 100, SKEncodedImageFormat.Png), "small.png"),
            "users");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("too small", result.Message);
    }

    [Fact]
    public async Task ProcessAsync_DisallowedExtension_ReturnsUnsupportedMediaType()
    {
        var result = await _processor.ProcessAsync(
            FormFile(CreateImage(300, 300, SKEncodedImageFormat.Png), "avatar.txt"),
            "users");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, result.StatusCode);
    }

    [Fact]
    public async Task ProcessAsync_DisallowedContent_ReturnsUnsupportedMediaType()
    {
        // BMP content behind a .png name: the extension passes, the detected format must not.
        var result = await _processor.ProcessAsync(
            FormFile(CreateBmp(), "avatar.png"),
            "users");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, result.StatusCode);
    }

    [Fact]
    public async Task ProcessAsync_CorruptFile_ReturnsUnsupportedMediaType()
    {
        var result = await _processor.ProcessAsync(
            FormFile([1, 2, 3, 4], "avatar.png"),
            "users");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, result.StatusCode);
    }

    [Fact]
    public async Task ProcessAsync_TooLargeFile_ReturnsRequestEntityTooLarge()
    {
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(x => x.WebRootPath).Returns(_root);

        var processor = new ImageProcessor(environment.Object, Options.Create(new ImageSettings
        {
            MaxFileSize = 10,
            MinDimension = 150,
            MaxDimension = 400,
            AllowedFormats = [".png"],
        }));

        var result = await processor.ProcessAsync(
            FormFile(CreateImage(300, 300, SKEncodedImageFormat.Png), "avatar.png"),
            "users");

        Assert.False(result.IsSuccessful);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, result.StatusCode);
    }

    [Fact]
    public async Task ProcessAsync_ValidJpeg_ReturnsJpeg()
    {
        var result = await _processor.ProcessAsync(
            FormFile(CreateImage(300, 300, SKEncodedImageFormat.Jpeg), "avatar.jpg"),
            "users");

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Result);

        using var codec = OpenCodec(result.Result!);
        Assert.Equal(SKEncodedImageFormat.Jpeg, codec.EncodedFormat);
    }

    [Fact]
    public async Task ProcessAsync_NoFile_ReturnsSuccessWithoutPath()
    {
        var result = await _processor.ProcessAsync(null, "users");

        Assert.True(result.IsSuccessful);
        Assert.Null(result.Result);
    }

    [Fact]
    public async Task Delete_RemovesExistingFile()
    {
        var result = await _processor.ProcessAsync(
            FormFile(CreateImage(300, 300, SKEncodedImageFormat.Png), "avatar.png"),
            "users");

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Result);
        var fullPath = Path.Combine(_root, result.Result!);
        Assert.True(File.Exists(fullPath));

        _processor.Delete(result.Result);

        Assert.False(File.Exists(fullPath));
    }

    private static byte[] CreateImage(int width, int height, SKEncodedImageFormat format)
    {
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.CornflowerBlue);
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, 100);
        Assert.NotNull(data);
        return data!.ToArray();
    }

    private static byte[] CreateBmp()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // Minimal valid 1x1 24-bit BMP.
        writer.Write((byte)'B');
        writer.Write((byte)'M');
        writer.Write(58);          // file size
        writer.Write(0);           // reserved
        writer.Write(54);          // pixel data offset
        writer.Write(40);          // DIB header size
        writer.Write(1);           // width
        writer.Write(1);           // height
        writer.Write((short)1);    // planes
        writer.Write((short)24);   // bits per pixel
        writer.Write(0);           // compression
        writer.Write(4);           // image size
        writer.Write(0);           // x pixels per meter
        writer.Write(0);           // y pixels per meter
        writer.Write(0);           // colors used
        writer.Write(0);           // important colors
        writer.Write(new byte[] { 0x00, 0x00, 0xFF, 0x00 });
        writer.Flush();

        return stream.ToArray();
    }

    private static IFormFile FormFile(byte[] bytes, string fileName) =>
        new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", fileName);

    private SKCodec OpenCodec(string relativePath)
    {
        var bytes = File.ReadAllBytes(Path.Combine(_root, relativePath));
        var data = SKData.CreateCopy(bytes);
        var codec = SKCodec.Create(data);
        Assert.NotNull(codec);
        return codec!;
    }
}
