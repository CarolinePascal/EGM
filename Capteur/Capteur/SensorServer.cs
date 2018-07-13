using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Capteur
{
    class SensorServer
    {
        private bool _isConnected;

        private const int _messageLength=8;
        private byte[] _message = new byte[8];

        private const int _answerLength = 36;
        private byte[] _answer = new byte[_answerLength];

        private UdpClient _udpClient;
        private const int _port = 49152;
        public IPAddress IPAddress { get; set; }
        
        private const double _gain = 1000000.0;

        private List<double>[] _efforts;

        public SensorServer(string address)
        {
            _isConnected = false;
            InitEfforts();
            IPAddress = IPAddress.Parse(address);
        }

        public void InitEfforts()
        {
            _efforts = new List<double>[6];
            for (int i = 0; i < 6; i++)
            {
                _efforts[i] = new List<double>();
            }
        }

        public void Start()
        {
            var remoteEP = new IPEndPoint(IPAddress, _port);
            _udpClient = new UdpClient(remoteEP);

            if (!_isConnected)
            {
                short header = IPAddress.HostToNetworkOrder((short)0x1234);
                short order = IPAddress.HostToNetworkOrder((short)0x0002);
                int time = IPAddress.HostToNetworkOrder((int)0);

                BitConverter.GetBytes(header).CopyTo(_message, 0);
                BitConverter.GetBytes(order).CopyTo(_message, 2);
                BitConverter.GetBytes(time).CopyTo(_message, 4);

                Console.WriteLine(BitConverter.ToString(_message));
            }

            _isConnected = true;
        }

        public void Stop()
        {
            if (_isConnected)
            {
                short header = IPAddress.HostToNetworkOrder((short)0x1234);
                short order = IPAddress.HostToNetworkOrder((short)0x0000);
                int time = IPAddress.HostToNetworkOrder((int)0);

                BitConverter.GetBytes(header).CopyTo(_message, 0);
                BitConverter.GetBytes(order).CopyTo(_message, 2);
                BitConverter.GetBytes(time).CopyTo(_message, 4);

                _udpClient.Send(_message, _messageLength);
            }

            _isConnected = false;
            _udpClient.Close();
        }

        public void Parse()
        {
            byte[] slicing = new byte[4];

            for (int i=0;i<6;i++)
            {
                Array.Copy(_answer, 12 + 4 * i, slicing, 0, 4);
                _efforts[i].Add(IPAddress.NetworkToHostOrder(BitConverter.ToInt32(slicing, 0))/_gain);
            }
        }

        public double[] GetState()
        {
            string str = String.Empty;
            double[] results = new double[6];

            for (int i=0;i<6;i++)
            {
                results[i] = _efforts[i].Average();
                str += " " + results[i].ToString();
            }
            Console.WriteLine(str);
            InitEfforts();
            return (results);
        }

        public void Test()
        {
            int header1 = IPAddress.HostToNetworkOrder((int)0);
            int header2 = IPAddress.HostToNetworkOrder((int)0);
            int header3 = IPAddress.HostToNetworkOrder((int)0);
            int Fx = IPAddress.HostToNetworkOrder((int)123456);
            int Fy = IPAddress.HostToNetworkOrder((int)0);
            int Fz = IPAddress.HostToNetworkOrder((int)123456789);
            int Mx = IPAddress.HostToNetworkOrder((int)123456);
            int My = IPAddress.HostToNetworkOrder((int)0);
            int Mz = IPAddress.HostToNetworkOrder((int)123456789);


            BitConverter.GetBytes(header1).CopyTo(_answer, 0);
            BitConverter.GetBytes(header2).CopyTo(_answer, 4);
            BitConverter.GetBytes(header3).CopyTo(_answer, 8);
            BitConverter.GetBytes(Fx).CopyTo(_answer, 12);
            BitConverter.GetBytes(Fy).CopyTo(_answer, 16);
            BitConverter.GetBytes(Fz).CopyTo(_answer, 20);
            BitConverter.GetBytes(Mx).CopyTo(_answer, 24);
            BitConverter.GetBytes(My).CopyTo(_answer, 28);
            BitConverter.GetBytes(Mz).CopyTo(_answer, 32);
        }





    }
}
