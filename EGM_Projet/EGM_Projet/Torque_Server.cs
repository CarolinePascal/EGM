using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace EGMProjet
{
    class TorqueServer : InputServer
    {
        /// <summary>
        /// 6 motor torques values
        /// </summary>
        private List<double>[] _torques;

        /// <summary>
        /// Default contructor for a Torque_Server instance with UDP port argument
        /// </summary>
        /// <param name="IPport">Port of the UDP communication different from 6510</param>
        public TorqueServer(int ipPort):base(ipPort)
        {
            InitTorque();
        }

        private void InitTorque()
        {
            _torques = new List<double>[6];
            for (int i=0;i<6;i++)
            {
                _torques[i] = new List<double>();
            }
        }

        /// <summary>
        /// Parses and records the recieved messages containing the torques values : Time Taxis1 Taxis2 Taxis3 Taxis4 Taxis5 Taxis6
        /// </summary>
        /// <param name="returnData">Recieved message as a string</param>
        public override void Parsing(string returnData)
        {
            String[] substrings = returnData.Split(' ');
            returnData = returnData.Replace('.', ',');
            String[] substrings2 = returnData.Split(' ');
            int temps = Int32.Parse(substrings[0]);

                for (int i=0;i<6;i++)
                {
                    _torques[i].Add(double.Parse(substrings2[i + 1]));
                    //Program.plot.FillTorque(substrings[1], substrings[2], substrings[3], substrings[4], substrings[5], substrings[6], substrings[0]);
                }
            
        }

        /// <summary>
        /// Returns the 6 averaged torque values position as a string
        /// </summary>
        /// <returns></returns>
        public override string GetState()
        {
            double[] results = new double[6];
            string str = System.String.Empty;
            for (int i=0;i<6;i++)
            {
                results[i] = _torques[i].Average();
                str += results[i].ToString() + " ";
            }
            Console.WriteLine(str);
            return (str);          
        }
    }
}
