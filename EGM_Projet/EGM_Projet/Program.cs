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
            //Non-EGM Servers
            Torque_Server TServer = new Torque_Server(5000);
            Order_Server OServer = new Order_Server(4000);

            // Add all the servers which are not EGM servers
            List<Server> servers = new List<Server>();
            servers.Add(OServer);
            servers.Add(TServer);

            //EGM Server
            EGM_Server MainServer = new EGM_Server(servers, 5000);

            //Event table : the events are set when the servers's threads ends
            ManualResetEvent[] events = new ManualResetEvent[servers.Count+1];
            for(int i=0;i<servers.Count;i++)
            {
                events[i] = servers[i].stop;
            }
            events[events.Length-1] = MainServer.stop;

            //Main loop
            while (MainServer.reboot)
            {
                plot = new Plot();
                OServer.Start();
                TServer.Start();
                MainServer.Start();

                WaitHandle.WaitAll(events);
                plot.Clear();
            }
        }
    }
}
