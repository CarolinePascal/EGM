using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using abb.egm;


///////////////////////////////////////////////////////////////////////////////////////////////////////////


namespace EgmSmallTest
{
    class Program
    {
        // listen on this port for inbound messages
        public static int IpPortNumber = 6510;

        // usefull Paths
        public static string filePath = "C:/Users/carol/Desktop/Stage_1/plot.py";
        public static string filePath2 = "C:/Users/carol/Desktop/Stage_1/plot2.py";
        public static string python = "c:/users/carol/appdata/local/programs/python/python36-32/python.exe";

        // number of positions to plot
        public static int Plot = 1500;

        static void Main(string[] args)
        {
            Sensor s = new Sensor();
            s.Start();

            Console.WriteLine("Press any key to Exit");
            Console.ReadLine();
        }
    }

    public class Sensor
    {
        private Thread _sensorThread = null;
        private UdpClient _udpServer = null;
        private bool _exitThread = false;

        private uint _seqNumber = 0;    //for sumscheck

        //Ordered position
        private float _x;
        private float _y;
        private float _z;

        //Robot's position
        private int _robotX;
        private int _robotY;
        private int _robotZ;


        // Set the measurements for the square and initialize the start points
        public Sensor()
        {
            //Initial position specified by the fine robtarget in Rapid
            _x = 400;
            _y = -300;
            _z = 100;

            _robotX = 0;
            _robotY = 0;
            _robotZ = 0;
        }


        //Initializes the header of the plotting python script
        public StringBuilder PlotInit()
        {
            var text = new StringBuilder();

            text.AppendLine("#Plot Python Essai");
            text.AppendLine("import matplotlib.pyplot as plt");
            text.AppendLine("from mpl_toolkits.mplot3d import Axes3D");
            text.AppendLine(" ");
            text.AppendLine("X=[]");
            text.AppendLine("Y=[]");
            text.AppendLine("Z=[]");
            text.AppendLine(" ");
            text.AppendLine("fig=plt.figure()");
            text.AppendLine("ax=fig.gca(projection='3d')");
            text.AppendLine(" ");

            return (text);

        }

        public StringBuilder TorqueInit()
        {
            var text = new StringBuilder();

            text.AppendLine("#Plot Python Essai");
            text.AppendLine("import matplotlib.pyplot as plt");
            text.AppendLine(" ");
            text.AppendLine("T1=[]");
            text.AppendLine("T2=[]");
            text.AppendLine("T3=[]");
            text.AppendLine("T4=[]");
            text.AppendLine("T5=[]");
            text.AppendLine("T6=[]");
            text.AppendLine("T=[]");
            text.AppendLine(" ");
            text.AppendLine("fig=plt.figure()");
            text.AppendLine(" ");

            return (text);
        }

        //Writes the recorded positions values in the python script
        public void Fill(string x, string y, string z, StringBuilder text)
        {
            var newLine = string.Format("X.append({0})", x);
            text.AppendLine(newLine);
            newLine = string.Format("Y.append({0})", y);
            text.AppendLine(newLine);
            newLine = string.Format("Z.append({0})", z);
            text.AppendLine(newLine);
        }

        public void FillTorque(string t1, string t2, string t3, string t4, string t5, string t6, string t, StringBuilder text)
        {
            var newLine = string.Format("T1.append({0})", t1);
            text.AppendLine(newLine);
            newLine = string.Format("T2.append({0})", t2);
            text.AppendLine(newLine);
            newLine = string.Format("T3.append({0})", t3);
            text.AppendLine(newLine);
            newLine = string.Format("T4.append({0})", t4);
            text.AppendLine(newLine);
            newLine = string.Format("T5.append({0})", t5);
            text.AppendLine(newLine);
            newLine = string.Format("T6.append({0})", t6);
            text.AppendLine(newLine);
            newLine = string.Format("T.append({0})", t);
            text.AppendLine(newLine);
        }



        //Writes the plotting instructions in the python script and executes it
        public void Plot(StringBuilder text)
        {
            text.AppendLine("ax.plot(X,Y,Z)");
            text.AppendLine("plt.axis('equal')");
            text.AppendLine("plt.show()");
            File.WriteAllText(Program.filePath, text.ToString());

            ProcessStartInfo start = new ProcessStartInfo();
            string cmd = Program.python;
            string args = Program.filePath;

            start.FileName = cmd;
            start.Arguments = args;
            start.UseShellExecute = false;// Do not use OS shell
            start.CreateNoWindow = true; // We don't need new window
            start.RedirectStandardOutput = true;// Any output, generated by application will be redirected back
            start.RedirectStandardError = true; // Any error in standard output will be redirected back (for example exceptions)

            Process process = Process.Start(start);
        }

