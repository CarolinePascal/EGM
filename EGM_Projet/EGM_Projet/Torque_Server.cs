using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EGM_Projet
{
    class Torque_Server : Input_Server
    {
        //6 Torques
        double[] torques = new double[6];

        int check;

        public Torque_Server(int IPport):base(IPport)
        {
            for (int i=0;i<6;i++)
            {
                torques[i] = 0;
            }
            check = 1;

        }

        public override void Parsing(string returnData)
        {
            returnData = returnData.Replace('.', ',');
            String[] substrings = returnData.Split(' ');
            int temps = Int32.Parse(substrings[0]);

            if (temps % 4 == 0 && temps != check)
            {
                check = temps;
                for (int i=0;i<6;i++)
                {
                    torques[i] = double.Parse(substrings[i + 1]);
                }
            }
        }
    }
}
