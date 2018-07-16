using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ForceSensor
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
        private IPAddress _ipAddress;
        private IPEndPoint _remoteEP;

        
        private const double _gain = 1000000.0;

        private List<double>[] _efforts;

        public SensorServer(string address)
        {
            _isConnected = false;
            InitEfforts();
            _ipAddress = IPAddress.Parse(address);
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
            _udpClient = new UdpClient(_port);
            _remoteEP = new IPEndPoint(_ipAddress, _port);
            _udpClient.Connect(_remoteEP);

            if (!_isConnected)
            {
                short header = IPAddress.HostToNetworkOrder((short)0x1234);
                short order = IPAddress.HostToNetworkOrder((short)0x0002);
                int time = IPAddress.HostToNetworkOrder((int)0);

                BitConverter.GetBytes(header).CopyTo(_message, 0);
                BitConverter.GetBytes(order).CopyTo(_message, 2);
                BitConverter.GetBytes(time).CopyTo(_message, 4);

                _udpClient.Send(_message, _messageLength);

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

        public void Main()
        {
            int n = 0;
            while (n <= 5000)
            {
                _answer = _udpClient.Receive(ref _remoteEP);
                Parse();
                n++;
                if(n%50==0)
                {
                    GetState();
                }
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
    }
}
