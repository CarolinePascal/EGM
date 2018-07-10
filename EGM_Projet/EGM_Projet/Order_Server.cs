using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EGM_Projet
{
    public class Order_Server : Input_Server
    {
        //Ordered position
        public float x;
        public float y;
        public float z;

        public Order_Server(int IPport) : base(IPport)
        {
            x = 0;
            y = 0;
            z = 0;
        }

        public override void Parsing(string returnData)
        {
            returnData = returnData.Replace('.', ',');
            String[] substrings = returnData.Split(' ');

            x = float.Parse(substrings[0]);
            y = float.Parse(substrings[1]);
            z = float.Parse(substrings[2]);
        }


    }
}
