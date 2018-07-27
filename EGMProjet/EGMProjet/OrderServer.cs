using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D; //Add referernce : PresentationCore.dll


namespace EGMProjet
{
    public class OrderServer : InputServer
    {
        //Ordered robot's position
        public Vector3D Vector { get; set; }
        public EulerAngles Angles { get; set; }

        /// <summary>
        /// Default contructor for a Order_Server instance with UDP port argument
        /// </summary>
        /// <param name="IPport">Port of the UDP communication different from 6510</param>
        public OrderServer(int ipPort, string ipAddress) : base(ipPort, ipAddress)
        {
            Vector = new Vector3D(0, 0, 0);
            Angles = new EulerAngles(0, 0, 0);
        }

        /// <summary>
        /// Parses and records the recieved messages containing the ordered positions : X Y Z
        /// </summary>
        /// <param name="returnData">Recieved message as a string</param>
        public override void Parsing(string returnData)
        {
            returnData = returnData.Replace('.', ',');
            String[] substrings = returnData.Split(' ');

            Vector = new Vector3D(double.Parse(substrings[0]), double.Parse(substrings[1]), double.Parse(substrings[2]));
            Angles = new EulerAngles(double.Parse(substrings[3]), double.Parse(substrings[4]), double.Parse(substrings[5]));
        }
        
        public override string GetState()
        {
            string str = Vector.ToString() + " " + Angles.ToString();
            Console.WriteLine(str);
            return (str);
        }

    }
}
