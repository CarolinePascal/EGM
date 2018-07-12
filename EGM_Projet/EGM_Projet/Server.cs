using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;


namespace EGMProjet
{
    public abstract class Server
    {

        private Thread _thread;

        protected UdpClient _udpClient;
        /// <summary>
        /// Port of the UDP communication
        /// </summary>
        protected int _port;

        public bool Exit { get; set; }
        public bool Wait { get; set; }
        public bool Reboot { get; set; }

        protected int _seqNumber;

        protected int _refTime;

        public ManualResetEvent Stop { get; set; }

        /// <summary>
        /// Default constructor of a Server instance
        /// </summary>
        public Server()
        {
            _thread = null;

            _udpClient = null;
            _port = 0;

            Exit = false;
            Wait = false;
            Reboot = true;

            _refTime = 0;

            Stop = new ManualResetEvent(false);
        }

        /// <summary>
        /// Constructor of a Server instance with UDP port argument
        /// </summary>
        /// <param name="IPport">Port of the UDP communication</param>
        public Server(int IPport)
        {
            _thread = null;

            _udpClient = null;
            _port = IPport;

            Exit = false;
            Wait = false;
            Reboot = false;

            _refTime = 0;

            Stop = new ManualResetEvent(false);
        }

        /// <summary>
        /// Starts the server thread 
        /// </summary>
        public void StartServer()
        {
            Exit = false;
            Wait = false;

            Stop.Reset();

            _thread = new Thread(new ThreadStart(Init));
            _thread.Start();
        }

        /// <summary>
        /// Stops the server thread and the UDP communication
        /// </summary>
        public void StopServer()
        {
            Exit = true;
            _udpClient.Close();

            Stop.Set();

            if (!Reboot) { _thread.Abort(); }
        }

        /// <summary>
        /// Creates the UDPClient, Main loop and Stop()
        /// </summary>
        public void Init()
        {
            _udpClient = new UdpClient(_port);

            Console.WriteLine("Connexion avec le serveur - Port : " + _port);

            int n;

            Main(out n);

            Counter(n);
            StopServer();
        }

        /// <summary>
        /// Displays the number n of recieved messages
        /// </summary>
        /// <param name="n">Number of recieved messages</param>
        public abstract void Counter(int n);

        /// <summary>
        /// Main loop of the server thread
        /// </summary>
        /// <param name="n">Number of recieved messages</param>
        public abstract void Main(out int n);

        /// <summary>
        /// Returns the interest value of the server as a string
        /// </summary>
        /// <returns></returns>
        public abstract string GetState();
    }
}
