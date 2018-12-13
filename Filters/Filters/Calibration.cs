using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HAL.ENPC.Sensoring;
using HAL.ENPC.Sensoring.SensorData;
using HAL.ENPC;
using HAL.ENPC.Filtering;
using HAL.Numerics;
using HAL.Catalog.Items;
using HAL.Control;

using HAL.Objects;
using HAL.Objects.Mechanisms;
using HAL.Objects.Parts;
using HAL.Procedures;
using HAL.Runtime;
using HAL.Spatial;
using HAL.Spatial.Curves;
using HAL.Spatial.Intersections;


namespace HAL.ENPC.Debug
{
    public class Calibration : HAL.ENPC.Debug.AverageFilter
    {

        private double[] _calibData = new double[12]; //mg c s fx fy fz xmg ymg zmg mx my mz

        private Control.OnlineController _egm;

        private Mechanism _robot;

        public Calibration(int filterSize, double[] resultsCalibration, Control.OnlineController[] controllers, Mechanism mechanism) : base(filterSize,controllers)
        {
            double[] _calibData = resultsCalibration;
            _egm = controllers[0];
            _robot = mechanism;
        }

        public override TorsorState FilterMethod(TorsorState sensorData)
        {
            Torsor measure = base.FilterMethod(sensorData).Value;

            

            if (Security(measure))
            {
                return (new TorsorState(measure, true));
            }

            Joints currentJoints = ((Sensoring.SensorData.Mechanism.EgmMecanismState)_egm.SensorState.Value).Joints;
            Vector3D[] corrGravite = CorrectionGravite(currentJoints);

            //Corrections

            measure.Values[0] -= _calibData[3] + corrGravite[0].Values[0];
            measure.Values[1] -= _calibData[4] + corrGravite[0].Values[1];
            measure.Values[2] -= _calibData[5] + corrGravite[0].Values[2];

            measure.Values[3] -= _calibData[9] + corrGravite[1].Values[0];
            measure.Values[4] -= _calibData[10] + corrGravite[1].Values[1];
            measure.Values[5] -= _calibData[11] + corrGravite[1].Values[2];

            return (new TorsorState(measure, false));
        }

        private Vector3D[] CorrectionGravite(Joints joints)
        {
            //double R13 = Math.Cos(joints.J5) * (Math.Cos(joints.J1) * Math.Cos(joints.J4) * Math.Cos(joints.J2 + joints.J3) - Math.Sin(joints.J1) * Math.Sin(joints.J4));
            //R13 += Math.Sin(joints.J5) + Math.Sin(joints.J2 + joints.J3) * Math.Cos(joints.J1);
            //R13 *= Math.Sin(joints.J6);
            //R13 += Math.Cos(joints.J6) * (Math.Sin(joints.J1) * Math.Cos(joints.J4) + Math.Sin(joints.J4) * Math.Cos(joints.J1) * Math.Cos(joints.J2 + joints.J3));

            //double R23 = Math.Cos(joints.J5) * (Math.Sin(joints.J1) * Math.Cos(joints.J4) * Math.Cos(joints.J2 + joints.J3) + Math.Cos(joints.J1) * Math.Sin(joints.J4));
            //R23-= Math.Sin(joints.J5) + Math.Sin(joints.J2 + joints.J3) * Math.Sin(joints.J1);
            //R23 *= Math.Sin(joints.J6);
            //R23 += Math.Cos(joints.J6) * (Math.Cos(joints.J1) * Math.Cos(joints.J4) - Math.Sin(joints.J4) * Math.Sin(joints.J1) * Math.Cos(joints.J2 + joints.J3));

            //double R33 = -Math.Sin(joints.J6) * (Math.Sin(joints.J5) * Math.Cos(joints.J2 + joints.J3) + Math.Sin(joints.J2 + joints.J3) * Math.Cos(joints.J4) * Math.Cos(joints.J5));
            //R33 -= Math.Cos(joints.J6) * Math.Sin(joints.J4) * Math.Sin(joints.J2 + joints.J3);

            
            Motion.JointPositions positions = new Motion.JointPositions(_robot.Joints, joints.Values);
            _robot.Jog(positions, null);

            RotationMatrix endPoint = _robot.GetActiveEndPointLocation(true).Rotation;

            double R13 = endPoint.Values[2][0];
            double R23 = endPoint.Values[2][1];
            double R33 = endPoint.Values[2][2];

            Vector3D vector = new Vector3D(new double[] { R13 * _calibData[1] + R23 * _calibData[2], -R13 * _calibData[2] + R23 * _calibData[1], R33 });
            Vector3D vectorF = vector.Multiply(-_calibData[0]);

            Vector3D center = new Vector3D(new double[] { _calibData[6], _calibData[7], _calibData[8] });
            Vector3D vectorM = vector.CrossProduct(center);

            return(new Vector3D[] { vectorF, vectorM });

        }

        private bool Security(Torsor torsor)
        {
            bool flag = false;
            if(Math.Abs(torsor.RX)>130 || Math.Abs(torsor.RY) > 130 || Math.Abs(torsor.RZ) > 400 || Math.Abs(torsor.TX) > 10 || Math.Abs(torsor.TY) > 10 || Math.Abs(torsor.TZ) > 10)
            {
                flag = true;
            }
            return (flag);

        }

    }
}
