using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking.MaxineAR;

public class MaxineVRC : ExtTrackingModule
{
    private MaxineUdpReceiver? _receiver;

    public override (bool SupportsEye, bool SupportsExpression) Supported => (true, true);

    public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable)
    {
        // UDP受信設定を読み込み、MaxineARからのストリームを待ち受ける。
        MaxineConfig config = MaxineConfig.Load();
        _receiver = new MaxineUdpReceiver(Logger, config.Host, config.Port);

        ModuleInformation = new ModuleMetadata
        {
            Name = "Maxine AR SDK Module"
        };

        return (true, true);
    }

    public override void Teardown()
    {
        _receiver?.Dispose();
    }

    public override void Update()
    {
        if (_receiver is null)
            return;

        if (_receiver.TryGetLatest(out float[] payload))
        {
            MaxineExpressions.Apply(payload, UnifiedTracking.Data);
        }
    }
}
