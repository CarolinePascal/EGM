using System;
using System.Collections.Generic;
using System.Net;
using HAL.ENPC.Control;
using HAL.ENPC.Messaging.Server.ABB;
using HAL.ENPC.Messaging.Server.ATI;
using HAL.ENPC.Sensoring;

namespace HAL.ENPC.Debug
{
    class Program
    {
        static void Main(string[] args)
        {

            List<RealTimeController> sensors = new List<RealTimeController>();
            ControlStrategy controlStrategy = null;
            ControlStrategy.Create("egm", (int)ControlableType.Joint, out controlStrategy);

            EgmServer egmServer = new EgmServer("EgmServer", IPAddress.Parse("127.0.0.1"), 6510, 6510);
            ForceSensorServer atiServer = new ForceSensorServer("atiServer", IPAddress.Parse("192.168.1.1"), 49152, 49152);
            Sensor egmSensor = new Sensor(new Identifier("EgmSensor"), null, egmServer);
            Sensor atiSensor = new Sensor(new Identifier("AtiSensor"), null, atiServer);
            sensors.Add(egmSensor);
            sensors.Add(atiSensor);
            RealTimeController controller = null;
            RealTimeController.Create(ref controller, "MyController", sensors, controlStrategy, out controller);
            controller.ConnectAll();

            Console.WriteLine("Enter to staret monitoring.");
            Console.ReadLine();

            controller.Monitor(true, new[] { Messaging.MessageCode.Joints/*, Messaging.MessageCode.Torsor */});

            Console.WriteLine("Enter to terminate");
            Console.ReadLine();
            controller.Terminate();

            Console.WriteLine("Enter to exit");
            Console.ReadLine();

        }

    }
}