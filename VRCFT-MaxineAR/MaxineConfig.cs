using System.IO;
using System.Reflection;
using System.Text.Json;

namespace VRCFaceTracking.MaxineAR;

public class MaxineConfig
{
    private const string ConfigFileName = "MaxineARConfig.json";

    public string Host { get; set; } = MaxineUdpReceiver.DefaultHost;

    public int Port { get; set; } = MaxineUdpReceiver.DefaultPort;

    public static MaxineConfig Load()
    {
        // 実行ファイルと同階層の設定ファイルを読む。存在しない場合はデフォルト値を返す。
        string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string configPath = Path.Combine(assemblyDir, ConfigFileName);

        if (!File.Exists(configPath))
            return new MaxineConfig();

        try
        {
            string json = File.ReadAllText(configPath);
            MaxineConfig? config = JsonSerializer.Deserialize<MaxineConfig>(json);
            return config ?? new MaxineConfig();
        }
        catch
        {
            // 読み込みに失敗した場合は安全側に倒してデフォルト値を返す。
            return new MaxineConfig();
        }
    }
}
