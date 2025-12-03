using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VRCFaceTracking.Maxine;

public class MaxineUdpReceiver : IDisposable
{
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Task _listenerTask;
    private readonly UdpClient _client;
    private readonly Action<float[]> _onFrame;

    public MaxineUdpReceiver(ILogger logger, string host, int port, Action<float[]> onFrame)
    {
        _logger = logger;
        _onFrame = onFrame;

        if (!IPAddress.TryParse(host, out var address))
        {
            address = IPAddress.Any;
            _logger.LogWarning($"受信ホストの指定に失敗したため {address} で待ち受けます。");
        }

        _client = new UdpClient(new IPEndPoint(address, port));
        _listenerTask = Task.Run(ListenAsync);
        _logger.LogInformation($"Maxine用UDP受信を {host}:{port} で開始しました。");
    }

    private async Task ListenAsync()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                var result = await _client.ReceiveAsync(_cancellationTokenSource.Token);
                var values = ParsePayload(result.Buffer);

                if (values != null)
                    _onFrame(values);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "UDP受信中にエラーが発生しました。");
            }
        }
    }

    private float[]? ParsePayload(byte[] buffer)
    {
        try
        {
            string payload;

            if (buffer.Length > 4)
            {
                int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, 0));
                if (length == buffer.Length - 4)
                    payload = Encoding.UTF8.GetString(buffer, 4, length);
                else
                    payload = Encoding.UTF8.GetString(buffer);
            }
            else
            {
                payload = Encoding.UTF8.GetString(buffer);
            }

            var parts = payload.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var values = new float[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                if (!float.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out values[i]))
                {
                    _logger.LogDebug($"数値に変換できない要素を検出しました: {parts[i]}");
                    return null;
                }
            }

            return values;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "UDPメッセージの解析に失敗しました。");
            return null;
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _client.Close();
        _client.Dispose();
        _cancellationTokenSource.Dispose();

        try
        {
            _listenerTask.Wait(TimeSpan.FromSeconds(1));
        }
        catch (AggregateException ex)
        {
            _logger.LogDebug(ex, "リスナータスク終了時に例外を無視しました。");
        }
    }
}
