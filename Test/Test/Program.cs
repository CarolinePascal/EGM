using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HAL.ENPC.Control;
using HAL.ENPC.Messaging;
using HAL.ENPC.Messaging.Server.ATI;
using HAL.ENPC.Sensoring;
using HAL.ENPC.Sensoring.SensorData;
using System.Diagnostics;
using HAL.ENPC.Communication;

namespace HAL.ENPC.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            Filtering.Filter<Torsor> filter = new RIIFilter(600, 2000);
            Console.ReadLine();

            //// Declaration(sensors)

            //List<RealTimeController> sensors = new List<RealTimeController>();

            ////EGM Sensor - Robot feedback
            //Sensor egmSensor = null;
            //Sensor.CreateEgm(ref egmSensor, "EgmSensor", "127.0.0.1", 6510, 6510, out egmSensor);
            //sensors.Add(egmSensor);

            ////Force Sensor
            //ForceSensorServer forceSensorServer = new ForceSensorServer("ForceSensor", IPAddress.Parse("192.168.1.205"), 49152, 49152, filter: new RIIFilter(100, 1000));
            //Sensor forceSensor = new Sensor(new Identifier("ForceSensor"), null, forceSensorServer, null);
            //sensors.Add(forceSensor);

            ////Declaration(real time controller)
            //ControlBuilder.Create("egm", (int)ControlableType.Joint, out Control.ABB.Egm controlStrategy);
            //RealTimeController controller = null;
            //RealTimeController.Create(ref controller, "MyController", sensors, controlStrategy, out controller);

            ////Buffer settings for the record
            //controller.BufferMaximumMessageCount = 6000;

            //// Monitoring initialization

            //RealTimeController.Monitor(controller, new[] { (int)MessageCode.Joints, (int)MessageCode.Torsor }, true, true);

            //// Control initialization
            ////RealTimeController.RunControl(controller, true, egmSensor, (int)MessageCode.Joints, true);

            //Thread.Sleep(8000);
            //forceSensorServer.Bias();
            //Console.WriteLine("Starting");
            //Thread.Sleep(6000);

            //Analysis.SaveSensorData(controller, true, "C:\\Users\\FormationRobotAdmin\\Documents\\CP019-EGM\\test.csv", out _, out _);

            ////Task.Run(() => RunControlTest(controller));

            //Console.WriteLine("enter to exit");
        }

        /// <summary>
        /// Asynchronous record and control task
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        static async Task RunControlTest(RealTimeController controller)
        {
            controller.BufferMaximumMessageCount = 5;
            controller.Buffering = true;

            Torsor torsorSmooth = Torsor.Default;
            Joints actualJoints = Joints.Default;

            //Control messages timer
            Stopwatch stopWatch = new Stopwatch();
            //Command timer
            Stopwatch stopWatch2 = new Stopwatch();

            //Starting timers
            stopWatch.Start();
            stopWatch2.Start();            

            while (stopWatch2.ElapsedMilliseconds<=100000)
            {
                //Get the last recieved message
                controller.ReceiveBufferQueue.TryDequeue(out IMessage message);

                if (message is TorsorMessage torsorMessage)
                {
                    torsorSmooth = torsorMessage.Payload; 
                }
                else if (message is JointMessage jointMessage)
                {
                    actualJoints = jointMessage.Payload;
                }

                //Send the motion order - control message
                if(stopWatch.ElapsedMilliseconds>=500)
                {
                    Joints joints = CreateJointFromTorsor(torsorSmooth, actualJoints);
                    ControlBuilder.Joint(controller, new double[] { joints.J1, joints.J2, joints.J3, joints.J4, joints.J5, joints.J6 });
                    stopWatch.Restart();
                }

            }
        }

        /// <summary>
        /// Motion command creation method
        /// </summary>
        /// <param name="torsor">Force sensor measurement as a Torsor</param>
        /// <param name="refJoints">Acutal robot position as a Joints</param>
        /// <returns></returns>
        public static Joints CreateJointFromTorsor(Torsor torsor, Joints refJoints)
        {
            //Joints target
            Joints targetJoints = new Joints(2*torsor.TX, 2*torsor.TY, 2*torsor.TZ, 2*torsor.RX, 2*torsor.RY, 2*torsor.RZ);
      
            //Smoothing of the motion
            double j1 = Smooth(refJoints.J1, targetJoints.J1, 0.5);
            double j2 = Smooth(refJoints.J2, targetJoints.J2, 0.5);
            double j3 = Smooth(refJoints.J3, targetJoints.J3, 0.5);
            double j4 = Smooth(refJoints.J4, targetJoints.J4, 0.75);
            double j5 = Smooth(refJoints.J5, targetJoints.J5, 0.75);
            double j6 = Smooth(refJoints.J6, targetJoints.J6, 0.75);

            Joints commandJoints = new Joints(j1, j2, j3, j4, j5, j6);
            Console.WriteLine("Commande : " + commandJoints.ToString());

            return (commandJoints);
        }

        /// <summary>
        /// Smoothing method
        /// </summary>
        /// <param name="start">Starting point</param>
        /// <param name="target">Target</param>
        /// <param name="rate">Smoothing rate beetween 0 and 1</param>
        /// <returns></returns>
        public static double Smooth (double start, double target, double rate)
        {
            return (start + (target - start) * rate);
        }
    }
}