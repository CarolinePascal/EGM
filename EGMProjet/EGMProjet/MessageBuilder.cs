using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using abb.egm;
using System.IO;

namespace EGMProjet
{
    public class MessageBuilder
    {
        EgmSensor.Builder _sensor;

        /// <summary>
        /// Default contructor - Creates a new message builder to fill
        /// </summary>
        public MessageBuilder()
        {
            _sensor = EgmSensor.CreateBuilder();
        }



        /// <summary>
        /// Creates and fill the message header with 
        ///  - The identifier of the message
        ///  - The time stamp of the mesage
        ///  - The type of the message
        /// </summary>
        /// <param name="seqNumber">Identifier of the last message sent by the ordering server</param>
        public void MakeHeader(ref int seqNumber)
        {
            EgmHeader.Builder header = new EgmHeader.Builder();

            header.SetSeqno((uint)seqNumber++)
                  .SetTm((uint)DateTime.Now.Ticks)
                  .SetMtype(EgmHeader.Types.MessageType.MSGTYPE_CORRECTION);

            _sensor.SetHeader(header);
        }



        /// <summary>
        /// Builds a translation command
        /// </summary>
        /// <param name="coordinates">3 translation components as an array</param>
        /// <returns></returns>
        public EgmCartesian.Builder CartesianBuilder(double[] coordinates)
        {
            EgmCartesian.Builder cartesian = new EgmCartesian.Builder();

            cartesian.SetX(coordinates[0])
                     .SetY(coordinates[1])
                     .SetZ(coordinates[2]);

            return (cartesian);
        }

        /// <summary>
        /// Builds a rotation command from a quaternion
        /// </summary>
        /// <param name="orientation">4 quaternion components as an array</param>
        /// <returns></returns>
        public EgmQuaternion.Builder QuaterionBuilder(double[] orientation)
        {
            EgmQuaternion.Builder quaternion = new EgmQuaternion.Builder();

            quaternion.SetU0(orientation[0])
                      .SetU1(orientation[1])
                      .SetU2(orientation[2])
                      .SetU3(orientation[3]);

            return (quaternion);
        }

        /// <summary>
        /// Builds a rotation command from a matrix
        /// </summary>
        /// <param name="matrix">3x3 rotation matrix as an array</param>
        /// <returns></returns>
        public EgmQuaternion.Builder QuaterionBuilder(double[,] matrix)
        {
            EgmQuaternion.Builder quaternion = new EgmQuaternion.Builder();

            double qw = 2.0 * Math.Sqrt(1.0 + matrix[0, 0] + matrix[1, 1] + matrix[2, 2]);

            quaternion.SetU0(qw / 4.0)
                      .SetU1((matrix[2, 1] - matrix[1, 2]) / qw)
                      .SetU2((matrix[0, 2] - matrix[2, 0]) / qw)
                      .SetU3((matrix[1, 0] - matrix[0, 1]) / qw);

            return (quaternion);
        }

        /// <summary>
        /// Builds a rotation command from Euler angles
        /// </summary>
        /// <param name="angles">3 Euler angles as an array</param>
        /// <returns></returns>
        public EgmEuler.Builder EulerBuilder(double[] angles)
        {
            EgmEuler.Builder euler = new EgmEuler.Builder();

            euler.SetX(angles[0])
                 .SetY(angles[1])
                 .SetZ(angles[2]);

            return (euler);
        }

        /// <summary>
        /// Builds a cartesian motion command from a translation command and a rotation command
        /// </summary>
        /// <param name="cartesian">Trasnlation command</param>
        /// <param name="quaternion">Rotation command as a quaternion</param>
        /// <returns></returns>
        public EgmPose.Builder PoseBuilder(EgmCartesian.Builder cartesian, EgmQuaternion.Builder quaternion)
        {
            EgmPose.Builder pose = new EgmPose.Builder();

            pose.SetPos(cartesian)
                .SetOrient(quaternion);

            return (pose);

        }

