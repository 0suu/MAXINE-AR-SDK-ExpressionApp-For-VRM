using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace ExpressionAppForVRM
{
    public class UDPReceiver : MonoBehaviour
    {
        // �\�P�b�g
        private UdpClient udpClient;

        // ��M�|�[�g�ԍ�
        public int port = 9000;

        // ��M����������
        private string receivedString = "";

        [SerializeField]
        MoveManager moveManager = null;

        void Start()
        {
            // �\�P�b�g�쐬
            udpClient = new UdpClient(port);

            // �񓯊���M�J�n�i�R�[���o�b�N�֐��w��j
            udpClient.BeginReceive(ReceiveCallback, null);

            Debug.Log("Start receiving...");
        }

        void Update()
        {
            // ��M���������񂪂���Ε\������
            if (receivedString != "")
            {
                moveManager.UpdateRecivedValue(receivedString);

                receivedString = "";
            }
        }

        void OnApplicationQuit()
        {
            // �\�P�b�g�I��
            udpClient.Close();

            Debug.Log("Stop receiving.");
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // ��M�f�[�^�Ƒ��M���A�h���X���擾����
                IPEndPoint remoteEP = null;
                byte[] rcvBytes = udpClient.EndReceive(ar, ref remoteEP);

                // ��M�����f�[�^���當���񒷂��ƕ�����f�[�^�𕪗�����iC++���Ɠ��������j
                byte[] lenBytes = new byte[4];
                Array.Copy(rcvBytes, 0, lenBytes, 0, 4);
                byte[] strBytes = new byte[rcvBytes.Length - 4];
                Array.Copy(rcvBytes, 4, strBytes, 0, rcvBytes.Length - 4);

                // �����񒷂���uint32_t�ɕϊ����ăz�X�g�o�C�g�I�[�_�[�ɂ���iC++���Ɠ��������j
                uint len = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenBytes));

                // ������f�[�^��UTF-8�ŃG���R�[�h����iC++���ƃG���R�[�h��������v���Ă���K�v������j
                string str = Encoding.UTF8.GetString(strBytes);

                receivedString = str;

                // �Ăє񓯊���M�J�n�i�R�[���o�b�N�֐��w��j
                udpClient.BeginReceive(ReceiveCallback, null);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }
}