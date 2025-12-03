using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Expressions;
using TrackingVector2 = VRCFaceTracking.Core.Types.Vector2;

namespace VRCFaceTracking.Maxine;

public class MaxineModule : ExtTrackingModule
{
    private static readonly string[] ValueOrder =
    {
        "browDown_L", "browDown_R", "browInnerUp_L", "browInnerUp_R", "browOuterUp_L", "browOuterUp_R",
        "cheekPuff_L", "cheekPuff_R", "cheekSquint_L", "cheekSquint_R",
        "eyeBlink_L", "eyeBlink_R", "eyeLookDown_L", "eyeLookDown_R", "eyeLookIn_L", "eyeLookIn_R",
        "eyeLookOut_L", "eyeLookOut_R", "eyeLookUp_L", "eyeLookUp_R", "eyeSquint_L", "eyeSquint_R",
        "eyeWide_L", "eyeWide_R", "jawForward", "jawLeft", "jawOpen", "jawRight", "mouthClose",
        "mouthDimple_L", "mouthDimple_R", "mouthFrown_L", "mouthFrown_R", "mouthFunnel", "mouthLeft",
        "mouthLowerDown_L", "mouthLowerDown_R", "mouthPress_L", "mouthPress_R", "mouthPucker", "mouthRight",
        "mouthRollLower", "mouthRollUpper", "mouthShrugLowe", "mouthShrugUppe", "mouthSmile_L", "mouthSmile_R",
        "mouthStretch_L", "mouthStretch_R", "mouthUpperUp_L", "mouthUpperUp_R", "noseSneer_L", "noseSneer_R",
        "Rotation_X", "Rotation_Y", "Rotation_Z", "Rotation_W", "TransrationX", "TransrationY", "TransrationZ"
    };
    private static readonly IReadOnlyDictionary<string, int> ValueIndexLookup = ValueOrder
        .Select((name, index) => new { name, index })
        .ToDictionary(x => x.name, x => x.index);

    private readonly object _lock = new();
    private float[]? _latestValues;
    private MaxineUdpReceiver? _receiver;

    public override (bool SupportsEye, bool SupportsExpression) Supported => (true, true);

