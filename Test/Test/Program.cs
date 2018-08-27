using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HAL.ENPC.Communication;
using HAL.ENPC.Control;
using HAL.ENPC.Filtering;
using HAL.ENPC.Messaging;
using HAL.ENPC.Messaging.Server.ATI;
using HAL.ENPC.Messaging.Server.Generic;
using HAL.ENPC.Messaging.Server.ABB;
using HAL.ENPC.Sensoring;
using HAL.ENPC.Sensoring.SensorData;
using UnityEngine;

namespace HAL.ENPC.Debug
{
    class Program
    {
        static void Main(string[] args)
        {

           // Declaration(sensors)
           List<RealTimeController> sensors = new List<RealTimeController>();

            Sensor egmSensor = null;
            Sensor.CreateEgm(ref egmSensor, "EgmSensor", "127.0.0.1", 6510, 6510, out egmSensor);
            sensors.Add(egmSensor);

            //ForceSensorServer forceSensorServer = new ForceSensorServer("ForceSensor", IPAddress.Parse("192.168.1.1"), 49152, 49152, filter: new AverageFilter(10));
            //Sensor forceSensor = new Sensor(new Identifier("ForceSensor"), null, forceSensorServer, null);
            //sensors.Add(forceSensor);

            //Declaration(real time controller)
            ControlBuilder.Create("egm", (int)ControlableType.Joint, out Control.ABB.Egm controlStrategy);
            RealTimeController controller = null;
            RealTimeController.Create(ref controller, "MyController", sensors, controlStrategy, out controller);
            controller.BufferMaximumMessageCount = 6000;

            // Monitoring initialization

            RealTimeController.Monitor(controller, new[] {(int)MessageCode.Joints, (int)MessageCode.Torsor}, true, true);

            //Console.WriteLine(message.ToString());

            // Control initialization
            //RealTimeController.RunControl(controller, true, egmSensor, (int)MessageCode.Joints, true);                                                                                                                                                                

            Console.WriteLine("Test run?");
            Console.ReadLine();
            Thread.Sleep(10000);
            Console.WriteLine("Edit bug report.");
            Analysis.SaveSensorData(controller, true, "C:\\Users\\FormationRobotAdmin\\Documents\\CP019-EGM\\test20m.csv", out _, out _);
        }

        static async Task RunControlTest(RealTimeController controller)
        {
            int index = 1;
            controller.BufferMaximumMessageCount = 5;
            controller.Buffering = true;

            Torsor torsorSmooth = Torsor.Default;

            while (true)
            {
                controller.ReceiveBufferQueue.TryDequeue(out IMessage message);
                if(message is TorsorMessage torsorMessage)
                {
                    torsorSmooth = torsorMessage.Payload; //Tester SmoothDamp de Mathf
                }

                Joints joints = CreateJointFromTorsor(torsorSmooth);

                controller.CommandMessageQueue.Add(controller.ControlStrategy(joint));
            }
        }

    }
}