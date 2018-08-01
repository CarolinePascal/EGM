using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using HAL.Communications;
using HAL.Communications.Protocols;
using HAL.ENPC.Messaging.Server;
using HAL.ENPC.Messaging.Server.ATI;
using HAL.ENPC.Sensoring;

namespace HAL.ENPC.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Debug.Listeners.Clear();
            System.Diagnostics.Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            UdpServer.ConfirmationDelay = -1;

            SensorController instance = null;
            List<ISensor> sensors = new List<ISensor>();
            Sensor egm = new Sensor(new Identifier("SensorReceiver"), new List<Protocol> { new Electrical() }, new EgmServer(IPAddress.Parse("127.0.0.1"), 6510, 6510));
            Sensor forceSensor = new Sensor(new Identifier("SensorReceiver"), new List<Protocol> { new Electrical() }, new ForceSensorServer(IPAddress.Parse("192.168.1.1"), 49152, 49152));
            sensors.AddRange(new[] { /*sensorReceiver,*/ forceSensor });
            Communication.SensorServer(ref instance, "Name", sensors, forceSensor, false, false, out SensorController controller);
            Console.WriteLine("Sensor started.");
            Communication.Run(controller, true);
            Test(controller);
            Console.ReadLine();
        }

        static void Test(SensorController controller)
        {
            Sender(controller, 1);
        }

        static async Task Sender(SensorController controller, int sleepTime)
        {

            controller.StartSensors();
            controller.MonitorSensor();
            if (controller.SenderServer is EgmServer egmServer) egmServer.JointControl(true);

            int i = 1;
            while (true)
            {
                Console.WriteLine($"Sending{i}");
                Thread.Sleep(sleepTime);
                //i = -i;
            }
        }






        static void Test(string alias, GenericServer server, int timing)
        {
            Sensor sensor = new Sensor(new Identifier(alias), new List<Protocol> { new Electrical() }, server);
            sensor.SensorServer.Initialize();
            Sender(sensor, timing);
        }



        static async Task Sender(Sensor sensor, int sleepTime)
        {
            int i = 0;
            while (sensor.SensorServer.IsInitialized)
            {
                Console.WriteLine($"Sending{i}");
                await sensor.SensorServer.Enqueue(new StringMessage(MessageCode.String, "Something -------------------------------------------------"), false, null, false);
                Thread.Sleep(sleepTime);
                i++;
            }
        }
    }
}