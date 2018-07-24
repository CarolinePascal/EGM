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
using System.Windows.Media.Media3D; //Add referernce : PresentationCore.dll

namespace EGMProjet
{
    public class EGMServer : Server   //Also stands for main server 
    {
        /// <summary>
        /// List of all the Input_Servers connected to the EGM "Main" Server
        /// </summary>
        private List<Server> _servers;
        
        /// <summary>
        /// Maximum number of plotted positions
        /// </summary>
        private int _countMax { get; set; }

        // Feebacked robot's positions
        private float _robotX;
        private float _robotY;
        private float _robotZ;

        /// <summary>
        /// Default constructor of a EGM_Server instance -The UDP Port is necessarely 6510
        /// </summary>
        public EGMServer(string ipAddress) : base(ipAddress)
        {
            _servers = null;

            _countMax = 0;

            _robotX = 0;
            _robotY = 0;
            _robotZ = 0;

            _port = 6510;
            _ipAddress = ipAddress;
        }

        /// <summary>
        /// Constructor of a EGM_Server instance with the list of the connected servers and the plot number
        /// </summary>
        /// <param name="liste">List of all the Input_Servers connected to the EGM "Main" Server</param>
        /// <param name="n"> Maximum number of plotted positions</param>
        /// <remarks></remarks>
        public EGMServer(List<Server> liste, int n, string ipAddress) : base(ipAddress)
        {
            _servers = liste;

            _countMax = n;

            _robotX = 0;
            _robotY = 0;
            _robotZ = 0;

            _port = 6510;
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
            var remoteEP = new IPEndPoint(IPAddress.Parse(_ipAddress), _port);
            n = 0;
            
            while (Exit == false && n<_countMax)
            {

                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    foreach (Server server in _servers)
                    {
                        server.Wait = true;
                    }

                    ConsoleKey key2 = new ConsoleKey();
                    do
                    {
                        Console.WriteLine("Do you want to quit the procedure ? Yes [Y] No [N]");
                        key2 = Console.ReadKey(true).Key;

                    } while (key2 != ConsoleKey.Y && key2 != ConsoleKey.N);

                    if (key2 == ConsoleKey.Y)
                    {
                        foreach (Server server in _servers)
                        {
                            Exit=true;
                        }
                        break;

                    }

                    else if (key2 == ConsoleKey.N)
                    {
                        foreach (Server server in _servers)
                        {
                            server.Wait = false;
                        }
                        continue;
                    }
                }

                var data = _udpClient.Receive(ref remoteEP);

                if (data != null)
                {
                    n++;
                    EgmRobot robot = EgmRobot.CreateBuilder().MergeFrom(data).Build();
                    //DisplayInboundMessage(robot);

                    if (n == 1)
                    {
                        _refTime = (int)robot.Header.Tm;
                    }

                    _robotX = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.X));
                    _robotY = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.Y));
                    _robotZ = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.Z));

                    double[] eulerangles = new double[] { Convert.ToInt32(robot.FeedBack.Cartesian.Euler.X), Convert.ToInt32(robot.FeedBack.Cartesian.Euler.Y), Convert.ToInt32(robot.FeedBack.Cartesian.Euler.Z) };

                    Program.Plot.Fill(_robotX.ToString(), _robotY.ToString(), _robotZ.ToString(), ((int)robot.Header.Tm-_refTime).ToString());

                    MessageBuilder sensor = new MessageBuilder();

                    sensor.MakeHeader(ref _seqNumber);

                    Vector3D coordinates = new Vector3D(300,0,100+ 40 * Math.Sin(2 * Math.PI * n / 500));

                    EulerAngles euler = new EulerAngles(-180, 0, 0);

                    sensor.MovePose(coordinates, euler);

                    CartesianSpeed speeds = new CartesianSpeed(0, 0, 0, 0, 0, 0);
                    sensor.SpeedCartesian(speeds);
 
                    byte[] message = sensor.Build();

                    _udpClient.Send(message.ToArray(), (int)message.Length, remoteEP);
                }
            }

            foreach(Server server in _servers)
            { 
                server.Wait = true;
            }

            Program.Plot.Trace();

            ConsoleKey key = new ConsoleKey();
            do
            {
                Console.WriteLine("Reboot the procedure (Close all the python figures before rebooting) ? Yes [Y] No [N]");
                key = Console.ReadKey(true).Key;

            } while (key != ConsoleKey.Y && key != ConsoleKey.N);

            if (key == ConsoleKey.N)
            {
                foreach (Server server in _servers)
                {
                    server.Reboot = false;
                    Reboot = false;
                }
            }
            
            else if (key == ConsoleKey.Y)
            {
                foreach (Server server in _servers)
                {
                    server.Reboot = true;
                    Reboot = true;
                }
            }

            foreach (Server server in _servers)
            {
                server.Exit = true;
                server.Wait = false;
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
        /// Displays the number and the time of a feedback message recieved from the Motion Control
        /// </summary>
        /// <param name="robot">abb.egm type containing the feebback message</param>
        /// <returns></returns>
        private void DisplayInboundMessage(EgmRobot robot)
        {
            if (robot.HasHeader && robot.Header.HasSeqno && robot.Header.HasTm)
            {
                Console.WriteLine("Seq={0} tm={1}", robot.Header.Seqno.ToString(), robot.Header.Tm.ToString()); //Uncomment to display
            }
            else
            {
                Console.WriteLine("No header in robot message");  //Uncomment to display
            }
        }

        /// <summary>
        /// Returns the feedbacked robot's position as a string
        /// </summary>
        /// <returns></returns>
        public override string GetState()
        {
            string str = _robotX.ToString() + " " + _robotY.ToString() + " " + _robotZ.ToString();
            Console.WriteLine(str);
            return (str);
        }
    }
}
