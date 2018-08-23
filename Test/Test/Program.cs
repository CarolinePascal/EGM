using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HAL.ENPC.Communication;
using HAL.ENPC.Control;
using HAL.ENPC.Messaging;
using HAL.ENPC.Messaging.Server.Generic;
using HAL.ENPC.Messaging.Server.ABB;
using HAL.ENPC.Sensoring;
using HAL.ENPC.Sensoring.SensorData;
using HAL.ENPC.Messaging.Server.ATI;
using HAL.ENPC.Filtering;

namespace HAL.ENPC.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            // Declaration (sensors)
            List<RealTimeController> sensors = new List<RealTimeController>();
            //Sensor egmSensor = null;
            //Sensor.CreateEgm(ref egmSensor, "EgmSensor", "127.0.0.1", 6510, 6510, out egmSensor);
            //sensors.Add(egmSensor);

            Filter<Torsor> filter = new FilterTorsor() { FilterSize = 20 };
            ForceSensorServer forceSensorServer = new ForceSensorServer("ForceSensor", IPAddress.Parse("192.168.1.1"), 49152, 49152, filter: filter);
            Sensor forceSensor = new Sensor(new Identifier("ForceSensor"), null, forceSensorServer, null);
            sensors.Add(forceSensor);

            // Declaration (real time controller)
            ControlBuilder.Create("egm", (int)ControlableType.Joint, out Control.ABB.Egm controlStrategy);
            RealTimeController controller = null;
            RealTimeController.Create(ref controller, "MyController", sensors, controlStrategy, out controller);
            controller.BufferMaximumMessageCount = 6000;

            // Monitoring initialization
            RealTimeController.Monitor(controller, new[] { (int)MessageCode.Torsor, (int)MessageCode.Joints, (int)MessageCode.String, (int)MessageCode.Double }, true, true);

            // Control initialization
            //RealTimeController.RunControl(controller, true, egmSensor, (int)MessageCode.Joints, true);

            Console.WriteLine("Test run?");
            Console.ReadLine();

            Console.WriteLine("Edit bug report.");
            Console.ReadLine();
            Analysis.SaveSensorData(controller, true, "C:\\Users\\FormationRobotAdmin\\Documents\\CP019-EGM\\test.csv", out _, out _);

            Console.WriteLine("Enter to exit.");
            Console.ReadLine();
        }
    }
}