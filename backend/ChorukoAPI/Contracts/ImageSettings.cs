namespace ChorukoAPI.Contracts;

public sealed class ImageSettings
{
    public const string SectionName = "ImageSettings";
    public long MaxFileSize { get; init; }
    public int MaxDimension { get; init; }
    public int MinDimension { get; init; }
    public required string[] AllowedFormats { get; init; }
}
