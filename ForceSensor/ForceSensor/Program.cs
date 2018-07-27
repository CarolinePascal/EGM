using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForceSensor
{
    class Program
    {
        static void Main(string[] args)
        {
            SensorServer s = new SensorServer("192.168.1.1");
            s.Start();
            s.Main();
            s.Stop();
        }
    }
}
