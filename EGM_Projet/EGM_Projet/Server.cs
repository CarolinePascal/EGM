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
        protected int Port;

        public bool exit;
        public bool wait;
        public bool reboot;

        protected int seqNumber;

        protected int refTime;

        public ManualResetEvent stop;

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

        public void Start()
        {
            exit = false;
            wait = false;

            stop.Reset();

            thread = new Thread(new ThreadStart(Init));
            thread.Start();
        }

        public void Stop()
        {
            exit = true;
            udpClient.Close();

            stop.Set();

            if (!reboot) { thread.Abort(); }
        }

        public void Init()
        {
            udpClient = new UdpClient(Port);

            Console.WriteLine("Connexion avec le serveur - Port : " + Port);

            int n = Main();

            Counter(n);
            Stop();
        }

        public abstract void Counter(int n);

        public abstract int Main();
    }
}
