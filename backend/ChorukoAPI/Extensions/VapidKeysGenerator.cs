using System.Text.Json;
using ChorukoAPI.Contracts;
using WebPush;

namespace ChorukoAPI.Extensions;

public static class VapidKeysGenerator
{
    private const string VapidKeysFilePath = "wwwroot/uploads/vapid-keys.json";

    public static void GenerateKeys(string subject = "mailto:admin@choruko.app")
    {
        // Ensure the directory exists
        var directory = Path.GetDirectoryName(VapidKeysFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var vapidKeys = VapidHelper.GenerateVapidKeys();

        var vapidSettings = new VapidSettings
        {
            PublicKey = vapidKeys.PublicKey,
            PrivateKey = vapidKeys.PrivateKey,
            Subject = subject
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(vapidSettings, options);
        File.WriteAllText(VapidKeysFilePath, json);

        Console.WriteLine("VAPID keys generated and saved to vapid-keys.json");
        Console.WriteLine($"Public Key: {vapidKeys.PublicKey}");
    }
}
