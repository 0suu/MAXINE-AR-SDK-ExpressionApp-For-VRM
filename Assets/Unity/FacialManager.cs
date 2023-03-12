using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using VRM;

namespace ExpressionAppForVRM
{
    public class Clips
    {
        // フィールド
        public string Name { get; set; } // 名前
        public float Value { get; set; } // 数値

        // コンストラクター
        public Clips(string name, float value)
        {
            this.Name = name;
            this.Value = value;
        }
    }

    public class FacialManager : MonoBehaviour
    {
        [SerializeField]
        MoveManager moveManager;

        [SerializeField]
        VRMBlendShapeProxy blendShapeProxy;

        [SerializeField]
        Transform rightEye = null, leftEye = null;

        List<Clips> clips = new();

        [Range(0f, 10f)] public float eyeBallSens = 6f;
        [Range(0f, 2f)] public float browSens = 1.1f;
        [Range(0f, 1f)] public float browSmooth = 0.1f;
        [Range(0f, 2f)] public float mouthRightLeftSens = 1.1f;
        [Range(0f, 2f)] public float mouthOpenCloseSens = 1.1f;

        float eyeBallMaxAngle = 45f;
        float LastEyeBrow; // スムーズにした眉のトラッキングの値
        float LastEyeBrowR; // スムーズにした眉のトラッキングの値
        float LastEyeBrowL; // スムーズにした眉のトラッキングの値

        #region ブレンドシェイプクリップ
        Clips browInnerUp = new("BrowInnerUp", 0f);
        Clips browDownLeft = new("BrowDownLeft", 0f);
        Clips browDownRight = new("BrowDownRight", 0f);
        Clips browOuterUpLeft = new("BrowOuterUpLeft", 0f);
        Clips browOuterUpRight = new("BrowOuterUpRight", 0f);
        Clips eyeLookUpLeft = new("EyeLookUpLeft", 0f);
        Clips eyeLookUpRight = new("EyeLookUpRight", 0f);
        Clips eyeLookDownLeft = new("EyeLookDownLeft", 0f);
        Clips eyeLookDownRight = new("EyeLookDownRight", 0f);
        Clips eyeLookInLeft = new("EyeLookInLeft", 0f);
        Clips eyeLookInRight = new("EyeLookInRight", 0f);
        Clips eyeLookOutLeft = new("EyeLookOutLeft", 0f);
        Clips eyeLookOutRight = new("EyeLookOutRight", 0f);
        Clips eyeBlinkLeft = new("EyeBlinkLeft", 0f);
        Clips eyeBlinkRight = new("EyeBlinkRight", 0f);
        Clips eyeSquintLeft = new("EyeSquintLeft", 0f);
        Clips eyeSquintRight = new("EyeSquintRight", 0f);
        Clips eyeWideLeft = new("EyeWideLeft", 0f);
        Clips eyeWideRight = new("EyeWideRight", 0f);
        Clips cheekPuff = new("CheekPuff", 0f);
        Clips cheekSquintLeft = new("CheekSquintLeft", 0f);
        Clips cheekSquintRight = new("CheekSquintRight", 0f);
        Clips noseSneerLeft = new("NoseSneerLeft", 0f);
        Clips noseSneerRight = new("NoseSneerRight", 0f);
        Clips jawOpen = new("JawOpen", 0f);
        Clips jawForward = new("JawForward", 0f);
        Clips jawLeft = new("JawLeft", 0f);
        Clips jawRight = new("JawRight", 0f);
        Clips mouthFunnel = new("MouthFunnel", 0f);
        Clips mouthPucker = new("MouthPucker", 0f);
        Clips mouthLeft = new("MouthLeft", 0f);
        Clips mouthRight = new("MouthRight", 0f);
        Clips mouthRollUpper = new("MouthRollUpper", 0f);
        Clips mouthRollLower = new("MouthRollLower", 0f);
        Clips mouthShrugUpper = new("MouthShrugUpper", 0f);
        Clips mouthShrugLower = new("MouthShrugLower", 0f);
        Clips mouthClose = new("MouthClose", 0f);
        Clips mouthSmileLeft = new("MouthSmileLeft", 0f);
        Clips mouthSmileRight = new("MouthSmileRight", 0f);
        Clips mouthFrownLeft = new("MouthFrownLeft", 0f);
        Clips mouthFrownRight = new("MouthFrownRight", 0f);
        Clips mouthDimpleLeft = new("MouthDimpleLeft", 0f);
        Clips mouthDimpleRight = new("MouthDimpleRight", 0f);
        Clips mouthUpperUpLeft = new("MouthUpperUpLeft", 0f);
        Clips mouthUpperUpRight = new("MouthUpperUpRight", 0f);
        Clips mouthLowerDownLeft = new("MouthLowerDownLeft", 0f);
        Clips mouthLowerDownRight = new("MouthLowerDownRight", 0f);
        Clips mouthPressLeft = new("MouthPressLeft", 0f);
        Clips mouthPressRight = new("MouthPressRight", 0f);
        Clips mouthStretchLeft = new("MouthStretchLeft", 0f);
        Clips mouthStretchRight = new("MouthStretchRight", 0f);
        Clips tongueOut = new("tongueOut", 0f); //取得できない
        #endregion

