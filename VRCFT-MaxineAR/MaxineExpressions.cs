using System;
using System.Numerics;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.Core.Types;

namespace VRCFaceTracking.MaxineAR;

public static class MaxineExpressions
{
    private const int ExpressionCount = 53;
    private const int PoseCount = 7;
    private const float Rad2Deg = 180f / MathF.PI;

    public static void Apply(float[] values, UnifiedTrackingData data)
    {
        if (values.Length < ExpressionCount)
            return;

        // 眉
        Set(data, UnifiedExpressions.BrowLowererLeft, values[0]);
        Set(data, UnifiedExpressions.BrowLowererRight, values[1]);
        Set(data, UnifiedExpressions.BrowInnerUpLeft, values[2]);
        Set(data, UnifiedExpressions.BrowInnerUpRight, values[3]);
        Set(data, UnifiedExpressions.BrowOuterUpLeft, values[4]);
        Set(data, UnifiedExpressions.BrowOuterUpRight, values[5]);

        // 頬
        Set(data, UnifiedExpressions.CheekPuffLeft, values[6]);
        Set(data, UnifiedExpressions.CheekPuffRight, values[7]);
        Set(data, UnifiedExpressions.CheekSquintLeft, values[8]);
        Set(data, UnifiedExpressions.CheekSquintRight, values[9]);

        // 顎・口周り
        Set(data, UnifiedExpressions.JawForward, values[24]);
        Set(data, UnifiedExpressions.JawLeft, values[25]);
        Set(data, UnifiedExpressions.JawOpen, values[26]);
        Set(data, UnifiedExpressions.JawRight, values[27]);
        Set(data, UnifiedExpressions.MouthClosed, values[28]);
        Set(data, UnifiedExpressions.MouthDimpleLeft, values[29]);
        Set(data, UnifiedExpressions.MouthDimpleRight, values[30]);
        Set(data, UnifiedExpressions.MouthFrownLeft, values[31]);
        Set(data, UnifiedExpressions.MouthFrownRight, values[32]);
        SetFunnel(data, values[33]);
        SetMouthHorizontal(data, values[34], values[40]);
        Set(data, UnifiedExpressions.MouthLowerDownLeft, values[35]);
        Set(data, UnifiedExpressions.MouthLowerDownRight, values[36]);
        Set(data, UnifiedExpressions.MouthPressLeft, values[37]);
        Set(data, UnifiedExpressions.MouthPressRight, values[38]);
        SetPucker(data, values[39]);
        SetRoll(data, values[41], values[42]);
        Set(data, UnifiedExpressions.MouthRaiserLower, values[43]);
        Set(data, UnifiedExpressions.MouthRaiserUpper, values[44]);
        Set(data, UnifiedExpressions.MouthCornerPullLeft, values[45]);
        Set(data, UnifiedExpressions.MouthCornerPullRight, values[46]);
        Set(data, UnifiedExpressions.MouthStretchLeft, values[47]);
        Set(data, UnifiedExpressions.MouthStretchRight, values[48]);
        Set(data, UnifiedExpressions.MouthUpperUpLeft, values[49]);
        Set(data, UnifiedExpressions.MouthUpperUpRight, values[50]);

        // 鼻
        Set(data, UnifiedExpressions.NasalDilationLeft, values[51]);
        Set(data, UnifiedExpressions.NasalDilationRight, values[52]);

        ApplyEyes(values, data);
        ApplyHead(values, data);
    }

    private static void ApplyEyes(float[] values, UnifiedTrackingData data)
    {
        float blinkLeft = Get(values, 10);
        float blinkRight = Get(values, 11);
        float lookDownLeft = Get(values, 12);
        float lookDownRight = Get(values, 13);
        float lookInLeft = Get(values, 14);
        float lookInRight = Get(values, 15);
        float lookOutLeft = Get(values, 16);
        float lookOutRight = Get(values, 17);
        float lookUpLeft = Get(values, 18);
        float lookUpRight = Get(values, 19);
        float squintLeft = Get(values, 20);
        float squintRight = Get(values, 21);
        float wideLeft = Get(values, 22);
        float wideRight = Get(values, 23);

        float closureLeft = MathF.Clamp(blinkLeft + squintLeft, 0f, 1f);
        float closureRight = MathF.Clamp(blinkRight + squintRight, 0f, 1f);

        Set(data, UnifiedExpressions.EyeSquintLeft, closureLeft);
        Set(data, UnifiedExpressions.EyeSquintRight, closureRight);
        Set(data, UnifiedExpressions.EyeWideLeft, wideLeft);
        Set(data, UnifiedExpressions.EyeWideRight, wideRight);

        data.Eye.Left.Openness = MathF.Clamp(1f - closureLeft + wideLeft * 0.35f, 0f, 1f);
        data.Eye.Right.Openness = MathF.Clamp(1f - closureRight + wideRight * 0.35f, 0f, 1f);

        float gazeXLeft = MathF.Clamp(lookInLeft - lookOutLeft, -1f, 1f);
        float gazeXRight = MathF.Clamp(lookOutRight - lookInRight, -1f, 1f);
        float gazeYLeft = MathF.Clamp(lookUpLeft - lookDownLeft, -1f, 1f);
        float gazeYRight = MathF.Clamp(lookUpRight - lookDownRight, -1f, 1f);

        data.Eye.Left.Gaze = new Vector2(gazeXLeft, gazeYLeft);
        data.Eye.Right.Gaze = new Vector2(gazeXRight, gazeYRight);
    }