    public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable)
    {
        var config = MaxineConfig.Load(Logger);
        _receiver = new MaxineUdpReceiver(Logger, config.Host, config.Port, OnValuesReceived);

        ModuleInformation = new ModuleMetadata
        {
            Name = "Maxine AR SDK Webcam"
        };

        return (true, true);
    }

    public override void Teardown()
    {
        _receiver?.Dispose();
    }

    public override void Update()
    {
        float[]? snapshot = null;
        lock (_lock)
        {
            if (_latestValues != null)
            {
                snapshot = _latestValues;
                _latestValues = null;
            }
        }

        if (snapshot == null)
            return;

        if (snapshot.Length < ValueOrder.Length)
        {
            Logger.LogWarning($"受信データ数が不足しています。期待値: {ValueOrder.Length} 実際: {snapshot.Length}");
            return;
        }

        ApplyExpressions(snapshot);
        ApplyEyes(snapshot);
        ApplyHead(snapshot);
    }

    private void OnValuesReceived(float[] values)
    {
        lock (_lock)
        {
            _latestValues = values;
        }
    }

    private void ApplyExpressions(IReadOnlyList<float> values)
    {
        SetShape(UnifiedExpressions.BrowLowererLeft, values[GetIndex("browDown_L")]);
        SetShape(UnifiedExpressions.BrowLowererRight, values[GetIndex("browDown_R")]);
        SetShape(UnifiedExpressions.BrowInnerUpLeft, values[GetIndex("browInnerUp_L")]);
        SetShape(UnifiedExpressions.BrowInnerUpRight, values[GetIndex("browInnerUp_R")]);
        SetShape(UnifiedExpressions.BrowOuterUpLeft, values[GetIndex("browOuterUp_L")]);
        SetShape(UnifiedExpressions.BrowOuterUpRight, values[GetIndex("browOuterUp_R")]);

        SetShape(UnifiedExpressions.CheekPuffLeft, values[GetIndex("cheekPuff_L")]);
        SetShape(UnifiedExpressions.CheekPuffRight, values[GetIndex("cheekPuff_R")]);
        SetShape(UnifiedExpressions.CheekSquintLeft, values[GetIndex("cheekSquint_L")]);
        SetShape(UnifiedExpressions.CheekSquintRight, values[GetIndex("cheekSquint_R")]);

        SetShape(UnifiedExpressions.EyeSquintLeft, values[GetIndex("eyeSquint_L")]);
        SetShape(UnifiedExpressions.EyeSquintRight, values[GetIndex("eyeSquint_R")]);
        SetShape(UnifiedExpressions.EyeWideLeft, values[GetIndex("eyeWide_L")]);
        SetShape(UnifiedExpressions.EyeWideRight, values[GetIndex("eyeWide_R")]);

        SetShape(UnifiedExpressions.JawForward, values[GetIndex("jawForward")]);
        SetShape(UnifiedExpressions.JawLeft, values[GetIndex("jawLeft")]);
        SetShape(UnifiedExpressions.JawOpen, values[GetIndex("jawOpen")]);
        SetShape(UnifiedExpressions.JawRight, values[GetIndex("jawRight")]);

        var mouthClose = values[GetIndex("mouthClose")];
        SetShape(UnifiedExpressions.MouthClosed, mouthClose);

        SetShape(UnifiedExpressions.MouthDimpleLeft, values[GetIndex("mouthDimple_L")]);
        SetShape(UnifiedExpressions.MouthDimpleRight, values[GetIndex("mouthDimple_R")]);
        SetShape(UnifiedExpressions.MouthFrownLeft, values[GetIndex("mouthFrown_L")]);
        SetShape(UnifiedExpressions.MouthFrownRight, values[GetIndex("mouthFrown_R")]);

        var funnel = values[GetIndex("mouthFunnel")];
        SetShape(UnifiedExpressions.LipFunnelLowerLeft, funnel);
        SetShape(UnifiedExpressions.LipFunnelLowerRight, funnel);
        SetShape(UnifiedExpressions.LipFunnelUpperLeft, funnel);
        SetShape(UnifiedExpressions.LipFunnelUpperRight, funnel);

        var mouthLeft = values[GetIndex("mouthLeft")];
        var mouthRight = values[GetIndex("mouthRight")];
        SetShape(UnifiedExpressions.MouthUpperLeft, mouthLeft);
        SetShape(UnifiedExpressions.MouthLowerLeft, mouthLeft);
        SetShape(UnifiedExpressions.MouthUpperRight, mouthRight);
        SetShape(UnifiedExpressions.MouthLowerRight, mouthRight);

        SetShape(UnifiedExpressions.MouthLowerDownLeft, values[GetIndex("mouthLowerDown_L")]);
        SetShape(UnifiedExpressions.MouthLowerDownRight, values[GetIndex("mouthLowerDown_R")]);
        SetShape(UnifiedExpressions.MouthPressLeft, values[GetIndex("mouthPress_L")]);
        SetShape(UnifiedExpressions.MouthPressRight, values[GetIndex("mouthPress_R")]);

        var pucker = values[GetIndex("mouthPucker")];
        SetShape(UnifiedExpressions.LipPuckerLowerLeft, pucker);
        SetShape(UnifiedExpressions.LipPuckerLowerRight, pucker);
        SetShape(UnifiedExpressions.LipPuckerUpperLeft, pucker);
        SetShape(UnifiedExpressions.LipPuckerUpperRight, pucker);

        SetShape(UnifiedExpressions.MouthRollLower, values[GetIndex("mouthRollLower")]);
        SetShape(UnifiedExpressions.MouthRollUpper, values[GetIndex("mouthRollUpper")]);
        SetShape(UnifiedExpressions.MouthRaiserLower, values[GetIndex("mouthShrugLowe")]);
        SetShape(UnifiedExpressions.MouthRaiserUpper, values[GetIndex("mouthShrugUppe")]);
        SetShape(UnifiedExpressions.MouthCornerPullLeft, values[GetIndex("mouthSmile_L")]);
        SetShape(UnifiedExpressions.MouthCornerPullRight, values[GetIndex("mouthSmile_R")]);

        SetShape(UnifiedExpressions.MouthStretchLeft, values[GetIndex("mouthStretch_L")]);
        SetShape(UnifiedExpressions.MouthStretchRight, values[GetIndex("mouthStretch_R")]);
        SetShape(UnifiedExpressions.MouthUpperUpLeft, values[GetIndex("mouthUpperUp_L")]);
        SetShape(UnifiedExpressions.MouthUpperUpRight, values[GetIndex("mouthUpperUp_R")]);
        SetShape(UnifiedExpressions.NoseSneerLeft, values[GetIndex("noseSneer_L")]);
        SetShape(UnifiedExpressions.NoseSneerRight, values[GetIndex("noseSneer_R")]);
    }

    private void ApplyEyes(IReadOnlyList<float> values)
    {
        var eyeData = UnifiedTracking.Data.Eye;

        eyeData.Left.Openness = Clamp01(1f - values[GetIndex("eyeBlink_L")]);
        eyeData.Right.Openness = Clamp01(1f - values[GetIndex("eyeBlink_R")]);

        eyeData.Left.Gaze = new TrackingVector2(
            ClampRange(values[GetIndex("eyeLookOut_L")] - values[GetIndex("eyeLookIn_L")]),
            ClampRange(values[GetIndex("eyeLookDown_L")] - values[GetIndex("eyeLookUp_L")])
        );
        eyeData.Right.Gaze = new TrackingVector2(
            ClampRange(values[GetIndex("eyeLookOut_R")] - values[GetIndex("eyeLookIn_R")]),
            ClampRange(values[GetIndex("eyeLookDown_R")] - values[GetIndex("eyeLookUp_R")])
        );
    }

    private void ApplyHead(IReadOnlyList<float> values)
    {
        var rotation = new Quaternion(
            values[GetIndex("Rotation_X")],
            values[GetIndex("Rotation_Y")],
            values[GetIndex("Rotation_Z")],
            values[GetIndex("Rotation_W")]
        );

        var (pitch, yaw, roll) = ToEulerAngles(rotation);

        UnifiedTracking.Data.Head.HeadPitch = NormalizeAngleRad(pitch);
        UnifiedTracking.Data.Head.HeadYaw = NormalizeAngleRad(yaw);
        UnifiedTracking.Data.Head.HeadRoll = NormalizeAngleRad(roll);

        UnifiedTracking.Data.Head.HeadPosX = ClampRange(values[GetIndex("TransrationX")]);
        UnifiedTracking.Data.Head.HeadPosY = ClampRange(values[GetIndex("TransrationY")]);
        UnifiedTracking.Data.Head.HeadPosZ = ClampRange(values[GetIndex("TransrationZ")]);
    }

    private void SetShape(UnifiedExpressions expression, float value)
    {
        UnifiedTracking.Data.Shapes[(int)expression].Weight = Clamp01(value);
    }

    private static float Clamp01(float value) => Math.Clamp(value, 0f, 1f);

    private static float ClampRange(float value) => Math.Clamp(value, -1f, 1f);

    private static float NormalizeAngleRad(float value) => ClampRange(value * (180f / (float)Math.PI) / 90f);

    private static int GetIndex(string key)
    {
        if (ValueIndexLookup.TryGetValue(key, out var index))
            return index;

        throw new ArgumentOutOfRangeException(nameof(key), $"{key} は既知の入力名ではありません。");
    }

    private static (float pitch, float yaw, float roll) ToEulerAngles(Quaternion q)
    {
        var sinrCosp = 2 * (q.W * q.X + q.Y * q.Z);
        var cosrCosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
        var roll = (float)Math.Atan2(sinrCosp, cosrCosp);

        var sinp = 2 * (q.W * q.Y - q.Z * q.X);
        float pitch;
        if (Math.Abs(sinp) >= 1)
            pitch = (float)Math.CopySign(Math.PI / 2, sinp);
        else
            pitch = (float)Math.Asin(sinp);

        var sinyCosp = 2 * (q.W * q.Z + q.X * q.Y);
        var cosyCosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        var yaw = (float)Math.Atan2(sinyCosp, cosyCosp);

        return (pitch, yaw, roll);
    }
}
