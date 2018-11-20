using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

using System.Diagnostics;


/// <summary>
/// **SENSOR CONNECTION**
/// 
/// 1) Unplug the Ethernet cable from the LAN port on the computer
/// 2) Set the IPV4 IP Address on "192.168.1.100"
/// 3) Set the subnet mask on "255.255.255.0"
/// 4) Plug the Ethernet cable back again
/// 
/// The sensor is now accessible on the following IP Address : "192.168.1.1"
///     -> This IP Address can be modified on the sensor interface, but only if the switch 9 of the Netbox is off
/// </summary>

namespace ForceSensor
{
    class SensorServer
    {
        private bool _isConnected;

        /// <summary>
        /// UDP communication port of the sensor
        /// </summary>
        private const int _port = 49152;

        private IPAddress _ipAddress;
        private UdpClient _udpClient;
        private IPEndPoint _remoteEP;
        
        /// <summary>
        /// Static gain of the sensor as a float - can be modified on its interface
        /// </summary>
        private const double _gain = 1000000.0;

        /// <summary>
        /// The recorded efforts are stored as a queue of doubles array  - the size of the queue is 100 by default, the size of the arrays is 6
        /// </summary>
        private Queue<double[]> _efforts;
        private int _sizeQueue = 100;

        /// <summary>
        /// The instant current efforts are stored in a double array
        /// </summary>
        private double[] _temp;

        /// <summary>
        /// Constructor of a SensorServer instance with a specified IP Address
        /// </summary>
        /// <param name="address">IP Address a a string</param>
        /// <param name="size">Size of the sensor queue as an integer</param>
        public SensorServer(string address, int size)
        {
            _isConnected = false;
            _sizeQueue = size;
            InitEfforts();
            _ipAddress = IPAddress.Parse(address);
        }

        /// <summary>
        /// Initialisation of the _efforts array
        /// </summary>
        public void InitEfforts()
        {
            _efforts = new Queue<double[]>(_sizeQueue);
            _temp = new double[6];
        }

        /// <summary>
        /// Starts the UDP communication with the sensor
        /// </summary>
        public void Start()
        {
            _udpClient = new UdpClient(_port);
            _remoteEP = new IPEndPoint(_ipAddress, _port);
            _udpClient.Connect(_remoteEP);

            if (!_isConnected)
            {
                byte[] message = new byte[8];

                short header = IPAddress.HostToNetworkOrder((short)0x1234);
                short order = IPAddress.HostToNetworkOrder((short)0x0042);
                int time = IPAddress.HostToNetworkOrder((int)0);

                BitConverter.GetBytes(header).CopyTo(message, 0);
                BitConverter.GetBytes(order).CopyTo(message, 2);
                BitConverter.GetBytes(time).CopyTo(message, 4);

                _udpClient.Send(message, 8);

                order = IPAddress.HostToNetworkOrder((short)0x0002);
                BitConverter.GetBytes(order).CopyTo(message, 2);
                _udpClient.Send(message, 8);

            }

            _isConnected = true;
        }

        /// <summary>
        /// Stops the UDP Communication with the sensor
        /// </summary>
        public void Stop()
        {
            if (_isConnected)
            {
                byte[] message = new byte[8];

                short header = IPAddress.HostToNetworkOrder((short)0x1234);
                short order = IPAddress.HostToNetworkOrder((short)0x0000);
                int time = IPAddress.HostToNetworkOrder((int)0);

                BitConverter.GetBytes(header).CopyTo(message, 0);
                BitConverter.GetBytes(order).CopyTo(message, 2);
                BitConverter.GetBytes(time).CopyTo(message, 4);

                _udpClient.Send(message,8);
            }

            _isConnected = false;
            _udpClient.Close();
        }

        /// <summary>
        /// Parses the messages recieved from the sensos and records the efforts values
        /// </summary>
        public void Parse()
        {
            byte[] slicing = new byte[4];
            byte[] answer = new byte[36];
            answer = _udpClient.Receive(ref _remoteEP);

            bool flag = true;

            double[] buffer = new double[6];

            if(_efforts.Count < _sizeQueue)
            {
                flag = false;
            }

            for (int i=0;i<6;i++)
            {
                Array.Copy(answer, 12 + 4 * i, slicing, 0, 4);
                buffer[i] = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(slicing, 0)) / _gain;
            }

            if (flag)
            {
                _efforts.Dequeue();
            }
            _temp = buffer;
            _efforts.Enqueue(_temp);
        }

        /// <summary>
        /// Main loop of the communication with the sensor - The GetState() method is called every 50 messages received
        /// </summary>
        public void Main()
        {
            while(true)
            {
                //Parse()
            }
        }

        /// <summary>
        /// Get the averaged efforts monitored by the sensor
        /// </summary>
        /// <returns>Averaged efforts since the begining of th communication or last GetState() call</returns>
        public double[] GetStateAvg()
        {
            double[] results = new double[6];

            foreach(double[] torsor in _efforts)
            {
                Console.WriteLine(torsor[0]);
                for(int i=0;i<6;i++)
                {
                    results[i] += torsor[i] / _sizeQueue;
                }
            }
            return (results);
        }

        public double[] GetStateLast()
        {
            return (_temp);
        }
    }
}