    private static void ApplyHead(float[] values, UnifiedTrackingData data)
    {
        if (values.Length < ExpressionCount + PoseCount)
            return;

        int offset = ExpressionCount;
        var rotation = new Quaternion(values[offset], values[offset + 1], values[offset + 2], values[offset + 3]);
        Vector3 euler = QuaternionToEuler(rotation);

        data.Head.HeadPitch = ClampUnit(euler.X / 90f);
        data.Head.HeadYaw = ClampUnit(euler.Y / 90f);
        data.Head.HeadRoll = ClampUnit(euler.Z / 90f);

        data.Head.HeadPosX = ClampUnit(values[offset + 4]);
        data.Head.HeadPosY = ClampUnit(values[offset + 5]);
        data.Head.HeadPosZ = ClampUnit(values[offset + 6]);
    }

    private static void Set(UnifiedTrackingData data, UnifiedExpressions expression, float weight)
    {
        data.Shapes[(int)expression].Weight = MathF.Max(0f, weight);
    }

    private static void SetFunnel(UnifiedTrackingData data, float weight)
    {
        Set(data, UnifiedExpressions.LipFunnelUpperLeft, weight);
        Set(data, UnifiedExpressions.LipFunnelUpperRight, weight);
        Set(data, UnifiedExpressions.LipFunnelLowerLeft, weight);
        Set(data, UnifiedExpressions.LipFunnelLowerRight, weight);
    }

    private static void SetPucker(UnifiedTrackingData data, float weight)
    {
        Set(data, UnifiedExpressions.LipPuckerUpperLeft, weight);
        Set(data, UnifiedExpressions.LipPuckerUpperRight, weight);
        Set(data, UnifiedExpressions.LipPuckerLowerLeft, weight);
        Set(data, UnifiedExpressions.LipPuckerLowerRight, weight);
    }

    private static void SetRoll(UnifiedTrackingData data, float lowerWeight, float upperWeight)
    {
        Set(data, UnifiedExpressions.LipSuckLowerLeft, lowerWeight);
        Set(data, UnifiedExpressions.LipSuckLowerRight, lowerWeight);
        Set(data, UnifiedExpressions.LipSuckUpperLeft, upperWeight);
        Set(data, UnifiedExpressions.LipSuckUpperRight, upperWeight);
    }

    private static void SetMouthHorizontal(UnifiedTrackingData data, float leftWeight, float rightWeight)
    {
        Set(data, UnifiedExpressions.MouthUpperLeft, leftWeight);
        Set(data, UnifiedExpressions.MouthLowerLeft, leftWeight);
        Set(data, UnifiedExpressions.MouthUpperRight, rightWeight);
        Set(data, UnifiedExpressions.MouthLowerRight, rightWeight);
    }

    private static float Get(float[] values, int index)
    {
        return index < values.Length ? values[index] : 0f;
    }

    private static Vector3 QuaternionToEuler(Quaternion q)
    {
        float sinrCosp = 2f * (q.W * q.X + q.Y * q.Z);
        float cosrCosp = 1f - 2f * (q.X * q.X + q.Y * q.Y);
        float roll = MathF.Atan2(sinrCosp, cosrCosp) * Rad2Deg;

        float sinp = 2f * (q.W * q.Y - q.Z * q.X);
        float pitch = MathF.Abs(sinp) >= 1f ? MathF.CopySign(90f, sinp) : MathF.Asin(sinp) * Rad2Deg;

        float sinyCosp = 2f * (q.W * q.Z + q.X * q.Y);
        float cosyCosp = 1f - 2f * (q.Y * q.Y + q.Z * q.Z);
        float yaw = MathF.Atan2(sinyCosp, cosyCosp) * Rad2Deg;

        return new Vector3(pitch, yaw, roll);
    }

    private static float ClampUnit(float value)
    {
        return MathF.Clamp(value, -1f, 1f);
    }
}
