namespace ChorukoAPI.Contracts;

public class VapidSettings
{
    public required string PublicKey { get; set; }

    public required string PrivateKey { get; set; }

    public required string Subject { get; set; }
}