        //Writes the plotting instructions in the python script and executes it
        public void PlotTorque(StringBuilder text)
        {
            text.AppendLine("plt.plot(T,T1,label='Axe 1')");
            text.AppendLine("plt.plot(T,T2,label='Axe 2')");
            text.AppendLine("plt.plot(T,T3,label='Axe 3')");
            text.AppendLine("plt.plot(T,T4,label='Axe 4')");
            text.AppendLine("plt.plot(T,T5,label='Axe 5')");
            text.AppendLine("plt.plot(T,T6,label='Axe 6')");
            text.AppendLine("plt.legend()");
            text.AppendLine("plt.show()");
            File.WriteAllText(Program.filePath2, text.ToString());

            ProcessStartInfo start = new ProcessStartInfo();
            string cmd = Program.python;
            string args = Program.filePath2;

            start.FileName = cmd;
            start.Arguments = args;
            start.UseShellExecute = false;// Do not use OS shell
            start.CreateNoWindow = true; // We don't need new window
            start.RedirectStandardOutput = true;// Any output, generated by application will be redirected back
            start.RedirectStandardError = true; // Any error in standard output will be redirected back (for example exceptions)

            Process process = Process.Start(start);
        }

        public void SensorThread()
        {
            // create an udp client and listen on any address and the port _ipPortNumber
            _udpServer = new UdpClient(Program.IpPortNumber);
            var remoteEP = new IPEndPoint(IPAddress.Any, Program.IpPortNumber);
            Console.WriteLine("Connexion avec le client - Adresse IP : " + remoteEP.Address + " - Port : " + remoteEP.Port);// The IPAdress is created by the program

            StringBuilder text = PlotInit();
            StringBuilder text2 = TorqueInit();

            int counter = 0;  //Number of positions recoded and ploted
            int counter2 = 0;
            int timer = 0;

            int n = 0;
            double corr = 1;

            while (_exitThread == false && timer <= Program.Plot)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape) break;  //Stops the program if esc key is pressed

                // get the message from robot
                var data = _udpServer.Receive(ref remoteEP);                

                if (data != null)
                {
                    timer++;

                    if (data.Length >= 300)
                    {
                        counter++;

                        // de-serialize inbound message from robot using Google Protocol Buffer
                        EgmRobot robot = EgmRobot.CreateBuilder().MergeFrom(data).Build();

                        // Get the robots X-position
                        _robotX = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.X));
                        // Get the robots Y-position
                        _robotY = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.Y));
                        // Get the robots Z-position
                        _robotZ = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.Z));

                        Fill(_robotX.ToString(), _robotY.ToString(), _robotZ.ToString(), text);

                        // create a new outbound sensor message
                        EgmSensor.Builder sensor = EgmSensor.CreateBuilder();
                        CreateSensorMessage(sensor, ref n, ref corr);

                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            EgmSensor sensorMessage = sensor.Build();
                            sensorMessage.WriteTo(memoryStream);

                            // send the UDP message to the robot
                            int bytesSent = _udpServer.Send(memoryStream.ToArray(),
                                                           (int)memoryStream.Length, remoteEP);
                            if (bytesSent < 0)
                            {
                                Console.WriteLine("Error send to robot");
                            }
                        }
                    }

                    else
                    {
                        counter2++;
                        string returnData = Encoding.ASCII.GetString(data);
                        Console.WriteLine(returnData);
                        String[] substrings = returnData.Split(' ');

                        FillTorque(substrings[1], substrings[2], substrings[3], substrings[4], substrings[5], substrings[6], substrings[0], text2);
                    }
                }
            }

            Plot(text);
            PlotTorque(text2);
            Console.WriteLine(counter);
            Console.WriteLine(counter2);
            Console.WriteLine(timer);
            this.Stop();
            Console.WriteLine("Press any key to exit");
            while (!Console.KeyAvailable)
            {
            }
        }

        // Display message from robot
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


        //////////////////////////////////////////////////////////////////////////


        // Create a sensor message to send to the robot
        void CreateSensorMessage(EgmSensor.Builder sensor, ref int n, ref double corr)
        {
            // create a header
            EgmHeader.Builder hdr = new EgmHeader.Builder();
            hdr.SetSeqno(_seqNumber++)
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

            if (n < 750)
            {
                n++;
            }

            else if (n >= 750)
            {
                n = 0;
                corr = -corr;
            }

            _x = 400;
            _y = SimpleCorrection(ref _y, (float)corr);
            _z = 100 + 100 * (float)Math.Cos(n / 25);


            pc.SetX(_x)
              .SetY(_y)
              .SetZ(_z);

            pq.SetU0(0.0)   //To check, but seems to be vertical
              .SetU1(0.0)
              .SetU2(0.0)
              .SetU3(0.0);

            pos.SetPos(pc)  //pose definition
                .SetOrient(pq);

            // bind pos object to planned
            planned.SetCartesian(pos);
            // bind planned to sensor object
            sensor.SetPlanned(planned);

            return;
        }

        // Start a thread to listen on inbound messages
        public void Start()
        {
            _sensorThread = new Thread(new ThreadStart(SensorThread));  //Multitasking <3
            _sensorThread.Start();
        }

        // Stop and exit thread
        public void Stop()
        {
            _exitThread = true;
            _sensorThread.Abort();  //Stops the thread
        }

        public float SimpleCorrection(ref float target, float corr)
        {
            target = target + corr;
            return (target);
        }
    }
}
