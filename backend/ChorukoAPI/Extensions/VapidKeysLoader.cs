using System.Text.Json;
using ChorukoAPI.Contracts;

namespace ChorukoAPI.Extensions;

public static class VapidKeysLoader
{
    private const string VapidKeysFilePath = "wwwroot/uploads/vapid-keys.json";

    public static void LoadVapidKeys(IConfiguration configuration)
    {
        if (!File.Exists(VapidKeysFilePath))
        {
            Console.WriteLine("WARNING: vapid-keys.json file not found!");
            return;
        }

        try
        {
            var json = File.ReadAllText(VapidKeysFilePath);
            var vapidSettings = JsonSerializer.Deserialize<VapidSettings>(json);

            if (vapidSettings == null)
            {
                Console.WriteLine("WARNING: Failed to deserialize VAPID keys!");
                return;
            }

            // Update configuration with loaded keys
            configuration["VapidSettings:PublicKey"] = vapidSettings.PublicKey;
            configuration["VapidSettings:PrivateKey"] = vapidSettings.PrivateKey;
            configuration["VapidSettings:Subject"] = vapidSettings.Subject;

            Console.WriteLine($"VAPID keys loaded successfully from {VapidKeysFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to load VAPID keys: {ex.Message}");
        }
    }
}
