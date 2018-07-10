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
        /// <summary>
        /// List of all the Input_Servers connected to the EGM "Main" Server
        /// </summary>
        List<Server> servers;    

        /// <summary>
        /// Maximum number of plotted positions
        /// </summary>
        int countMax;

        // Feebacked robot's positions
        float _robotX;
        float _robotY;
        float _robotZ;

        /// <summary>
        /// Default constructor of a EGM_Server instance -The UDP Port is necessarely 6510
        /// </summary>
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
        /// Constructor of a EGM_Server instance with the list of the connected servers and the plot number
        /// </summary>
        /// <param name="liste">List of all the Input_Servers connected to the EGM "Main" Server</param>
        /// <param name="n"> Maximum number of plotted positions</param>
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

        /// <summary>
        /// Main loop for EGM_Server :
        /// - Key Reading for pausing, rebooting and stopping the execution
        /// - Recieving EGM feedback messages from the Motion Control
        /// - Sending EGM order messages to the Motion Control
        /// </summary>
        /// <param name="n">Number of recieved EGM messages</param>
        public override void Main(out int n)
        {
            n = 0;
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

                    Program.plot.Fill(_robotX.ToString(), _robotY.ToString(), _robotZ.ToString(), ((int)robot.Header.Tm-refTime).ToString());

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

            Program.plot.Trace();

            ConsoleKey key = new ConsoleKey();
            do
            {
                Console.WriteLine("Reboot the procedure (Close all the python figures before rebooting) ? Yes [Y] No [N]");
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
        }

        /// <summary>
        /// Displays the number n of recieved EGM messages
        /// </summary>
        /// <param name="n">Number of recieved EGM messages</param>
        public override void Counter(int n)
        {
            Console.WriteLine("Messages EGM : " + n);
        }

        /// <summary>
        /// Creates an EGM message to send to the Motion Control with the ordered robot position
        /// </summary>
        /// <param name="sensor">abb.egm type created in the Main loop</param>
        /// <param name="servers">List of all the Input_Servers connected to the EGM "Main" Server - Especially the servers providing the position to reach !</param>
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

        /// <summary>
        /// Displays the number and the time of a feedback message recieved from the Motion Control
        /// </summary>
        /// <param name="robot">abb.egm type containing the feebback message</param>
        /// <returns></returns>
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
