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
        static async Task Main(string[] args)
        {
            //Signal acquisition freuquency
            double sfreq = 32;

            //Filter initialization
            Filtering.Filter<TorsorState> filter = null;

            UdpClient Uclient = new UdpClient(4444);
            IPEndPoint remote = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444);
            IPEndPoint remote2 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2000);

            //Sart session
            var client = new Client();

            //Sensor emulating server
            GenericServer server = new GenericServer("Test", IPAddress.Parse("127.0.0.1"), 5555, 8888);
            await server.Run();

            //Sensor
            OnlineController sensor = null;
            OnlineController.CreateGenericSensor(ref sensor, "Sensor", "127.0.0.1", 8888, 5555, out sensor);

            sensor.IsFiltering = true;
            sensor.Buffering = true;
            sensor.Monitor(true, true, MessageCode.String, MessageCode.TorsorSensorState);


            int[] window = new int[] {1,3,5,7,9,11};
            //Filter identifier
            string str = "BUT";

            //Starting - Restarting messages
            var datagram = Encoding.ASCII.GetBytes("1");
            var datagram2 = Encoding.ASCII.GetBytes("0");
        
            //Start the emulated sensor
            SendData(server, Uclient, remote);
            await Task.Delay(5000);

            foreach (int f in window)
            {
                //Filter to benchmark
                filter = new AverageFilter(f);
                
                sensor.Filter = filter;
                sensor.BufferMaximumMessageCount = f;

                Console.WriteLine("C'est parti");
                
                //Start-restart grasshopper broadcast
                Uclient.Send(datagram2, datagram2.Length, remote2);
                await Task.Delay(1000);
                Uclient.Send(datagram, datagram.Length, remote2);

                //Name of the .csv file
                str += f.ToString();
                Console.WriteLine(str);

                //Read and save vlaues
                await Read(sensor,str);

                str = str.Remove(3, 1);
                Console.WriteLine("Done!");
            }


        }

        /// <summary>
        /// Sending method for the sensor emulating server - Recieves external data and broadcast it with a working message type
        /// </summary>
        /// <param name="server">Emulating GenericServer</param>
        /// <param name="client">Recieving UDP client</param>
        /// <param name="remote">Remote IP Endpoint</param>
        /// <returns></returns>
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


        /// <summary>
        /// Parsing method - converts external data into a Torsor
        /// </summary>
        /// <param name="data">Recieved data as a string</param>
        /// <returns>Parsed data as a Torsor</returns>
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

        /// <summary>
        /// Reading and Saving methods - Reads the sensor state and records it in a .csv file
        /// </summary>
        /// <param name="controller">Sensor as a OnlineController</param>
        /// <param name="str">Ptath of the .csv file as a string</param>
        /// <returns></returns>
        private static async Task Read(OnlineController controller,string str)
        {
            var csv = new StringBuilder();
            int n = 0;

            while (n<150)
            {
                n++;
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
            File.WriteAllText("C:\\Users\\Nahkriin\\Desktop\\Cesure1\\Benchmark_Filtres\\"+str+".csv", csv.ToString());
        }
    }
}    
