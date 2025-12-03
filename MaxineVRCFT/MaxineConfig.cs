using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace VRCFaceTracking.Maxine;

public class MaxineConfig
{
    public string Host { get; set; } = "127.0.0.1";

    public int Port { get; set; } = 9000;

    private const string ConfigFileName = "MaxineConfig.json";

    public static MaxineConfig Load(ILogger logger)
    {
        string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string path = Path.Combine(directoryName, ConfigFileName);

        try
        {
            if (File.Exists(path))
            {
                var loaded = JsonConvert.DeserializeObject<MaxineConfig>(File.ReadAllText(path));
                if (loaded != null)
                    return loaded;
            }

            var fallback = new MaxineConfig();
            File.WriteAllText(path, JsonConvert.SerializeObject(fallback, Formatting.Indented));
            logger.LogWarning($"設定ファイルが見つからなかったため {path} にデフォルトを作成しました。");
            return fallback;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "設定ファイルの読み込みに失敗したためデフォルトを使用します。");
            return new MaxineConfig();
        }
    }
}
