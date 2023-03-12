using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace ExpressionAppForVRM
{
    public class MoveManager : MonoBehaviour
    {
        [SerializeField]
        Transform headIKTarget = null;

        public Dictionary<string, float> ReceivedValue = new()
        {
            { "browDown_L", 0f },
            { "browDown_R", 0f },
            { "browInnerUp_L", 0f },
            { "browInnerUp_R", 0f },
            { "browOuterUp_L", 0f },
            { "browOuterUp_R", 0f },
            { "cheekPuff_L", 0f },
            { "cheekPuff_R", 0f },
            { "cheekSquint_L", 0f },
            { "cheekSquint_R", 0f },
            { "eyeBlink_L", 0f },
            { "eyeBlink_R", 0f },
            { "eyeLookDown_L", 0f },
            { "eyeLookDown_R", 0f },
            { "eyeLookIn_L", 0f },
            { "eyeLookIn_R", 0f },
            { "eyeLookOut_L", 0f },
            { "eyeLookOut_R", 0f },
            { "eyeLookUp_L", 0f },
            { "eyeLookUp_R", 0f },
            { "eyeSquint_L", 0f },
            { "eyeSquint_R", 0f },
            { "eyeWide_L", 0f },
            { "eyeWide_R", 0f },
            { "jawForward",0f},
            { "jawLeft", 0f},
            { "jawOpen", 0f},
            { "jawRight", 0f},
            { "mouthClose", 0f},
            { "mouthDimple_L", 0f },
            { "mouthDimple_R", 0f },
            { "mouthFrown_L", 0f },
            { "mouthFrown_R", 0f },
            { "mouthFunnel", 0f},
            { "mouthLeft", 0f},
            { "mouthLowerDown_L", 0f},
            { "mouthLowerDown_R", 0f},
            { "mouthPress_L", 0f },
            { "mouthPress_R", 0f },
            { "mouthPucker", 0f},
            { "mouthRight", 0f},
            { "mouthRollLower", 0f},
            { "mouthRollUpper", 0f},
            { "mouthShrugLowe", 0f},
            { "mouthShrugUppe", 0f},
            { "mouthSmile_L", 0f },
            { "mouthSmile_R", 0f },
            { "mouthStretch_L", 0f },
            { "mouthStretch_R", 0f },
            { "mouthUpperUp_L", 0f },
            { "mouthUpperUp_R", 0f },
            { "noseSneer_L", 0f },
            { "noseSneer_R", 0f },
            { "Rotation_X", 0f },
            { "Rotation_Y", 0f },
            { "Rotation_Z", 0f },
            { "Rotation_W", 0f },
            { "TransrationX", 0f },
            { "TransrationY", 0f },
            { "TransrationZ", 0f },
        };
                
        void Update()
        {
            //ì™ÇÃâÒì]
            headIKTarget.localRotation = new Quaternion(ReceivedValue["Rotation_X"], ReceivedValue["Rotation_Y"], ReceivedValue["Rotation_Z"], ReceivedValue["Rotation_W"]);
        }

        /// <summary>
        /// èoóÕÇDictionaryÇ…ì¸ÇÍÇÈ
        /// </summary>
        /// <param name="value"></param>
        public void UpdateRecivedValue(string value)
        {
            var stringArr = value.Split(',');

            var floatArr = Array.ConvertAll(stringArr, float.Parse);

            for (int i = 0; i < ReceivedValue.Keys.Count; i++)
            {
                var key = ReceivedValue.Keys.ElementAt(i);
                ReceivedValue[key] = floatArr[i];
            }
        }
    }
}
