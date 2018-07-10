using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace EGM_Projet
{
    public abstract class Input_Server : Server
    {
        //Recieved data
        string returnData;

        public Input_Server(int IPport) : base(IPport)
        {
            returnData = null;
        }

        public override int Main()
        {
            int n = 0;
            var remoteEP = new IPEndPoint(IPAddress.Any, Port);

            while (exit==false)
            {
                do
                {
                    if (exit) { break; }
                } while (wait);

                var data = udpClient.Receive(ref remoteEP);

                if (data != null)
                {
                    n++;
                    returnData = Encoding.ASCII.GetString(data);
                    Parsing(returnData);

                }
            }
            
            return(n);
        }

        public override void Counter(int n)
        {
            Console.WriteLine("Messages reçus : " + n);
        }

        public abstract void Parsing(string returnData);
    }
}
