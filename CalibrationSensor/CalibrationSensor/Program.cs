using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HAL.Catalog.Items;
using HAL.Control;
using HAL.ENPC.Control;
using HAL.ENPC.Control.Builder.ABB;
using HAL.ENPC.Messaging;
using HAL.ENPC.Sensoring.SensorData;
using HAL.Objects;
using HAL.Objects.Mechanisms;
using HAL.Objects.Parts;
using HAL.Procedures;
using HAL.Runtime;
using HAL.Spatial;
using HAL.Spatial.Curves;
using HAL.Spatial.Intersections;
using HAL.ENPC.Messaging.Server.Generic;

using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace HAL.ENPC.Debug
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Measures buffer size
            int N = 10;

            //Start Session
            var client = new Client();

            //Load robot - necessary for EGM control and monitoring
            Mechanism mechanism = LoadMechanism(client).Result;

            //Server emulating the force sensor
            GenericServer server = new GenericServer("Test", IPAddress.Parse("127.0.0.1"), 5555, 8888);
            await server.Run();

            SendData(server);
            await Task.Delay(1000);

            //Force sensor Online controller for the amulated server
            OnlineController sensor = null;
            OnlineController.CreateGenericSensor(ref sensor, "Sensor", "127.0.0.1", 8888, 5555, out sensor);
            sensor.Buffering = true;
            sensor.Monitor(true, true, MessageCode.TorsorSensorState);

            //Create ATI sensor
            OnlineController ati = null;
            OnlineController.CreateAtiSensor(ati, "Ati", "127.0.0.1", 49152, 49152, out ati);

            await ForceMonitor(ati);

            //Create EGM joints control
            OnlineController controller = InitializeEgmController(mechanism, true);
            controller.RunSynchronizedControl(MessageCode.MecanismStateEgm, true);

            await Monitor(controller);
            await Task.Delay(1000);

            //Launching calibration procedure
            CalibrationProcedure procedure = new CalibrationProcedure(controller, ati);
            await procedure.Procedure();

            Console.WriteLine("Exit");
            Console.ReadLine();
        }

        private static async Task SendData(HAL.ENPC.Messaging.Server.Generic.GenericServer server)
        {
            while (true)
            {
                await server.SendMessage(new TorsorStateMessage(new HAL.ENPC.Sensoring.TorsorState(new Torsor(0,0,0,0,0,1), false)));
                await Task.Delay(20);
            }
        }

        /// <summary>
        /// Pure force control command prototype - Constant stiffness control on forces only 
        /// </summary>
        /// <param name="forceSensor">Force sensor OnlineController</param>
        /// <param name="egmControl">EGM OnlineController</param>
        /// <param name="K">Diagonal items of the stiffness control matrix</param>
        /// <returns></returns>
        private static async Task Commande(OnlineController forceSensor, OnlineController egmControl, double[] K)
        {
            EgmConstraints settings = new EgmConstraints(ControlStrategy.Debug, 0.1, 0.1, 10);
            CartesianControl cartControl = new CartesianControl(settings, egmControl);
            cartControl.Run();

            int n = 0;

            while(n<200)
            {
                n++;
                Torsor torsor = (Torsor)forceSensor.SensorState.Value;
                Vector3D vector = new Vector3D(K[0]*torsor.TX, K[1]*torsor.TY, K[2]*torsor.TZ);

                UpdateTcpPosition(cartControl, vector, null, null);

                await Task.Delay(500);
            }
        }

        /// <summary>
        /// Method which loads a robot mechanism
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Asynchronous task dedicated to the monitoring of an AIT force sensor
        /// </summary>
        /// <param name="atiController">ATI force sensor online controller</param>
        /// <returns></returns>
        private static async Task ForceMonitor(OnlineController atiController)
        {
            atiController.Buffering = true;
            atiController.Monitor(true, true, MessageCode.TorsorSensorState);
        }

        private static async Task Monitor(OnlineController controller)
        {
            controller.Buffering = true;
            controller.Monitor(true, true, MessageCode.MecanismStateEgm);
            //while(true)
            //{
            //    Console.WriteLine(((Sensoring.SensorData.Mechanism.EgmMecanismState)controller.SensorState)?.Joints.ToString());
            //}
        }

        /// <summary> Initialize a virtual EGM controller</summary>
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

        /// <summary>
        /// Adds a displacements to the current position - TO BE FIXED
        /// </summary>
        /// <param name="control">CartesianControll</param>
        /// <param name="vector">Vector3D to be added - X,Y,Z</param>
        /// <param name="mechanism">Robot used as a Mechanism</param>
        /// <param name="calibrationData">Calibration data computed with the CalibrationProcedure - For the sensor frame</param>
        private static void UpdateTcpPosition(CartesianControl control, Vector3D vector, Mechanism mechanism, double[] calibrationData)
        {
            Console.WriteLine(vector);

            var state = (Sensoring.SensorData.Mechanism.EgmMecanismState)control.Controller.SensorState;

            //Calculating the translation in the robot base frame

            IReadOnlyList<Joint> joints = mechanism.Joints;
            Motion.JointPositions positions = new Motion.JointPositions(joints, state.Joints.Values);
            mechanism.Jog(positions, null);

            RotationMatrix endPoint = mechanism.GetActiveEndPointLocation(true).Rotation;
            RotationMatrix sensor = new RotationMatrix(new double[] { calibrationData[1], -calibrationData[2], 0, calibrationData[2], calibrationData[1], 0, 0, 0, 1 });

            RotationMatrix total = sensor.Multiply(endPoint);

            Vector3D trueVector = (total.Inverse()).Multiply(vector);

            var orientation = state.EndPoint;
            
            if (orientation == null)
            {
                //return;
            }
            QuaternionFrame tcp = new QuaternionFrame(new Vector3D(orientation.Position.Values), new Numerics.Quaternion(orientation.Rotation.Values));
            vector = trueVector; //with Correction
            control.ObjectiveFrame = new QuaternionFrame(tcp.Position.Add(in vector), tcp.Rotation);
            Console.WriteLine(control.ObjectiveFrame.Position);
            control.MoveTowards(out _, out _);
        }

        private static bool PromptConfirmation(string confirmText)
        {
            Console.Write(confirmText + " [y/n] : ");
            ConsoleKey response = Console.ReadKey(false).Key;
            Console.WriteLine();
            return (response == ConsoleKey.Y);
        }
    }
}

