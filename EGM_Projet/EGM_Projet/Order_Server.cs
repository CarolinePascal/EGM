using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EGM_Projet
{
    public class Order_Server : Input_Server
    {
        //Ordered robot's position
        public float x;
        public float y;
        public float z;

        /// <summary>
        /// Default contructor for a Order_Server instance with UDP port argument
        /// </summary>
        /// <param name="IPport">Port of the UDP communication different from 6510</param>
        public Order_Server(int IPport) : base(IPport)
        {
            x = 0;
            y = 0;
            z = 0;
        }

        /// <summary>
        /// Parses and records the recieved messages containing the ordered positions : X Y Z
        /// </summary>
        /// <param name="returnData">Recieved message as a string</param>
        public override void Parsing(string returnData)
        {
            returnData = returnData.Replace('.', ',');
            String[] substrings = returnData.Split(' ');

            x = float.Parse(substrings[0]);
            y = float.Parse(substrings[1]);
            z = float.Parse(substrings[2]);
        }
        
        public override string GetState()
        {
            string str = x.ToString() + " " + y.ToString() + " " + z.ToString();
            Console.WriteLine(str);
            return (str);
        }

    }
}
