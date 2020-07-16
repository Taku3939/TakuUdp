using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEditor;
using UnityEngine;

namespace TakuUdp
{
    public class UdpSender : MonoBehaviour
    {
        [SerializeField] private List<Transform> trackers = new List<Transform>();
        [SerializeField] private string ip;
        [SerializeField] private int port;

        [SerializeField] private int packet_limit = 1024;
        private UdpClient client;
        public bool IsConnect { get; set; }
        
        public void Connect()
        {
            client = new UdpClient();
            client.Connect(ip, port);
            IsConnect = true;
        }

        public void Disconnect()
        {
            if (client == null)
            {
                return;
            }
            client.Dispose();
            IsConnect = false;
        }

        /// <summary>
        /// Udp Send
        /// </summary>
        public void Send()
        {
            List<Transform> transforms = new List<Transform>();
            trackers.ForEach(x => transforms.Add(x));
            Send(transforms);
        }
        
        public void Send(List<Transform> transforms)
        {
            var bits = convert(transforms);
            client.SendAsync(bits, bits.Length);
        }
        
        /// <summary>
        /// Transformの配列をdouble型をbyte配列変換する
        /// </summary>
        /// <param name="transforms">ObjectのTransform</param>
        /// <returns></returns>
        byte[] convert(List<Transform> transforms)
        {
            byte[] sendFrame = new byte[1024];
            
            int index = 0;
            var size_d = sizeof(double);
            transforms.ForEach(tr =>
            {
                //Position
                var pos = tr.position;
                byte[] x = BitConverter.GetBytes((double)pos.x);
                byte[] y = BitConverter.GetBytes((double)pos.y);
                byte[] z = BitConverter.GetBytes((double)pos.z);

                
                //Quaternion
                var rot = tr.rotation;
                byte[] r_x = BitConverter.GetBytes((double)rot.x);
                byte[] r_y = BitConverter.GetBytes((double)rot.y);
                byte[] r_z = BitConverter.GetBytes((double)rot.z);
                byte[] r_w = BitConverter.GetBytes((double)rot.w);
                
                //Position
                Array.Copy(x, 0, sendFrame, index, size_d);
                index += size_d;
                Array.Copy(y, 0, sendFrame, index, size_d);
                index += size_d; 
                Array.Copy(z, 0, sendFrame, index, size_d);
                index += size_d;
                
                //Quaternion
                Array.Copy(r_x, 0, sendFrame, index, size_d);
                index += size_d;
                Array.Copy(r_y, 0, sendFrame, index, size_d);
                index += size_d; 
                Array.Copy(r_z, 0, sendFrame, index, size_d);
                index += size_d;
                Array.Copy(r_w, 0, sendFrame, index, size_d);
                index += size_d;
            });

            return sendFrame;
        }
        
        private void Update()
        {
            if(!IsConnect)
                return;
            Send();
        }

        public void OnApplicationQuit()
        {
            client.Dispose();
        }
    }

    [CustomEditor(typeof(UdpSender))]
    public class UdpSenderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var sender = target as UdpSender;
            
            EditorGUI.BeginChangeCheck();

            sender.IsConnect = GUILayout.Toggle(sender.IsConnect, new GUIContent("IsConnect"));
            
            if (EditorGUI.EndChangeCheck())
            {
                if (sender.IsConnect)
                {
                    sender.Connect();
                }
                else
                {
                    sender.Disconnect();
                }
                
            }
        }
    }
}