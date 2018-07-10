using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using abb.egm;


///////////////////////////////////////////////////////////////////////////////////////////////////////////


namespace EGM_Projet
{
    class Program
    {
        public static Plot plot;

        public static void Main(string[] args)
        {
            Torque_Server TServer = new Torque_Server(5000);
            Order_Server OServer = new Order_Server(4000);

            List<Server> servers = new List<Server>();
            servers.Add(OServer);
            servers.Add(TServer);

            EGM_Server MainServer = new EGM_Server(servers, 5000);

            ManualResetEvent[] events = new ManualResetEvent[servers.Count+1];
            for(int i=0;i<servers.Count;i++)
            {
                events[i] = servers[i].stop;
            }
            events[events.Length-1] = MainServer.stop;

            while (MainServer.reboot)
            {
                OServer.Start();
                TServer.Start();
                MainServer.Start();

                WaitHandle.WaitAll(events);
            }
        }
    }
}