        /// <summary>
        /// Builds a cartesian motion command from a transmation command and a rotation command
        /// </summary>
        /// <param name="cartesian">Translation command</param>
        /// <param name="euler">Rotation command as Euler angles</param>
        /// <returns></returns>
        public EgmPose.Builder PoseBuilder(EgmCartesian.Builder cartesian, EgmEuler.Builder euler)
        {
            EgmPose.Builder pose = new EgmPose.Builder();

            pose.SetPos(cartesian)
                .SetEuler(euler);

            return (pose);
        }

        /// <summary>
        /// Builds a joints (internal or external) motion or speed command 
        /// </summary>
        /// <param name="rotations">6 joints values as an array</param>
        /// <returns></returns>
        public EgmJoints.Builder JointsBuilder(double[] rotations)
        {
            EgmJoints.Builder joints = new EgmJoints.Builder();

            for (int i = 0; i < rotations.Length; i++)
            {
                joints.AddJoints(rotations[i]);
            }

            return (joints);
        }

        /// <summary>
        /// Builds a cartesian speed reference command 
        /// </summary>
        /// <param name="speeds">6 speed reference values in mm/s or degrees/s</param>
        /// <returns></returns>
        public EgmCartesianSpeed.Builder CartesianSpeedBuilder(double[] speeds)
        {
            EgmCartesianSpeed.Builder cartesian = new EgmCartesianSpeed.Builder();

            for (int i = 0; i < speeds.Length; i++)
            {
                cartesian.AddValue(speeds[i]);
            }

            return (cartesian);
        }



        /// <summary>
        /// Fills the message with a cartesian motion command
        /// </summary>
        /// <param name="pose">Cartesian motion command</param>
        public void MovePose(EgmPose.Builder pose)
        {
            EgmPlanned.Builder planned = new EgmPlanned.Builder();

            planned.SetCartesian(pose);
            _sensor.SetPlanned(planned);
        }

        /// <summary>
        /// Fills the message with a cartesian motion command
        /// </summary>
        /// <param name="coordinates">3 translation components as an array</param>
        /// <param name="orientation">4 quaternion components as an array</param>
        public void MovePoseQuaternion(double[] coordinates, double[] orientation)
        {
            EgmPlanned.Builder planned = new EgmPlanned.Builder();

            planned.SetCartesian(PoseBuilder(CartesianBuilder(coordinates), QuaterionBuilder(orientation)));
            _sensor.SetPlanned(planned);
        }

        /// <summary>
        /// Fills the message with a cartesian motion command
        /// </summary>
        /// <param name="coordinates">3 translation components as an array</param>
        /// <param name="orientation">3x3 rotation matrix as an array</param>
        public void MovePoseQuaternion(double[] coordinates, double[,] matrix)
        {
            EgmPlanned.Builder planned = new EgmPlanned.Builder();

            planned.SetCartesian(PoseBuilder(CartesianBuilder(coordinates), QuaterionBuilder(matrix)));
            _sensor.SetPlanned(planned);
        }

        /// <summary>
        /// Fills the message with a cartesian motion command
        /// </summary>
        /// <param name="coordiates">3 translation components as an array</param>
        /// <param name="angles">3 Euler angles as an array</param>
        public void MovePoseEuler(double[] coordinates, double[] angles)
        {
            EgmPlanned.Builder planned = new EgmPlanned.Builder();

            planned.SetCartesian(PoseBuilder(CartesianBuilder(coordinates), EulerBuilder(angles)));
            _sensor.SetPlanned(planned);
        }

        /// <summary>
        /// Fills the message with a joints motion command
        /// </summary>
        /// <param name="joints">Joints motion command</param>
        public void MoveJoints(EgmJoints.Builder joints)
        {
            EgmPlanned.Builder planned = new EgmPlanned.Builder();

            planned.SetJoints(joints);
            _sensor.SetPlanned(planned);
        }

