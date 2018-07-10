using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using abb.egm;
using System.IO;
using System.Threading;

namespace EGM_Projet
{
    public class EGM_Server : Server   //Also stands for main server 
    {
        List<Server> servers;    //List of all the servers connected to the EGM main sevrer

        int countMax;

        //Feedback position

        float _robotX;
        float _robotY;
        float _robotZ;

        public EGM_Server() : base()
        {
            servers = null;

            countMax = 0;

            _robotX = 0;
            _robotY = 0;
            _robotZ = 0;

            Port = 6510;
        }

        /// <summary>
        /// Describt fuunction
        /// </summary>
        /// <param name="liste">This do that</param>
        /// <param name="n"></param>
        /// <remarks></remarks>
        public EGM_Server(List<Server> liste, int n) : base()
        {
            servers = liste;

            countMax = n;

            _robotX = 0;
            _robotY = 0;
            _robotZ = 0;

            Port = 6510;
        }

       
        public override int Main()
        {
            int n = 0;
            var remoteEP = new IPEndPoint(IPAddress.Any, Port);

            while (exit == false && n<countMax)
            {

                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    foreach (Server server in servers)
                    {
                        server.wait = true;
                    }

                    ConsoleKey key2 = new ConsoleKey();
                    do
                    {
                        Console.WriteLine("Do you want to quit the procedure ? Yes [Y] No [N]");
                        key2 = Console.ReadKey(true).Key;

                    } while (key2 != ConsoleKey.Y && key2 != ConsoleKey.N);

                    if (key2 == ConsoleKey.Y)
                    {
                        foreach (Server server in servers)
                        {
                            exit=true;
                        }
                        break;

                    }

                    else if (key2 == ConsoleKey.N)
                    {
                        foreach (Server server in servers)
                        {
                            server.wait = false;
                        }
                        continue;
                    }
                }

                var data = udpClient.Receive(ref remoteEP);

                if (data != null)
                {
                    n++;
                    EgmRobot robot = EgmRobot.CreateBuilder().MergeFrom(data).Build();
                    //DisplayInboundMessage(robot);

                    if (n == 1)
                    {
                        refTime = (int)robot.Header.Tm;
                    }

                    _robotX = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.X));
                    _robotY = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.Y));
                    _robotZ = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.Z));

                    EgmSensor.Builder sensor = EgmSensor.CreateBuilder();

                    CreateSensorMessage(sensor, servers);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        EgmSensor sensorMessage = sensor.Build();
                        sensorMessage.WriteTo(memoryStream);

                        // send the UDP message to the robot
                        int bytesSent = udpClient.Send(memoryStream.ToArray(),
                                                        (int)memoryStream.Length, remoteEP);
                        
                        if (bytesSent < 0)
                        {
                            Console.WriteLine("Error send to robot");
                        }
                    }
                }
            }

            foreach(Server server in servers)
            { 
                server.wait = true;
            }


            ConsoleKey key = new ConsoleKey();
            do
            {
                Console.WriteLine("Reboot the procedure ? Yes [Y] No [N]");
                key = Console.ReadKey(true).Key;

            } while (key != ConsoleKey.Y && key != ConsoleKey.N);

            if (key == ConsoleKey.N)
            {
                foreach (Server server in servers)
                {
                    server.reboot = false;
                    reboot = false;
                }
            }
            
            else if (key == ConsoleKey.Y)
            {
                foreach (Server server in servers)
                {
                    server.reboot = true;
                    reboot = true;
                }
            }

            foreach (Server server in servers)
            {
                server.exit = true;
                server.wait = false;
            }

            return (n);            
        }

        public override void Counter(int n)
        {
            Console.WriteLine("Messages EGM : " + n);
        }

        public void CreateSensorMessage(EgmSensor.Builder sensor, List<Server> servers)
        {
            // create a header
            EgmHeader.Builder hdr = new EgmHeader.Builder();
            hdr.SetSeqno((uint)seqNumber++)
               //Timestamp in milliseconds (can be used for monitoring delays)
               .SetTm((uint)DateTime.Now.Ticks)
               //Sent by sensor, MSGTYPE_DATA if sent from robot controller
               .SetMtype(EgmHeader.Types.MessageType.MSGTYPE_CORRECTION);   //What are the main differencies between those messages types ?

            sensor.SetHeader(hdr);

            // create some sensor data
            EgmPlanned.Builder planned = new EgmPlanned.Builder();
            EgmPose.Builder pos = new EgmPose.Builder();
            EgmQuaternion.Builder pq = new EgmQuaternion.Builder();
            EgmCartesian.Builder pc = new EgmCartesian.Builder();

            float _x = 0;
            float _y = 0;
            float _z = 0;

            foreach(Server server in servers)
            {
                if(server is Order_Server)
                {
                    Order_Server Oserver = (Order_Server)server;
                    _x += Oserver.x;
                    _y += Oserver.y;
                    _z += Oserver.z;
                }
            }

            pc.SetX(_x)
              .SetY(_y)
              .SetZ(_z);

            pq.SetU0(0.0)
              .SetU1(0.0)
              .SetU2(0.0)
              .SetU3(0.0);

            pos.SetPos(pc)
                .SetOrient(pq);

            // bind pos object to planned
            planned.SetCartesian(pos);
            // bind planned to sensor object
            sensor.SetPlanned(planned);

            return;
        }

        int DisplayInboundMessage(EgmRobot robot)
        {
            int time = 0;
            if (robot.HasHeader && robot.Header.HasSeqno && robot.Header.HasTm)
            {
                time = (int)robot.Header.Tm;
                Console.WriteLine("Seq={0} tm={1}", robot.Header.Seqno.ToString(), robot.Header.Tm.ToString()); //Uncomment to display
            }
            else
            {
                Console.WriteLine("No header in robot message");  //Uncomment to display
            }
            return (time);
        }
    }
}
