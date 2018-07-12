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


namespace EGMProjet
{
    class Program
    {
        //public static Plot Plot;

        public static void Main(string[] args)
        {
            //Non-EGM Servers
            TorqueServer serverT = new TorqueServer(5000);
            OrderServer serverO = new OrderServer(4000);

            // Add all the servers which are not EGM servers
            List<Server> servers = new List<Server>();
            servers.Add(serverO);
            servers.Add(serverT);

            //EGM Server
            EGMServer serverMain = new EGMServer(servers,1000);

            //Event table : the events are set when the servers's threads ends
            ManualResetEvent[] events = new ManualResetEvent[servers.Count+1];
            for(int i=0;i<servers.Count;i++)
            {
                events[i] = servers[i].Stop;
            }
            events[events.Length-1] = serverMain.Stop;

            //Main loop
            while (serverMain.Reboot)
            {
                //Plot = new Plot();
                serverO.StartServer();
                serverT.StartServer();
                serverMain.StartServer();

                WaitHandle.WaitAll(events);
                //Plot.Clear();
                
            }
        }
    }
}
