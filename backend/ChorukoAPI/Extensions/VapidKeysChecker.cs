using System.Text.Json;
using ChorukoAPI.Contracts;

namespace ChorukoAPI.Extensions;

public static class VapidKeysChecker
{
    private const string VapidKeysFilePath = "vapid-keys.json";

    public static bool CheckVapidKeys()
    {
        if (!File.Exists(VapidKeysFilePath))
        {
            return false;
        }

        try
        {
            var json = File.ReadAllText(VapidKeysFilePath);
            var vapidSettings = JsonSerializer.Deserialize<VapidSettings>(json);

            return vapidSettings != null 
                   && !string.IsNullOrWhiteSpace(vapidSettings.PublicKey) 
                   && !string.IsNullOrWhiteSpace(vapidSettings.PrivateKey);
        }
        catch
        {
            return false;
        }
    }
}
