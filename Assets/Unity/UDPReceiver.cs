using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace ExpressionAppForVRM
{
    public class UDPReceiver : MonoBehaviour
    {
        // ソケット
        private UdpClient udpClient;

        // 受信ポート番号
        public int port = 9000;

        // 受信した文字列
        private string receivedString = "";

        [SerializeField]
        MoveManager moveManager = null;

        void Start()
        {
            // ソケット作成
            udpClient = new UdpClient(port);

            // 非同期受信開始（コールバック関数指定）
            udpClient.BeginReceive(ReceiveCallback, null);

            Debug.Log("Start receiving...");
        }

        void Update()
        {
            // 受信した文字列があれば表示する
            if (receivedString != "")
            {
                moveManager.UpdateRecivedValue(receivedString);

                receivedString = "";
            }
        }

        void OnApplicationQuit()
        {
            // ソケット終了
            udpClient.Close();

            Debug.Log("Stop receiving.");
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // 受信データと送信元アドレスを取得する
                IPEndPoint remoteEP = null;
                byte[] rcvBytes = udpClient.EndReceive(ar, ref remoteEP);

                // 受信したデータから文字列長さと文字列データを分離する（C++側と同じ処理）
                byte[] lenBytes = new byte[4];
                Array.Copy(rcvBytes, 0, lenBytes, 0, 4);
                byte[] strBytes = new byte[rcvBytes.Length - 4];
                Array.Copy(rcvBytes, 4, strBytes, 0, rcvBytes.Length - 4);

                // 文字列長さをuint32_tに変換してホストバイトオーダーにする（C++側と同じ処理）
                uint len = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenBytes));

                // 文字列データをUTF-8でエンコードする（C++側とエンコード方式が一致している必要がある）
                string str = Encoding.UTF8.GetString(strBytes);

                receivedString = str;

                // 再び非同期受信開始（コールバック関数指定）
                udpClient.BeginReceive(ReceiveCallback, null);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }
}