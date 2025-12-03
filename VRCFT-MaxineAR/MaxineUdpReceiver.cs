using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VRCFaceTracking.MaxineAR;

public class MaxineUdpReceiver : IDisposable
{
    public const string DefaultHost = "127.0.0.1";
    public const int DefaultPort = 9000;

    private readonly ILogger _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly UdpClient _udpClient;
    private readonly Task _listenTask;
    private readonly object _latestLock = new();
    private float[]? _latestPayload;

    public MaxineUdpReceiver(ILogger logger, string? host, int? port)
    {
        _logger = logger;

        IPAddress address = IPAddress.Parse(string.IsNullOrWhiteSpace(host) ? DefaultHost : host);
        int resolvedPort = port ?? DefaultPort;

        _udpClient = new UdpClient(new IPEndPoint(address, resolvedPort));
        _udpClient.Client.ReceiveTimeout = 2000;

        _logger.LogInformation("MaxineARのUDP受信を {Host}:{Port} で開始しました。", address, resolvedPort);

        _listenTask = Task.Run(ListenAsync, _cancellationTokenSource.Token);
    }

    public bool TryGetLatest(out float[] payload)
    {
        lock (_latestLock)
        {
            if (_latestPayload is null)
            {
                payload = Array.Empty<float>();
                return false;
            }

            payload = new float[_latestPayload.Length];
            Array.Copy(_latestPayload, payload, _latestPayload.Length);
            return true;
        }
    }

    private async Task ListenAsync()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                UdpReceiveResult result = await _udpClient.ReceiveAsync(_cancellationTokenSource.Token);
                float[]? parsed = Parse(result.Buffer);
                if (parsed is null)
                    continue;

                lock (_latestLock)
                {
                    _latestPayload = parsed;
                }
            }
            catch (OperationCanceledException)
            {
                // Dispose時にキャンセルされるので無視。
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "MaxineARのUDP受信で例外が発生しました。");
            }
        }
    }

    private static float[]? Parse(byte[] buffer)
    {
        // C++側で先頭4byteに長さが入るのでそれを外して文字列化する。
        if (buffer.Length <= sizeof(int))
            return null;

        int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, 0));
        if (length <= 0)
            return null;

        int payloadLength = Math.Min(length, buffer.Length - sizeof(int));
        string payload = Encoding.UTF8.GetString(buffer, sizeof(int), payloadLength);

        string[] parts = payload.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        float[] values = new float[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!float.TryParse(parts[i], out values[i]))
            {
                values[i] = 0f;
            }
        }

        return values;
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        try
        {
            _listenTask.Wait();
        }
        catch
        {
            // 破棄時の例外は握りつぶす。
        }

        _udpClient.Dispose();
        _cancellationTokenSource.Dispose();
    }
}
