using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace TakuUdp
{
    public class UdpReceiver : MonoBehaviour
    {
        [SerializeField] private List<Transform> trackers = new List<Transform>();

        private UdpClient _client;
        [SerializeField] private int port;
        private IPEndPoint _ipEndPoint;

        private void Start()
        {
            ReceiveEvent += b =>
            {
                int size_d = sizeof(double);
                byte[] x = new byte[size_d];
                byte[] y = new byte[size_d];
                byte[] z = new byte[size_d];

                byte[] r_x = new byte[size_d];
                byte[] r_y = new byte[size_d];
                byte[] r_z = new byte[size_d];
                byte[] r_w = new byte[size_d];

                int index = 0;

                foreach (var tracker in trackers)
                {
                    if (index > b.Length)
                        return;
                    
                    //Position   
                    Array.Copy(b, index, x, 0, size_d);
                    index = index + size_d;
                    Array.Copy(b, index, y, 0, size_d);
                    index = index + size_d;
                    Array.Copy(b, index, z, 0, size_d);
                    index = index + size_d;
                    
                    //Quaternion
                    Array.Copy(b, index, r_x, 0, size_d);
                    index = index + size_d;
                    Array.Copy(b, index, r_y, 0, size_d);
                    index = index + size_d;
                    Array.Copy(b, index, r_z, 0, size_d);
                    index = index + size_d;
                    Array.Copy(b, index, r_w, 0, size_d);
                    index = index + size_d;

                    var x_f = BitConverter.ToDouble(x, 0);
                    var y_f = BitConverter.ToDouble(y, 0);
                    var z_f = BitConverter.ToDouble(z, 0);

                    var rxf = BitConverter.ToDouble(r_x, 0);
                    var ryf = BitConverter.ToDouble(r_y, 0);
                    var rzf = BitConverter.ToDouble(r_z, 0);
                    var rwf = BitConverter.ToDouble(r_w, 0);
                    tracker.position = new Vector3((float) x_f, (float) y_f, (float) z_f);
                    tracker.rotation = new Quaternion((float) rxf, (float) ryf, (float) rzf, (float) rwf);
                }
            };

            _ipEndPoint = new IPEndPoint(IPAddress.Any, port);

            _client = new UdpClient(_ipEndPoint);


            //s.Result.Buffer
            Receive();

        }

        async Task Receive()
        {
            var s = await _client.ReceiveAsync();
            ReceiveEvent?.Invoke(s.Buffer);
            Receive();
        }

        public event Action<byte[]> ReceiveEvent;

        public void OnApplicationQuit()
        {
            _client.Dispose();
        }
    }
}