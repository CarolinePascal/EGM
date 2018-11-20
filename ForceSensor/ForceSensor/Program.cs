using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using abb.egm;

using System.IO;

namespace ForceSensor
{
    class Program
    {
        static void Main(string[] args)
        {
            double[] results = new double[6];
            double theta = 0;
            double theta2 = 0;

            SensorServer sensorServer = new SensorServer("192.168.1.205",50);
            //sensorServer.Start();

            Thread sensorThread;
            sensorThread = new Thread(new ThreadStart(sensorServer.Main));
            sensorThread.Start();

            var remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"),6510);
            UdpClient client = new UdpClient(remoteEP);

            var csv = new StringBuilder();

            int n = 0;

            while (!Console.KeyAvailable)
            {
                var data = client.Receive(ref remoteEP);
                n++;

                if (data != null && n%25==0)
                {
                    EgmRobot robot = EgmRobot.CreateBuilder().MergeFrom(data).Build();
                    theta = Convert.ToDouble((robot.FeedBack.Joints.JointsList[5]));
                    theta2 = Convert.ToDouble((robot.FeedBack.Joints.JointsList[2]));

                    results = sensorServer.GetStateLast();
                    Console.WriteLine(theta2.ToString()+" "+theta.ToString() + " " + results[1].ToString());

                    var newline = string.Format("{0};{1};{2};{3};{4};{5};{6};{7}", theta, theta2, results[0].ToString(), results[1].ToString(), results[2].ToString(), results[3].ToString(), results[4].ToString(), results[5].ToString());
                    csv.AppendLine(newline);
                }
            }

            File.WriteAllText("C:/Users/Nahkriin/Desktop/test.csv", csv.ToString());

            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
            sensorThread.Abort();
            sensorServer.Stop();
        }
    }
}
