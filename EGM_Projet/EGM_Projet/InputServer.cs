using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace EGMProjet
{
    public abstract class InputServer : Server
    {
        /// <summary>
        /// Recieved data as a string
        /// </summary>
        private string _returnData;

        /// <summary>
        /// Constructor of a Input_Server instance with UDP port argument
        /// </summary>
        /// <param name="IPport">Port of the UDP communication</param>
        public InputServer(int ipPort, string ipAddress) : base(ipPort, ipAddress)
        {
            _returnData = null;
        }

        /// <summary>
        /// Main loop for Input_Server :
        /// - check status for pausing or stopping the communication
        /// - Recieving and parsing input data
        /// </summary>
        /// <param name="n">Number of recieved messages</param>
        public override void Main(out int n, IPEndPoint remoteEP)
        {
            n = 0;

            while (Exit==false)
            {
                do
                {
                    if (Exit) { break; }
                } while (Wait);

                var data = _udpClient.Receive(ref remoteEP);

                if (data != null)
                {
                    n++;
                    _returnData = Encoding.ASCII.GetString(data);
                    Parsing(_returnData);
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
