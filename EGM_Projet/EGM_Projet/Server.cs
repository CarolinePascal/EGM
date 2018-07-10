using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;


namespace EGM_Projet
{
    public abstract class Server
    {

        private Thread thread;

        protected UdpClient udpClient;
        /// <summary>
        /// Port of the UDP communication
        /// </summary>
        protected int Port;

        public bool exit;
        public bool wait;
        public bool reboot;

        protected int seqNumber;

        protected int refTime;

        public ManualResetEvent stop;

        /// <summary>
        /// Default constructor of a Server instance
        /// </summary>
        public Server()
        {
            thread = null;

            udpClient = null;
            Port = 0;

            exit = false;
            wait = false;
            reboot = true;

            refTime = 0;

            stop = new ManualResetEvent(false);
        }

        /// <summary>
        /// Constructor of a Server instance with UDP port argument
        /// </summary>
        /// <param name="IPport">Port of the UDP communication</param>
        public Server(int IPport)
        {
            thread = null;

            udpClient = null;
            Port = IPport;

            exit = false;
            wait = false;
            reboot = false;

            refTime = 0;

            stop = new ManualResetEvent(false);
        }

        /// <summary>
        /// Starts the server thread 
        /// </summary>
        public void Start()
        {
            exit = false;
            wait = false;

            stop.Reset();

            thread = new Thread(new ThreadStart(Init));
            thread.Start();
        }

        /// <summary>
        /// Stops the server thread and the UDP communication
        /// </summary>
        public void Stop()
        {
            exit = true;
            udpClient.Close();

            stop.Set();

            if (!reboot) { thread.Abort(); }
        }

        /// <summary>
        /// Creates the UDPClient, Main loop and Stop()
        /// </summary>
        public void Init()
        {
            udpClient = new UdpClient(Port);

            Console.WriteLine("Connexion avec le serveur - Port : " + Port);

            int n;

            Main(out n);

            Counter(n);
            Stop();
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
    }
}
