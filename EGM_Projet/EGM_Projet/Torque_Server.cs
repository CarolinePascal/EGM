﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EGM_Projet
{
    class Torque_Server : Input_Server
    {
        /// <summary>
        /// 6 motor torques values
        /// </summary>
        double[] torques = new double[6];

        int check;

        /// <summary>
        /// Default contructor for a Torque_Server instance with UDP port argument
        /// </summary>
        /// <param name="IPport">Port of the UDP communication different from 6510</param>
        public Torque_Server(int IPport):base(IPport)
        {
            for (int i=0;i<6;i++)
            {
                torques[i] = 0;
            }
            check = 1;

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

            if (temps % 4 == 0 && temps != check)
            {
                check = temps;
                for (int i=0;i<6;i++)
                {
                    torques[i] = double.Parse(substrings2[i + 1]);
                    Program.plot.FillTorque(substrings[1], substrings[2], substrings[3], substrings[4], substrings[5], substrings[6], substrings[0]);
                }
            }
        }
    }
}
