using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EGMProjet
{
    public class OrderServer : InputServer
    {
        //Ordered robot's position
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        /// <summary>
        /// Default contructor for a Order_Server instance with UDP port argument
        /// </summary>
        /// <param name="IPport">Port of the UDP communication different from 6510</param>
        public OrderServer(int ipPort) : base(ipPort)
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        /// <summary>
        /// Parses and records the recieved messages containing the ordered positions : X Y Z
        /// </summary>
        /// <param name="returnData">Recieved message as a string</param>
        public override void Parsing(string returnData)
        {
            returnData = returnData.Replace('.', ',');
            String[] substrings = returnData.Split(' ');

            X = float.Parse(substrings[0]);
            Y = float.Parse(substrings[1]);
            Z = float.Parse(substrings[2]);
        }
        
        public override string GetState()
        {
            string str = X.ToString() + " " + Y.ToString() + " " + Z.ToString();
            Console.WriteLine(str);
            return (str);
        }

    }
}
