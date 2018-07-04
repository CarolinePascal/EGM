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
        public static string filePath2 = "C:/Users/carol/Desktop/Stage_1/plot2.py";
        public static string python = "c:/users/carol/appdata/local/programs/python/python36-32/python.exe";

        public static int Plot = 2000;


        static void Main(string[] args)
        {
            Sensor s = new Sensor();
            while (s._reboot)
            {
                s.Start();
            }
            
        }
    }

    public class Sensor
    {

        //////////////////////////// PROPRIETIES ////////////////////////////


        //private Thread _sensorThread = null;  //In case of Multitasking purposes - unables the stop-restart thing

        private UdpClient _udpServer = null;
        private int IpPortNumber = 6510;

        private bool _exitThread;
        public bool _reboot;

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
            _y = 0;
            _z = 0;

            _robotX = 0;
            _robotY = 0;
            _robotZ = 0;

            _exitThread = false;
            _reboot = true;
        }


        //////////////////////////////// PLOT ////////////////////////////////


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
            text.AppendLine("T=[]");
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
            text.AppendLine("from scipy.interpolate import spline");
            text.AppendLine("import numpy as np");
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
        public void Fill(string x, string y, string z, string t, StringBuilder text)
        {
            var newLine = string.Format("X.append({0})", x);
            text.AppendLine(newLine);
            newLine = string.Format("Y.append({0})", y);
            text.AppendLine(newLine);
            newLine = string.Format("Z.append({0})", z);
            text.AppendLine(newLine);
            newLine = string.Format("T.append({0})", t);
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

            text.AppendLine("fig2=plt.figure()");

            text.AppendLine("plt.plot(T,X,label='x')");
            text.AppendLine("plt.plot(T,Y,label='y')");
            text.AppendLine("plt.plot(T,Z,label='z')");

            text.AppendLine("plt.xlabel('Temps en ms')");
            text.AppendLine("plt.ylabel('Axes')");

            text.AppendLine("plt.legend(loc='best')");
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

        public void PlotTorque(StringBuilder text)
        {
            //text.AppendLine("T_smooth=np.linspace(T[0],T[-1],1000);");
            //text.AppendLine("t1=spline(T,T1,T_smooth)");
            //text.AppendLine("t2=spline(T,T2,T_smooth)");
            //text.AppendLine("t3=spline(T,T3,T_smooth)");
            //text.AppendLine("t4=spline(T,T4,T_smooth)");
            //text.AppendLine("t5=spline(T,T5,T_smooth)");
            //text.AppendLine("t6=spline(T,T6,T_smooth)");

            //text.AppendLine("plt.plot(T_smooth,t1,label='Axe 1')");
            //text.AppendLine("plt.plot(T_smooth,t2,label='Axe 2')");
            //text.AppendLine("plt.plot(T_smooth,t3,label='Axe 3')");
            //text.AppendLine("plt.plot(T_smooth,t4,label='Axe 4')");
            //text.AppendLine("plt.plot(T_smooth,t5,label='Axe 5')");
            //text.AppendLine("plt.plot(T_smooth,t6,label='Axe 6')");

            text.AppendLine("plt.plot(T,T1,'--',label='Axe 1')");
            text.AppendLine("plt.plot(T,T2,'--',label='Axe 2')");
            text.AppendLine("plt.plot(T,T3,'--',label='Axe 3')");
            text.AppendLine("plt.plot(T,T4,'--',label='Axe 4')");
            text.AppendLine("plt.plot(T,T5,'--',label='Axe 5')");
            text.AppendLine("plt.plot(T,T6,'--',label='Axe 6')");

            text.AppendLine("plt.xlabel('Temps en ms')");
            text.AppendLine("plt.ylabel('Couples en Nm')");


            text.AppendLine("plt.legend(loc='best')");
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


        //////////////////////////// COMMUNICATION ////////////////////////////


        public void SensorThread()
        {
            // create an udp client and listen on any address and the port ipPortNumber

            _udpServer = new UdpClient(IpPortNumber);
            var remoteEP = new IPEndPoint(IPAddress.Any, IpPortNumber);
            Console.WriteLine("Connexion avec le client - Adresse IP : " + remoteEP.Address + " - Port : " + remoteEP.Port);// The IPAdress is created by the program
            Console.WriteLine("==> Start the Rapid procedure <==");

            StringBuilder text = PlotInit();
            StringBuilder text2 = TorqueInit();

            //Counters

            int counter = 0;  
            int counter2 = 0;
            int timer = 0;

            int Reftime = 0;

            while (_exitThread == false && counter <= Program.Plot)
            {

                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)  //Stops the program if esc key is pressed
                {

                    ConsoleKey key2 = new ConsoleKey();

                    do
                    {
                        Console.WriteLine("Do you want to quit the procedure ? Yes [Y] No [N]");
                        key2 = Console.ReadKey(true).Key;

                    } while (key2 != ConsoleKey.Y && key2 != ConsoleKey.N);

                    if (key2 == ConsoleKey.Y) { break; }
                    else if (key2 == ConsoleKey.N) { continue; }
                }

                // get the message from robot

                var data = _udpServer.Receive(ref remoteEP);                

                if (data != null)
                {
                    timer++;

                    //Displays processing

                    if (timer % (Program.Plot/20) == 0)
                    {
                        int taux = timer*20 / Program.Plot;
                        string chaine = "";
                        for(int i=0; i<taux; i++)
                        {
                            chaine+= ".";
                        }
                        for(int i=taux;i<21;i++)
                        {
                            chaine += " ";
                        }
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        Console.Write(new String(' ', Console.BufferWidth));
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        Console.WriteLine("Processing : "+chaine+(timer*100/Program.Plot).ToString()+"%");
                    }

                    if (data.Length >= 300) //EGM Feedback data message
                    {
                        counter++;

                        // de-serialize inbound message from robot using Google Protocol Buffer
                        EgmRobot robot = EgmRobot.CreateBuilder().MergeFrom(data).Build();

                        if (counter == 1)
                        {
                            Reftime = (int)robot.Header.Tm;
                        }

                        DisplayInboundMessage(robot);

                        // Get the robots X-position
                        _robotX = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.X));
                        // Get the robots Y-position
                        _robotY = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.Y));
                        // Get the robots Z-position
                        _robotZ = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.Z));

                        Fill(_robotX.ToString(), _robotY.ToString(), _robotZ.ToString(),((int)robot.Header.Tm - Reftime).ToString(), text);

                        // create a new outbound sensor message
                        EgmSensor.Builder sensor = EgmSensor.CreateBuilder();
                        CreateSensorMessage(sensor);

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

                    else //Torque Freedback message
                    {
                        counter2++;
                        string returnData = Encoding.ASCII.GetString(data);
                        Console.WriteLine(returnData);    //Display
                        String[] substrings = returnData.Split(' ');

                        FillTorque(substrings[1], substrings[2], substrings[3], substrings[4], substrings[5], substrings[6], substrings[0], text2);
                    }
                }
            }

            Plot(text);
            PlotTorque(text2);

            Console.WriteLine(" ");
            Console.WriteLine("Nombre de Feedbacks EGM : "+ counter);
            Console.WriteLine("Nombre de Feedbacks couples : " + counter2);
            Console.WriteLine("Nombre de messages totaux :" + timer);

            _udpServer.Close();
            
            bool flag = true;
            ConsoleKey key = new ConsoleKey();

            do
            {
                Console.WriteLine("Reboot the procedure ? Yes [Y] No [N]");
                key = Console.ReadKey(true).Key;

            } while (key != ConsoleKey.Y && key != ConsoleKey.N);

            if (key == ConsoleKey.N) { flag = false; }
            else if (key == ConsoleKey.Y) { }
            
            this.Stop(flag);
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

        double n = 0;

        // Create a sensor message to send to the robot
        void CreateSensorMessage(EgmSensor.Builder sensor)
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

            _x = 400;
            _y = heart_y(n);
            _z = heart_z(n); ;

            n+=2*Math.PI/(Program.Plot-200);

            pc.SetX(_x)
              .SetY(_y)
              .SetZ(_z);

            //Console.WriteLine(_x.ToString()+" "+ _y.ToString()+" "+ _z.ToString());

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

        // Writes a simple offset correction on the path
        public float SimpleCorrection(ref float target, float corr)
        {
            target = target + corr;
            return (target);
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


        // Start a thread to listen on inbound messages
        public void Start()
        {
            //_sensorThread = new Thread(new ThreadStart(SensorThread));  //Multitasking 
            //_sensorThread.Start();
            _reboot = false;
            _exitThread = false;
            SensorThread();
        }

        // Stop and exit thread
        public void Stop(bool reboot)
        {
            _exitThread = true;
            _reboot = reboot;
            //_sensorThread.Abort();  //Stops the thread
        }
    }
}