        void Start()
        {
            clips.Add(browInnerUp);
            clips.Add(browDownLeft);
            clips.Add(browDownRight);
            clips.Add(browOuterUpLeft);
            clips.Add(browOuterUpRight);
            clips.Add(eyeLookUpLeft);
            clips.Add(eyeLookUpRight);
            clips.Add(eyeLookDownLeft);
            clips.Add(eyeLookDownRight);
            clips.Add(eyeLookInLeft);
            clips.Add(eyeLookInRight);
            clips.Add(eyeLookOutLeft);
            clips.Add(eyeLookOutRight);
            clips.Add(eyeBlinkLeft);
            clips.Add(eyeBlinkRight);
            clips.Add(eyeSquintLeft);
            clips.Add(eyeSquintRight);
            clips.Add(eyeWideLeft);
            clips.Add(eyeWideRight);
            clips.Add(cheekPuff);
            clips.Add(cheekSquintLeft);
            clips.Add(cheekSquintRight);
            clips.Add(noseSneerLeft);
            clips.Add(noseSneerRight);
            clips.Add(jawOpen);
            clips.Add(jawForward);
            clips.Add(jawLeft);
            clips.Add(jawRight);
            clips.Add(mouthFunnel);
            clips.Add(mouthPucker);
            clips.Add(mouthLeft);
            clips.Add(mouthRight);
            clips.Add(mouthRollUpper);
            clips.Add(mouthRollLower);
            clips.Add(mouthShrugUpper);
            clips.Add(mouthShrugLower);
            clips.Add(mouthClose);
            clips.Add(mouthSmileLeft);
            clips.Add(mouthSmileRight);
            clips.Add(mouthFrownLeft);
            clips.Add(mouthFrownRight);
            clips.Add(mouthDimpleLeft);
            clips.Add(mouthDimpleRight);
            clips.Add(mouthUpperUpLeft);
            clips.Add(mouthUpperUpRight);
            clips.Add(mouthLowerDownLeft);
            clips.Add(mouthLowerDownRight);
            clips.Add(mouthPressLeft);
            clips.Add(mouthPressRight);
            clips.Add(mouthStretchLeft);
            clips.Add(mouthStretchRight);
        }




