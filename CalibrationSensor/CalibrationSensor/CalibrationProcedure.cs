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
    class CalibrationProcedure
    {
        /// <summary>
        /// EGM OnlineController - Needs to be monitoring the MechanismStates of the robot
        /// </summary>
        private OnlineController _egm;

        /// <summary>
        /// Force sensor - Needs to be monitoring the TorsorSensorState of the force sensor
        /// </summary>
        private OnlineController _forceSensor;

        /// <summary>
        /// CalibrationProcedure constructor
        /// </summary>
        /// <param name="EGMController">EGM Onclinecontroller - monitoring MechanismStates</param>
        /// <param name="forceSensorController">Force sensor Onlinecontroller - monitoring TorsorSensorStates</param>
        public CalibrationProcedure(OnlineController EGMController, OnlineController forceSensorController)
        {
            _egm = EGMController;
            _forceSensor = forceSensorController;
        }

        /// <summary>
        /// Launches the calibration procedure
        /// </summary>
        /// <returns></returns>
        public async Task Procedure()
        {
            //Calibration positions
            Joints[] calibrationPositions = new Joints[] { new Joints(0, 0, 0, 0, 0, 0), new Joints(0, 0, 0, 0, 0, 90), new Joints(0, 0, 0, 0, 90, 0) };

            //Initializations 
            List<Torsor> measures = new List<Torsor>();
            List<Torsor> buffer = new List<Torsor>();

            Console.WriteLine("Lancement de la calibration - Verifier que tout est en place et appuyer sur entree");
            Console.ReadLine();

            foreach (Joints joints in calibrationPositions)
            {
                //Getting current joints values
                Joints currentJoints = ((Sensoring.SensorData.Mechanism.EgmMecanismState)_egm.SensorState).Joints;

                //Motion command
                while (CheckPositon(joints, currentJoints, 0.1) != true)
                {
                    EgmBuilder.Joint(_egm, Smoothed(joints, currentJoints, 30).Values);
                    currentJoints = ((Sensoring.SensorData.Mechanism.EgmMecanismState)_egm.SensorState).Joints;
                }

                //Stabilization delay
                await Task.Delay(5000);

                //Getting the forces and torques measurements
                for (int i = 0; i < 10; i++)
                {
                    buffer.Add((Torsor)_forceSensor.SensorState.Value);
                    await Task.Delay(100);
                }

                measures.Add(TorsorAverage(buffer));
            }


            Console.WriteLine("Mesures terminées - Calculs en cours");

            double g = 9.81; ;
            double[] calibrationData = CalibrationAnalysis(measures);

            Console.WriteLine("Calculs terminés - Les valeurs de calibration sont :");

            double p = calibrationData[0] / g;

            Console.WriteLine("Poids : " + p.ToString());

            double d = Math.Atan(calibrationData[2] / calibrationData[1]);

            Console.WriteLine("Dephasage : " + d.ToString());

            double x = calibrationData[6] / calibrationData[0];
            double y = calibrationData[7] / calibrationData[0];
            double z = calibrationData[8] / calibrationData[0];

            Console.WriteLine("Position du centre de gravité : [" + x.ToString() + "," + y.ToString() + "," + z.ToString() + "]");

            Console.WriteLine("Offset du capteur : [" + calibrationData[3].ToString() + "," + calibrationData[4].ToString() + "," + calibrationData[5].ToString() + "," + calibrationData[9].ToString() + "," + calibrationData[10].ToString() + "," + calibrationData[11].ToString() + "]");
        }

        /// <summary>
        /// Method which computes the average of a list of torsors
        /// </summary>
        /// <param name="list">List of Torsor</param>
        /// <returns></returns>
        private static Torsor TorsorAverage(List<Torsor> list)
        {
            double Fx = 0;
            double Fy = 0;
            double Fz = 0;
            double Tx = 0;
            double Ty = 0;
            double Tz = 0;

            foreach (Torsor torsor in list)
            {
                Fx += torsor.TX;
                Fy += torsor.TY;
                Fz += torsor.TZ;
                Tx += torsor.RX;
                Ty += torsor.RY;
                Tz += torsor.RZ;
            }

            double size = list.Count();

            Torsor result = new Torsor(Fx / size, Fy / size, Fz / size, Tx / size, Ty / size, Tz / size);

            return (result);
        }

        /// <summary>
        /// Method which proceeds the calibation calculus
        /// </summary>
        /// <param name="measures">Calibration measures as a List of Torsors</param>
        /// <returns></returns>
        private static double[] CalibrationAnalysis(List<Torsor> measures)
        {
            //mg\cos(\theta)
            double[] Tmgc = new double[] { measures[0].TX - measures[2].TX, measures[2].TY - measures[1].TY };
            //mg\sin(\theta) 
            double[] Tmgs = new double[] { measures[2].TX - measures[1].TX, measures[2].TY - measures[0].TY };

            for (int i = 0; i < Tmgc.Length; i++)
            {
                Console.WriteLine("mgcos" + i.ToString() + " : " + Tmgc[i].ToString());
                Console.WriteLine("mgsin" + i.ToString() + " : " + Tmgs[i].ToString());

            }

            Console.WriteLine("Continue ?");
            Console.ReadLine();

            double mgc = Tmgc.Average();
            double mgs = Tmgs.Average();

            double mg = Math.Sqrt(mgc * mgc + mgs * mgs);
            double c = mgc / mg;
            double s = mgs / mg;

            double[] Tfx = new double[] { measures[0].TX - mgc, measures[1].TX + mgs, measures[2].TX };
            double[] Tfy = new double[] { measures[0].TY + mgs, measures[1].TY + mgc, measures[2].TY };
            double[] Tfz = new double[] { measures[0].TZ, measures[1].TZ, measures[2].TZ - mg };

            for (int i = 0; i < Tfx.Length; i++)
            {
                Console.WriteLine("fx" + i.ToString() + " : " + Tfx[i].ToString());
                Console.WriteLine("fy" + i.ToString() + " : " + Tfy[i].ToString());
                Console.WriteLine("fz" + i.ToString() + " : " + Tfz[i].ToString());

            }

            Console.WriteLine("Continue ?");
            Console.ReadLine();

            double fx = Tfx.Average();
            double fy = Tfy.Average();
            double fz = Tfz.Average();

            double[] Txmg = new double[] { c * (measures[2].RZ - measures[1].RZ) + s * (measures[2].RZ - measures[0].RZ), (c * (measures[1].RY - measures[2].RY) + s * (measures[0].RY - measures[2].RY)) / (c + s) };
            double[] Tymg = new double[] { c * (measures[2].RZ - measures[0].RZ) + s * (measures[1].RZ - measures[2].RZ), (c * (measures[0].RX - measures[2].RX) + s * (measures[1].RX - measures[2].RX)) / (c + s) };
            double[] Tzmg = new double[] { (measures[0].RX - measures[1].RX) / (s - c), (measures[0].RY - measures[1].RY) / (s + c) };

            for (int i = 0; i < Txmg.Length; i++)
            {
                Console.WriteLine("xmg" + i.ToString() + " : " + Txmg[i].ToString());
                Console.WriteLine("ymg" + i.ToString() + " : " + Tymg[i].ToString());
                Console.WriteLine("zmg" + i.ToString() + " : " + Tzmg[i].ToString());

            }

            Console.WriteLine("Continue ?");
            Console.ReadLine();

            double xmg = Txmg.Average();
            double ymg = Tymg.Average();
            double zmg = Tzmg.Average();


            double[] Tmx = new double[] { measures[0].RX - zmg * s, measures[1].RX - zmg * c, measures[2].RX - ymg };
            double[] Tmy = new double[] { measures[0].RY - zmg * c, measures[1].RY + zmg * s, measures[2].RY + xmg };
            double[] Tmz = new double[] { measures[2].RZ, measures[0].RZ + xmg * s + ymg * c, measures[1].RZ + xmg * c - ymg * s };

            for (int i = 0; i < Tmx.Length; i++)
            {
                Console.WriteLine("mx" + i.ToString() + " : " + Tmx[i].ToString());
                Console.WriteLine("my" + i.ToString() + " : " + Tmy[i].ToString());
                Console.WriteLine("mz" + i.ToString() + " : " + Tmz[i].ToString());

            }

            Console.WriteLine("Continue ?");
            Console.ReadLine();

            double mx = Tmx.Average();
            double my = Tmy.Average();
            double mz = Tmz.Average();

            return (new double[] { mg, c, s, fx, fy, fz, xmg, ymg, zmg, mx, my, mz });
        }



        /// <summary>
        /// Method used to check wether the target is reached or not with respect to a given tolerance
        /// </summary>
        /// <param name="orderedJoints">Target to be reached</param>
        /// <param name="currentJoints">Current position</param>
        /// <param name="tolerance">Measure tolerance</param>
        /// <returns></returns>
        private static bool CheckPositon(Joints orderedJoints, Joints currentJoints, double tolerance)
        {
            bool flag = true;
            for (int i = 0; i < currentJoints.Values.Length; i++)
            {
                if (currentJoints.Values[i] < orderedJoints.Values[i] - tolerance || currentJoints.Values[i] > orderedJoints.Values[i] + tolerance)
                {
                    flag = false;
                }
            }
            return (flag);
        }

        /// <summary>
        /// Method which returns a smoothed joint command according to a maximum joint step
        /// </summary>
        /// <param name="orderedJoints">Joints to be reached</param>
        /// <param name="currentJoints">Current Joints</param>
        /// <param name="maxStep">Maximum joint step in degrees</param>
        /// <returns></returns>
        private static Joints Smoothed(Joints orderedJoints, Joints currentJoints, double maxStep)
        {
            Joints joints = new Joints();
            for (int i = 0; i < orderedJoints.Values.Length; i++)
            {
                if (orderedJoints.Values[i] - currentJoints.Values[i] > maxStep)
                {
                    joints.Values[i] = currentJoints.Values[i] + maxStep;
                }
                else if (orderedJoints.Values[i] - currentJoints.Values[i] < -maxStep)
                {
                    joints.Values[i] = currentJoints.Values[i] - maxStep;
                }
                else
                {
                    joints.Values[i] = orderedJoints.Values[i];
                }
            }
            return (joints);
        }
    }
}
