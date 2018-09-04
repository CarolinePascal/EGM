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

            // Declaration(sensors)
            List<RealTimeController> sensors = new List<RealTimeController>();

            Sensor egmSensor = null;
            Sensor.CreateEgm(ref egmSensor, "EgmSensor", "127.0.0.1", 6510, 6510, out egmSensor);
            sensors.Add(egmSensor);

            ForceSensorServer forceSensorServer = new ForceSensorServer("ForceSensor", IPAddress.Parse("192.168.1.205"), 49152, 49152, filter: null);
            Sensor forceSensor = new Sensor(new Identifier("ForceSensor"), null, forceSensorServer, null);
            sensors.Add(forceSensor);

            //Declaration(real time controller)
            ControlBuilder.Create("egm", (int)ControlableType.Joint, out Control.ABB.Egm controlStrategy);
            RealTimeController controller = null;
            RealTimeController.Create(ref controller, "MyController", sensors, controlStrategy, out controller);
            controller.BufferMaximumMessageCount = 6000;

            // Monitoring initialization

            RealTimeController.Monitor(controller, new[] { (int)MessageCode.Joints, (int)MessageCode.Torsor }, true, true);

            // Control initialization
            //RealTimeController.RunControl(controller, true, egmSensor, (int)MessageCode.Joints, true);

            //forceSensorServer.Bias();
            Thread.Sleep(8000);
            forceSensorServer.Bias();
            Console.WriteLine("Starting");

            Thread.Sleep(6000);

            Analysis.SaveSensorData(controller, true, "C:\\Users\\FormationRobotAdmin\\Documents\\CP019-EGM\\test.csv", out _, out _);

            //Task.Run(() => RunControlTest(controller));

            Console.WriteLine("enter to exit");
        }

        static async Task RunControlTest(RealTimeController controller)
        {
            controller.BufferMaximumMessageCount = 5;
            controller.Buffering = true;

            Torsor torsorSmooth = Torsor.Default;
            Joints actualJoints = Joints.Default;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Stopwatch stopWatch2 = new Stopwatch();
            stopWatch2.Start();            

            while (stopWatch2.ElapsedMilliseconds<=100000)
            {
                controller.ReceiveBufferQueue.TryDequeue(out IMessage message);
                if (message is TorsorMessage torsorMessage)
                {
                    torsorSmooth = torsorMessage.Payload; //Tester SmoothDamp de Mathf
                }
                else if (message is JointMessage jointMessage)
                {
                    actualJoints = jointMessage.Payload;
                }

                if(stopWatch.ElapsedMilliseconds>=500)
                {
                    Joints joints = CreateJointFromTorsor(torsorSmooth, actualJoints);
                    ControlBuilder.Joint(controller, new double[] { joints.J1, joints.J2, joints.J3, joints.J4, joints.J5, joints.J6 });
                    stopWatch.Restart();
                }

            }
        }

        public static Joints CreateJointFromTorsor(Torsor torsor, Joints refJoints)
        {
            Joints targetJoints = new Joints(2*torsor.TX, 2*torsor.TY, 2*torsor.TZ, 2*torsor.RX, 2*torsor.RY, 2*torsor.RZ);
      
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

        public static double Smooth (double start, double target, double rate)
        {
            return (start + (target - start) * rate);
        }

        // The normal distribution function.
        public static double F(double x, double mu, double sigma)
        {
            return (Math.Exp(-(x - mu)*(x-mu) / (2*sigma*sigma)) / Math.Sqrt(2*Math.PI*sigma*sigma));
        }

        public static double[] GaussianPonderation(int n)
        {
            double[] array = new double[n];
            for (int i=0;i<n;i++)
            {
                array[i] = F(n - 1 - i,0,1);
            }
            return (array);
        }
    }
}