        /// <summary>
        /// Fills the message with a joints motion command
        /// </summary>
        /// <param name="joints">6 joints values as an array</param>
        public void MoveJoints(double[] joints)
        {
            EgmPlanned.Builder planned = new EgmPlanned.Builder();

            planned.SetJoints(JointsBuilder(joints));
            _sensor.SetPlanned(planned);
        }

        /// <summary>
        /// Fills the message with an external joints motion command
        /// </summary>
        /// <param name="pose">External joints motion command</param>
        public void MoveExternalJoints (EgmJoints.Builder extJoints)
        {
            EgmPlanned.Builder planned = new EgmPlanned.Builder();

            planned.SetExternalJoints(extJoints);
            _sensor.SetPlanned(planned);

        }

        /// <summary>
        /// Fills the message with a joints motion command
        /// </summary>
        /// <param name="joints">6 joints values as an array</param>
        public void MoveExternalJoints(double[] joints)
        {
            EgmPlanned.Builder planned = new EgmPlanned.Builder();

            planned.SetExternalJoints(JointsBuilder(joints));
            _sensor.SetPlanned(planned);
        }



        /// <summary>
        /// Fills the message with a cartesian speed command
        /// </summary>
        /// <param name="cartesian">Caretsian speed command</param>
        public void SpeedCartesian(EgmCartesianSpeed.Builder cartesian)
        {
            EgmSpeedRef.Builder speed = new EgmSpeedRef.Builder();

            speed.SetCartesians(cartesian);
            _sensor.SetSpeedRef(speed);
        }

        /// <summary>
        /// Fills the message with a cartesian speed command
        /// </summary>
        /// <param name="speeds">6 speed reference values in mm/s (3 firsts - x,y,z) and degrees/s (3 lasts - Euler angles psi, theta, phi)</param>
        public void SpeedCartesian(double[] speeds)
        {
            EgmSpeedRef.Builder speed = new EgmSpeedRef.Builder();

            speed.SetCartesians(CartesianSpeedBuilder(speeds));
            _sensor.SetSpeedRef(speed);
        }

        /// <summary>
        /// Fills the message with a joints speed command
        /// </summary>
        /// <param name="cartesian">Joints speed command</param>
        public void SpeedJoints(EgmJoints.Builder joints)
        {
            EgmSpeedRef.Builder speed = new EgmSpeedRef.Builder();

            speed.SetJoints(joints);
            _sensor.SetSpeedRef(speed);
        }

        /// <summary>
        /// Fills the message with a joints speed command
        /// </summary>
        /// <param name="joints">6 joints values as an array</param>
        public void SpeedJoints(double[] joints)
        {
            EgmSpeedRef.Builder speed = new EgmSpeedRef.Builder();

            speed.SetJoints(JointsBuilder(joints));
            _sensor.SetSpeedRef(speed);
        }

        /// <summary>
        /// Fills the message with an external joints speed command
        /// </summary>
        /// <param name="cartesian">External joints speed command</param>
        public void SpeedExtJoints(EgmJoints.Builder extJoints)
        {
            EgmSpeedRef.Builder speed = new EgmSpeedRef.Builder();

            speed.SetExternalJoints(extJoints);
            _sensor.SetSpeedRef(speed);
        }

        /// <summary>
        /// Fills the message with an external joints speed command
        /// </summary>
        /// <param name="joints">6 joints values as an array</param>
        public void SpeedExtJoints(double[] joints)
        {
            EgmSpeedRef.Builder speed = new EgmSpeedRef.Builder();

            speed.SetExternalJoints(JointsBuilder(joints));
            _sensor.SetSpeedRef(speed);
        }


        /// <summary>
        /// Builds the message to send
        /// </summary>
        /// <returns>Message to send as a bytes array</returns>
        public byte[] Build()
        {
            MemoryStream memoryStream = new MemoryStream();
            
            EgmSensor sensorMessage = _sensor.Build();

            sensorMessage.WriteTo(memoryStream);

            return (memoryStream.ToArray());
        }












        





    }
}
