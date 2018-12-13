using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HAL.Catalog.Items;
using HAL.Control;
using HAL.Display;
using HAL.ENPC.Control;
using HAL.ENPC.Control.Builder;
using HAL.ENPC.Control.Builder.ABB;
using HAL.ENPC.Messaging;
using HAL.ENPC.Messaging.Server.Android;
using HAL.ENPC.Messaging.Server.Generic;
using HAL.ENPC.Sensoring;
using HAL.ENPC.Sensoring.SensorData;
using HAL.ENPC.Sensoring.SensorData.Mechanism;
using HAL.Objects;
using HAL.Objects.Mechanisms;
using HAL.Objects.Parts;
using HAL.Procedures;
using HAL.Runtime;
using HAL.Spatial;
using HAL.Spatial.Curves;
using HAL.Spatial.Intersections;
using HAL.Units.Length;

using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

using System.Diagnostics;

namespace HAL.ENPC.Debug
{
    class Program
    {
        //public static OnlineController _controller;
        //Sampling à 30Hz

        static async Task Main(string[] args)
        {


            double sfreq = 32;
            Filtering.Filter<TorsorState> filter = new ButterworthFilter(100, 500, 1);

            UdpClient Uclient = new UdpClient(4444);
            IPEndPoint remote = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444);
            IPEndPoint remote2 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000);

            //start session
            var client = new Client();
            //load robot
            //Mechanism mechanism = LoadMechanism(client).Result;

            GenericServer server = new GenericServer("Test", IPAddress.Parse("127.0.0.1"), 5555, 8888);
            await server.Run();

            OnlineController sensor = null;
            OnlineController.CreateGenericSensor(ref sensor, "Sensor", "127.0.0.1", 8888, 5555, out sensor);
            

            //OnlineController controller = null;
            //OnlineController.CreateGeneric(ref controller, "Controller", "127.0.0.1", 7777, 7777, null, null, null, new List<OnlineController>() { sensor }, out controller);
            sensor.IsFiltering = true;
            sensor.Buffering = true;
            sensor.Monitor(true, true, MessageCode.String, MessageCode.TorsorSensorState);
            //((GenericServer)controller.Server).Filter = filter;

            int[] window = new int[] {1,3,5,7,9,11};
            string str = "BUT";
            var datagram = Encoding.ASCII.GetBytes("1");
            var datagram2 = Encoding.ASCII.GetBytes("0");

            SendData(server, Uclient, remote);
            await Task.Delay(5000);