        void Update()
        {
            var ReceivedValue = moveManager.ReceivedValue;
            browInnerUp.Value = ReceivedValue["browInnerUp_L"]; //Rもある
            browDownLeft.Value = ReceivedValue["browDown_L"];
            browDownRight.Value = ReceivedValue["browDown_R"];
            browOuterUpLeft.Value = ReceivedValue["browOuterUp_L"];
            browOuterUpRight.Value = ReceivedValue["browOuterUp_R"];
            eyeLookUpLeft.Value = ReceivedValue["eyeLookUp_L"];
            eyeLookUpRight.Value = ReceivedValue["eyeLookUp_R"];
            eyeLookDownLeft.Value = ReceivedValue["eyeLookDown_L"];
            eyeLookDownRight.Value = ReceivedValue["eyeLookDown_R"];
            eyeLookInLeft.Value = ReceivedValue["eyeLookIn_L"];
            eyeLookInRight.Value = ReceivedValue["eyeLookIn_R"];
            eyeLookOutLeft.Value = ReceivedValue["eyeLookOut_L"];
            eyeLookOutRight.Value = ReceivedValue["eyeLookOut_R"];
            eyeBlinkLeft.Value = ReceivedValue["eyeBlink_L"] * 1.1f;
            eyeBlinkRight.Value = ReceivedValue["eyeBlink_R"] * 1.1f;
            eyeSquintLeft.Value = ReceivedValue["eyeSquint_L"];
            eyeSquintRight.Value = ReceivedValue["eyeSquint_R"];
            eyeWideLeft.Value = ReceivedValue["eyeWide_L"];
            eyeWideRight.Value = ReceivedValue["eyeWide_R"];
            cheekPuff.Value = ReceivedValue["cheekPuff_L"]; //Rもある
            cheekSquintLeft.Value = ReceivedValue["cheekSquint_L"];
            cheekSquintRight.Value = ReceivedValue["cheekSquint_R"];
            noseSneerLeft.Value = ReceivedValue["noseSneer_L"];
            noseSneerRight.Value = ReceivedValue["noseSneer_R"];
            jawOpen.Value = ReceivedValue["jawOpen"];
            jawForward.Value = ReceivedValue["jawForward"];
            jawLeft.Value = ReceivedValue["jawLeft"];
            jawRight.Value = ReceivedValue["jawRight"];
            mouthFunnel.Value = ReceivedValue["mouthFunnel"];
            mouthPucker.Value = ReceivedValue["mouthPucker"];
            mouthLeft.Value = ReceivedValue["mouthLeft"];
            mouthRight.Value = ReceivedValue["mouthRight"];
            mouthRollUpper.Value = ReceivedValue["mouthRollUpper"];
            mouthRollLower.Value = ReceivedValue["mouthRollLower"];
            mouthShrugUpper.Value = ReceivedValue["mouthShrugUppe"];
            mouthShrugLower.Value = ReceivedValue["mouthShrugLowe"];
            mouthClose.Value = ReceivedValue["mouthClose"];
            mouthSmileLeft.Value = ReceivedValue["mouthSmile_L"];
            mouthSmileRight.Value = ReceivedValue["mouthSmile_R"];
            mouthFrownLeft.Value = ReceivedValue["mouthFrown_L"];
            mouthFrownRight.Value = ReceivedValue["mouthFrown_R"];
            mouthDimpleLeft.Value = ReceivedValue["mouthDimple_L"];
            mouthDimpleRight.Value = ReceivedValue["mouthDimple_R"];
            mouthUpperUpLeft.Value = ReceivedValue["mouthUpperUp_L"];
            mouthUpperUpRight.Value = ReceivedValue["mouthUpperUp_R"];
            mouthLowerDownLeft.Value = ReceivedValue["mouthLowerDown_L"];
            mouthLowerDownRight.Value = ReceivedValue["mouthLowerDown_R"];
            mouthPressLeft.Value = ReceivedValue["mouthPress_L"];
            mouthPressRight.Value = ReceivedValue["mouthPress_R"];
            mouthStretchLeft.Value = ReceivedValue["mouthStretch_L"];
            mouthStretchRight.Value = ReceivedValue["mouthStretch_R"];

            //目
            {
                var valueBrow = Mathf.LerpUnclamped(LastEyeBrow, browInnerUp.Value * browSens, browSmooth);
                var valueBrowR = Mathf.LerpUnclamped(LastEyeBrowR, browDownLeft.Value * browSens, browSmooth);
                var valueBrowL = Mathf.LerpUnclamped(LastEyeBrowL, browDownRight.Value * browSens, browSmooth);

                browInnerUp.Value = valueBrow;
                browDownRight.Value = valueBrowR;
                browDownLeft.Value = valueBrowL;

                LastEyeBrow = valueBrow;
                LastEyeBrowR = valueBrowR;
                LastEyeBrowL = valueBrowL;
            }

            //口
            {
                mouthLeft.Value *= mouthRightLeftSens;
                mouthRight.Value *= mouthRightLeftSens;

                jawOpen.Value *= mouthOpenCloseSens;
            }

            //眼球の回転
            {
                // 左目の回転角度を計算する
                float leftEyeX = eyeLookDownLeft.Value - eyeLookUpLeft.Value;
                float leftEyeY = eyeLookOutLeft.Value - eyeLookInLeft.Value;
                float leftEyeZ = eyeLookInLeft.Value + eyeLookOutLeft.Value;

                // 右目の回転角度を計算する
                float rightEyeX = eyeLookDownRight.Value - eyeLookUpRight.Value;
                float rightEyeY = eyeLookInRight.Value - eyeLookOutRight.Value;
                float rightEyeZ = eyeLookInRight.Value + eyeLookOutRight.Value;

                // 目の回転角度を最大値に制限する
                leftEyeX = Mathf.Clamp(leftEyeX, -eyeBallMaxAngle, eyeBallMaxAngle);
                leftEyeY = Mathf.Clamp(leftEyeY, -eyeBallMaxAngle, eyeBallMaxAngle);
                leftEyeZ = Mathf.Clamp(leftEyeZ, -eyeBallMaxAngle, eyeBallMaxAngle);
                rightEyeX = Mathf.Clamp(rightEyeX, -eyeBallMaxAngle, eyeBallMaxAngle);
                rightEyeY = Mathf.Clamp(rightEyeY, -eyeBallMaxAngle, eyeBallMaxAngle);
                rightEyeZ = Mathf.Clamp(rightEyeZ, -eyeBallMaxAngle, eyeBallMaxAngle);

                // 目のオブジェクトを回転させる
                leftEye.localRotation = Quaternion.Euler(leftEyeX * eyeBallSens, leftEyeY * eyeBallSens, leftEyeZ * 0);
                rightEye.localRotation = Quaternion.Euler(rightEyeX * eyeBallSens, rightEyeY * eyeBallSens, rightEyeZ * 0);
            }

            //値の適用
            foreach (var i in clips)
            {
                if (i.Value > 1) i.Value = 1;

                blendShapeProxy.ImmediatelySetValue(i.Name, i.Value);
            }
        }
    }
}
