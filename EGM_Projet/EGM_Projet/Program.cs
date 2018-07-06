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
        // usefull Paths
        public static string filePath = "C:/Users/carol/Desktop/Stage_1/plot.py";
        public static string python = "c:/users/carol/appdata/local/programs/python/python36-32/python.exe";

        public static int Plot = 5000;  //Number of EGM messages to send (multiply by 4 to get the duration of the simulation in ms)

        public static ManualResetEvent[] events = new ManualResetEvent[3];    //Events which signals the end of the threads


        static void Main(string[] args)
        {
            Sensor s = new Sensor();

            while (s._reboot)
            {
                //Set events on false
                for (int i = 0; i < events.Length; i++)
                {
                    events[i] = new ManualResetEvent(false);
                }

                //Starting the threads
                s.StartI();
                s.Start();
                s.StartT();

                //Wait until all threads are closed
                WaitHandle.WaitAll(events);
            }

        }
    }


    ///////////////////////////////////////////////////////////////////////////////////////////////////////////


    public class Sensor
    {

        //////////////////////////// PROPRIETIES ////////////////////////////

        //Threads

        private Thread _sensorThread = null;    //EGM thread
        private Thread _torqueThread = null;    //Torque feedback thread
        private Thread _inputThread = null;     //Orders reception thread


        //UDP Clients - Same IP, one port by client

        private UdpClient _udpServer = null;
        private UdpClient _udpServerT = null;
        private UdpClient _udpServerI = null;

        private int IpPortNumber = 6510;
        private int IpPortNumberT = 5000;
        private int IpPortNumberI = 4000;


        //.txt file for the Python script - plotting purposes

        private StringBuilder text;


        //Closing and reboot conditions of the main EGM thread

        private bool _exitThread;
        public bool _reboot;


        //Suspend and closing conditions for torque and reception threads

        public bool Abort;
        public bool AbortI;
        public bool Wait;
        public bool WaitI;


        //EGM UDP messages sumcheck

        private uint _seqNumber = 0;    


        //Buffer position

        private float _x;
        private float _y;
        private float _z;


        //Robot's position

        private int _robotX;
        private int _robotY;
        private int _robotZ;


        //Ordered position

        private double inputdata_x;
        private double inputdata_y;
        private double inputdata_z;


        // Default constructor 

        public Sensor()
        {
            //Robot's initial position
            _x = 400;
            _y = -300;
            _z = 100;

            _robotX = 0;
            _robotY = 0;
            _robotZ = 0;

            inputdata_x = 0;
            inputdata_y = 0;
            inputdata_z = 0;

            _exitThread = false;
            _reboot = true;

            Abort = false;
            Wait = false;

            AbortI = false;
            WaitI = false;

            text = new StringBuilder();
        }


        //////////////////////////////// PLOT ////////////////////////////////


        //Initializes the header of the plotting python script
        public StringBuilder PlotInit()
        {
            var text = new StringBuilder();

            text.AppendLine("#Plot Python ");
            text.AppendLine("import matplotlib.pyplot as plt");
            text.AppendLine("from mpl_toolkits.mplot3d import Axes3D");

            text.AppendLine(" ");
            text.AppendLine("X=[]");
            text.AppendLine("Y=[]");
            text.AppendLine("Z=[]");
            text.AppendLine("TEGM=[]");
            text.AppendLine(" ");
            text.AppendLine("T1=[]");
            text.AppendLine("T2=[]");
            text.AppendLine("T3=[]");
            text.AppendLine("T4=[]");
            text.AppendLine("T5=[]");
            text.AppendLine("T6=[]");
            text.AppendLine("T=[]");
            text.AppendLine(" ");

            return (text);

        }

        //Writes the recorded positions values in the python script
        public void Fill(string x, string y, string z, string t, StringBuilder text)
        {
            var newLine = string.Format("X.append({0})", x);
            text.AppendLine(newLine);
            newLine = string.Format("Y.append({0})", y);
            text.AppendLine(newLine);
            newLine = string.Format("Z.append({0})", z);
            text.AppendLine(newLine);
            newLine = string.Format("TEGM.append({0})", t);
            text.AppendLine(newLine);
        }

        //Writes the recorded torques values in the python script
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
            text.AppendLine(" ");
            text.AppendLine("fig=plt.figure()");
            text.AppendLine("ax=fig.gca(projection='3d')");

            text.AppendLine("ax.plot(X,Y,Z)");

            text.AppendLine("ax.set_xlabel('X')");
            text.AppendLine("ax.set_ylabel('Y')");
            text.AppendLine("ax.set_zlabel('Z')");

            text.AppendLine("plt.axis('equal')");
            text.AppendLine("plt.legend(loc='best')");

            text.AppendLine(" ");

            text.AppendLine("fig2=plt.figure()");

            text.AppendLine("plt.plot(TEGM,X,label='x')");
            text.AppendLine("plt.plot(TEGM,Y,label='y')");
            text.AppendLine("plt.plot(TEGM,Z,label='z')");

            text.AppendLine("plt.xlabel('Temps en ms')");
            text.AppendLine("plt.ylabel('Axes')");

            text.AppendLine("plt.legend(loc='best')");

            text.AppendLine(" ");

            text.AppendLine("fig3=plt.figure()");

            text.AppendLine("plt.plot(T,T1,'--',label='Axe 1')");
            text.AppendLine("plt.plot(T,T2,'--',label='Axe 2')");
            text.AppendLine("plt.plot(T,T3,'--',label='Axe 3')");
            text.AppendLine("plt.plot(T,T4,'--',label='Axe 4')");
            text.AppendLine("plt.plot(T,T5,'--',label='Axe 5')");
            text.AppendLine("plt.plot(T,T6,'--',label='Axe 6')");

            text.AppendLine("plt.xlabel('Temps en ms')");
            text.AppendLine("plt.ylabel('Couples en Nm')");

            text.AppendLine("plt.legend(loc='best')");

            text.AppendLine(" ");

            text.AppendLine("if(T==TEGM):");  //Synchronisation condition

            text.AppendLine("   fig4=plt.figure()");
            text.AppendLine("   plt.plot(Y,T1,label='Axe 1')");

            text.AppendLine("   plt.xlabel('Déplacement en Y en mm')");
            text.AppendLine("   plt.ylabel('Couple axe 1 en Nm')");

            text.AppendLine("   plt.legend(loc='best')");

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


        //////////////////////////// COMMUNICATION ////////////////////////////


        //Reception of the ordered positions
        public void InputThread()
        {
            //Creation of the UDP Client
            _udpServerI = new UdpClient(IpPortNumberI);
            var remoteEP = new IPEndPoint(IPAddress.Any, IpPortNumberI);
            Console.WriteLine("Connexion avec le serveur Grasshopper - Adresse IP : " + remoteEP.Address + " - Port : " + remoteEP.Port);

            int timer = 0;

            while (!AbortI)
            {
                //Momentary interruption

                do
                {
                    if (AbortI) { break; }
                } while (WaitI);

                var data = _udpServerI.Receive(ref remoteEP);

                if (data != null)
                {
                    timer++;

                    //Parsing

                    string returnData = Encoding.ASCII.GetString(data);
                    returnData = returnData.Replace('.', ',');
                    String[] substrings = returnData.Split(' ');
                    
                    inputdata_x = double.Parse(substrings[0]);
                    inputdata_y = double.Parse(substrings[1]);
                    inputdata_z = double.Parse(substrings[2]);
                }
            }
            Console.WriteLine("Nombre de messages reçus :" + timer);
            _udpServerI.Close();
        }

        //Reception of the torques feedback
        public void TorqueThread()
        {
            //Creation of the UDP Client
            _udpServerT = new UdpClient(IpPortNumberT);
            var remoteEP = new IPEndPoint(IPAddress.Any, IpPortNumberT);
            Console.WriteLine("Connexion avec le serveur Rapid - Adresse IP : " + remoteEP.Address + " - Port : " + remoteEP.Port);
            Console.WriteLine(" ");
            Console.WriteLine("==> Start the Rapid procedure <==");

            int timer = 0;
            int check = 1;  //So at the first iteration check is different of 0 

            while (!Abort)
            {
                //Momentary interruption
                do
                {
                    if (Abort) { break; }
                } while (Wait);

                var data = _udpServerT.Receive(ref remoteEP);

                if (data != null)
                {
                    //Parsing

                    string returnData = Encoding.ASCII.GetString(data);
                    String[] substrings = returnData.Split(' ');
                    int temps = Int32.Parse(substrings[0]);

                    //Synchronization with EGM : the messages are recorded each 4 ms (EGM feedbacks are recieved at this frequency), based on the Rapid time.
                    if (temps % 4 == 0 && temps!=check)
                    {
                        check = temps;
                        FillTorque(substrings[1], substrings[2], substrings[3], substrings[4], substrings[5], substrings[6], substrings[0], text);
                        timer++;
                    }  
                }
            }
            Console.WriteLine("Nombre de messages couples :" + timer);
            _udpServerT.Close();
        }

        //Basically, EGM
        public void SensorThread()
        {
            //Creation of the UDP Client
            _udpServer = new UdpClient(IpPortNumber);
            var remoteEP = new IPEndPoint(IPAddress.Any, IpPortNumber);

            Console.WriteLine("Connexion avec le serveur Rapid - Adresse IP : " + remoteEP.Address + " - Port : " + remoteEP.Port);

            int timer = 0;
            int RefTime = 0;    

            while (_exitThread == false && timer <= Program.Plot)
            {
                //Momentary interruption if "ESC" is pressed
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)  //Stops the program if esc key is pressed
                {
                    Wait = true;
                    WaitI = true;

                    ConsoleKey key2 = new ConsoleKey();

                    do
                    {
                        Console.WriteLine("Do you want to quit the procedure ? Yes [Y] No [N]");
                        key2 = Console.ReadKey(true).Key;

                    } while (key2 != ConsoleKey.Y && key2 != ConsoleKey.N);

                    if (key2 == ConsoleKey.Y)
                    {
                        Abort = true;
                        AbortI = true;
                        break;
                    }

                    else if (key2 == ConsoleKey.N)
                    {
                        Wait = false;
                        WaitI = false;
                        continue;
                    }
                }

                var data = _udpServer.Receive(ref remoteEP);

                if (data != null)
                {
                    timer++;

                    //Displays processing
                    if (timer % (Program.Plot / 20) == 0)
                    {
                        int taux = timer * 20 / Program.Plot;
                        string chaine = "";
                        for (int i = 0; i < taux; i++)
                        {
                            chaine += ".";
                        }
                        for (int i = taux; i < 21; i++)
                        {
                            chaine += " ";
                        }
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        Console.Write(new String(' ', Console.BufferWidth));
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        Console.WriteLine("Processing : " + chaine + (timer * 100 / Program.Plot).ToString() + "%");
                    }

                    // de-serialize inbound message from robot using Google Protocol Buffer
                    EgmRobot robot = EgmRobot.CreateBuilder().MergeFrom(data).Build();

                    //Set the Rapid time of the first message recieved as the reference for synchronization
                    if (timer == 1)
                    {
                        RefTime = (int)robot.Header.Tm;
                    }

                    // Get the robots X-position
                    _robotX = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.X));
                    // Get the robots Y-position
                    _robotY = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.Y));
                    // Get the robots Z-position
                    _robotZ = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.Z));

                    Fill(_robotX.ToString(), _robotY.ToString(), _robotZ.ToString(),((int)robot.Header.Tm-RefTime).ToString(), text);

                    // create a new outbound sensor message
                    EgmSensor.Builder sensor = EgmSensor.CreateBuilder();
                    CreateSensorMessage(sensor, inputdata_x, inputdata_y, inputdata_z);

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
            }
            _udpServer.Close();

            Abort = true;
            AbortI = true;

            System.Threading.Thread.Sleep(200);

            Plot(text);

            Console.WriteLine("Nombre de messages EGM :" + timer);
            
            //Rebooting or aborting options
            bool flag = true;
            ConsoleKey key = new ConsoleKey();

            do
            {
                Console.WriteLine("Reboot the procedure ? Yes [Y] No [N]");
                key = Console.ReadKey(true).Key;

            } while (key != ConsoleKey.Y && key != ConsoleKey.N);

            if (key == ConsoleKey.N) { flag = false; }
            else if (key == ConsoleKey.Y) { }
            
            Stop(flag);
            StopT(flag);
            StopI(flag);
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


        /////////////////////////// PATH PLANNING ///////////////////////////


        // Create a sensor message to send to the robot -> TO DO : Orientation modification !
        void CreateSensorMessage(EgmSensor.Builder sensor, double x, double y, double z) 
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

            //New cartesian positions
            _y = -300 + (float)(y * 5);
            _z = 100 + (float)(z * 5);
            _x = 400 + (float)(x * 5);

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

        //Describes the shape of a heart 
        public float heart_y(double n)
        {
            return (float)(16*5 * Math.Pow(Math.Sin(n), 3));
        }

        public float heart_z(double n)
        {
            return (float)(13*5 * Math.Cos(n) - 5*5 * Math.Cos(2 * n) - 2*5 * Math.Cos(3 * n) - 5*Math.Cos(4 * n));
        }


        /////////////////////////// STOP & START ///////////////////////////


        //Thread starting procedures
        public void Start()
        {
            text.Clear();
            text = PlotInit();
            _reboot = false;
            _exitThread = false;
            _sensorThread = new Thread(new ThreadStart(SensorThread));  //Multitasking ;
            _sensorThread.Start();
        }

        public void StartT()
        {
            Abort = false;
            _torqueThread = new Thread(new ThreadStart(TorqueThread));  //Multitasking 
            _torqueThread.Start();
        }

        public void StartI()
        {
            AbortI = false;
            _inputThread = new Thread(new ThreadStart(InputThread));
            _inputThread.Start();
        }

        //Thread stopping procedures
        public void Stop(bool reboot)
        {
            _exitThread = true;
            _reboot = reboot;
            Program.events[0].Set();
            if (!reboot)
            {
                _sensorThread.Abort();
            }
        }

        public void StopT(bool reboot)
        {
            Abort = !reboot;
            Program.events[1].Set();
            if (!reboot)
            {
                _torqueThread.Abort();

            }
        }

        public void StopI(bool reboot)
        {
            AbortI = !reboot;
            Program.events[1].Set();
            if(!reboot)
            {
                _inputThread.Abort();
            }
        }
    }
}
