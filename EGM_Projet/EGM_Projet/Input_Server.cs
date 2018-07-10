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
        /// <summary>
        /// Recieved data as a string
        /// </summary>
        string returnData;

        /// <summary>
        /// Constructor of a Input_Server instance with UDP port argument
        /// </summary>
        /// <param name="IPport">Port of the UDP communication</param>
        public Input_Server(int IPport) : base(IPport)
        {
            returnData = null;
        }

        /// <summary>
        /// Main loop for Input_Server :
        /// - check status for pausing or stopping the communication
        /// - Recieving and parsing input data
        /// </summary>
        /// <param name="n">Number of recieved messages</param>
        public override void Main(out int n)
        {
            n = 0;
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
        }

        /// <summary>
        /// Displays the number n of recieved messages
        /// </summary>
        /// <param name="n">Number of recieved messages</param>
        public override void Counter(int n)
        {
            Console.WriteLine("Messages reçus : " + n);
        }

        /// <summary>
        /// Parses the recieved data
        /// </summary>
        /// <param name="returnData">Recieved data as a string</param>
        public abstract void Parsing(string returnData);
    }
}