            foreach (int f in window)
            {
                //if(f<=2)
                //{
                //    filter = new StaviskyGolayFilter(f, f);
                //}

                filter = new ButterworthFilter(sfreq/(double)f,sfreq,2) ;
                
                
                    
                sensor.Filter = filter;
                sensor.IsFiltering = true;
                sensor.BufferMaximumMessageCount = 4;

                Console.WriteLine("C'est tipaaaaar");
                //Console.ReadLine();

                Uclient.Send(datagram2, datagram2.Length, remote2);
                await Task.Delay(1000);
                Uclient.Send(datagram, datagram.Length, remote2);

                
                str += f.ToString();
                Console.WriteLine(str);

                await Read(sensor,str);

                str = str.Remove(3, 1);
                Console.WriteLine("Done!");
                //Console.ReadLine();
            }


        }

        private static async Task SendData(GenericServer server, UdpClient client, IPEndPoint remote)
        {            
            while (true)
            {
                byte[] raw = client.Receive(ref remote);
                string data = Encoding.ASCII.GetString(raw);

                await server.SendMessage(new TorsorStateMessage(new TorsorState(Parse(data),false)));
                await Task.Delay(20);
            }
        }

        private static Torsor Parse(string data)
        {
            string[] words = data.Split(';');
            Torsor torsor = new Torsor();

            
            for (int i=0;i<words.Length;i++)
            {
                torsor.Values[i] = double.Parse(words[i], System.Globalization.CultureInfo.InvariantCulture);
            }

            return (torsor);
        }

        private static async Task Read(OnlineController controller,string str)
        {
            var csv = new StringBuilder();
            int n = 0;

            //Stopwatch timer = new Stopwatch();
            //timer.Start();

            while (n<150)
            {
                n++;
                //controller.ReceiveBufferQueue.TryDequeue(out IMessage message);
                var state = controller.SensorState?.Value;
                if(state!=null)
                {
                    Console.WriteLine(controller.SensorState.Value);

                    var newLine = string.Format(((Torsor)controller.SensorState.Value).RX.ToString());
                    csv.AppendLine(newLine);
                }
                Console.WriteLine(controller.SensorState);

                await Task.Delay(20);
            }
            Console.WriteLine(n);
            //File.WriteAllText("C:\\Users\\Nahkriin\\Desktop\\Cesure1\\Benchmark_Filtres\\"+str+".csv", csv.ToString());
        }

        private static async Task ReadAndroidSensor(OnlineController controller)
        {
            while (true)
            {
                controller.ReceiveBufferQueue.TryDequeue(out IMessage message);
                Console.WriteLine(message);
            }
        }

        private static async Task<Mechanism> LoadMechanism(Client client)
        {
            Mechanism mechanism = null;
            Console.WriteLine("type a key word to refine mechanism search : ");
            string keyWord = Console.ReadLine();
            foreach (MechanismCatalogItem item in client.Catalogs.Mechanisms.Items.ToList().Where(i => string.IsNullOrEmpty(keyWord) || i.Title.Contains(keyWord)))
            {
                if (PromptConfirmation(item.ToString()))
                {
                    mechanism = client.Catalogs.Mechanisms.Retrieve(item);
                    Session.Current.ObjectGraph.AddEdge(new Connection(Session.Current.ObjectGraph.Root as Reference, mechanism.Base) { IsDefaultRootConnection = true });
                    break;
                }
            }
            return mechanism;
        }

        private static async Task Monitor(OnlineController controller)
        {
            MessageCode[] messageCodes = { MessageCode.MecanismStateEgm };
            controller.Buffering = true;
            controller.Monitor(true, true, messageCodes);
            while (true)
            {
                //Console.WriteLine(controller.SensorState);
                await Task.Delay(20);
            }
        }

        /// <summary> Initialize a virtual controller.</summary>
        /// <param name="mechanism"></param>
        /// <param name="virtualController"></param>
        /// <returns></returns>
        private static OnlineController InitializeEgmController(Mechanism mechanism, bool virtualController, params OnlineController[] subcontrollers)
        {
            //create egm controller
            List<IControllableObject> mechanisms = new List<IControllableObject> { mechanism };
            OnlineController instance = null;
            List<Procedure> procedures = new List<Procedure>() { new Procedure() };
            OnlineController.CreateEGM(ref instance, "EgmController", virtualController ? "127.0.0.1" : "192.168.125.1", 6510, 6510, mechanisms, subcontrollers.ToList(), procedures, null, out OnlineController controller);
            return controller;
        }

        /// <summary> Add displacement actual position. </summary>
        /// <param name="vector"></param>
        private static void UpdateTcpPosition(CartesianControl control, Vector3D vector)
        {
            Console.WriteLine(vector);
            IMecanismStateEnpc state = control.Controller.SensorState is IMecanismStateEnpc mecanismState ? mecanismState : null;
            if (state is null) return;
            QuaternionFrame tcp = new QuaternionFrame(new Vector3D(state.EndPoint.Position.Values), new Numerics.Quaternion(state.EndPoint.Rotation.Values));
            control.ObjectiveFrame = new QuaternionFrame(tcp.Position.Add(in vector), tcp.Rotation);
            control.MoveTowards(out _, out _);
        }

        /// <summary> Add displacement actual position. </summary>
        /// <param name="control"></param>
        private static void UpdateJointPositions(OnlineController controller, Joints joints)
        {
            Console.WriteLine(joints);
            EgmBuilder.Joint(controller, joints.Values);
        }

        private static bool PromptConfirmation(string confirmText)
        {
            Console.Write(confirmText + " [y/n] : ");
            ConsoleKey response = Console.ReadKey(false).Key;
            Console.WriteLine();
            return (response == ConsoleKey.Y);
        }

        /// <summary>Control up and down to move the robot by "step". Increase step with arrow right and diminish it with arrow left.  </summary>
        /// <returns>Completed task.</returns>
        //private static async Task ArrowKeyControl()
        //{
        //    int step = 1;
        //    while (true)
        //    {
        //        var ch = Console.ReadKey(false).Key;
        //        IMecanismStateEnpc state = _controller.SensorState is IMecanismStateEnpc mecanismState ? mecanismState : null;
        //        if (state is null) { await Task.Delay(10); continue; }
        //        Joints actualJoint = state.Joints;
        //        switch (ch)
        //        {
        //            case ConsoleKey.Escape:
        //                return;
        //            case ConsoleKey.UpArrow:
        //                UpdateJointPositions(_controller, actualJoint.Add(new Joints(0, 0, step, step, step, 0)));
        //                break;
        //            case ConsoleKey.DownArrow:
        //                UpdateJointPositions(_controller, actualJoint.Add(new Joints(0, 0, -step, -step, -step, 0)));
        //                break;
        //            case ConsoleKey.LeftArrow:
        //                step += 1;
        //                break;
        //            case ConsoleKey.RightArrow:
        //                step -= 1;
        //                break;
        //        }
        //        Console.WriteLine($"step is {step}");
        //    }
        //}
    }
}    
