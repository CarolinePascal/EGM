﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using abb.egm;
using System.IO;
using System.Windows.Media.Media3D; //Add referernce : PresentationCore.dll

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
        /// <param name="seqNumber">Identifier of the last EGM message sent to the robot</param>
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
        /// <param name="vector">Translation vector as a Vector3D</param>
        /// <returns></returns>
        public EgmCartesian.Builder CartesianBuilder(Vector3D vector)
        {
            EgmCartesian.Builder translation = new EgmCartesian.Builder();

            translation.SetX(vector.X)
                       .SetY(vector.Y)
                       .SetZ(vector.Z);

            return (translation);
        }

        /// <summary>
        /// Builds a rotation command from a quaternion
        /// </summary>
        /// <param name="quaternion">Rotation quaternion as a Quaternion</param>
        /// <returns></returns>
        public EgmQuaternion.Builder QuaterionBuilder(Quaternion quaternion)
        {
            EgmQuaternion.Builder rotation = new EgmQuaternion.Builder();

            rotation.SetU0(quaternion.W)
                    .SetU1(quaternion.X)
                    .SetU2(quaternion.Y)
                    .SetU3(quaternion.Z);

            return (rotation);
        }

        /// <summary>
        /// Builds a rotation command from Euler angles
        /// </summary>
        /// <param name="angles">Rotation Euler angles as an EulerAngles</param>
        /// <returns></returns>
        public EgmEuler.Builder EulerBuilder(EulerAngles angles)
        {
            EgmEuler.Builder rotation = new EgmEuler.Builder();

            rotation.SetX(angles.Psi)
                    .SetY(angles.Theta)
                    .SetZ(angles.Phi);

            return (rotation);
        }

        /// <summary>
        /// Builds a cartesian motion command from a translation command and a rotation command
        /// </summary>
        /// <param name="cartesian">Trasnlation command</param>
        /// <param name="quaternion">Rotation command from a quaternion</param>
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
        /// <param name="euler">Rotation command from Euler angles</param>
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
        /// <param name="rotations">Motion joints values as a Joints</param>
        /// <returns></returns>
        public EgmJoints.Builder JointsBuilder(Joints joints)
        {
            EgmJoints.Builder motion = new EgmJoints.Builder();

            for (int i = 0; i < 6; i++)
            {
                motion.AddJoints(joints.Rotations[i]);
            }

            return (motion);
        }

        /// <summary>
        /// Builds a cartesian speed reference command 
        /// </summary>
        /// <param name="speeds">Cartesian speed command as a CartesianSpeed</param>
        /// <returns></returns>
        public EgmCartesianSpeed.Builder CartesianSpeedBuilder(CartesianSpeed speed)
        {
            EgmCartesianSpeed.Builder command = new EgmCartesianSpeed.Builder();

            for (int i = 0; i < 6; i++)
            {
                command.AddValue(speed.Speed[i]);
            }

            return (command);
        }



        /// <summary>
        /// Fills the message with a cartesian motion command
        /// </summary>
        /// <param name="vector">Translation vector as a Vector3D</param>
        /// <param name="quaternion">Rotation quaternion as a Quaternion</param>
        public void MovePose(Vector3D vector, Quaternion quaternion)
        {
            EgmPlanned.Builder planned = new EgmPlanned.Builder();

            planned.SetCartesian(PoseBuilder(CartesianBuilder(vector), QuaterionBuilder(quaternion)));
            _sensor.SetPlanned(planned);
        }

        /// <summary>
        /// Fills the message with a cartesian motion command
        /// </summary>
        /// <param name="vector">Translation vector as a Vector3D</param>
        /// <param name="angles">Rotation Euler angles as an EulerAngles</param>
        public void MovePose(Vector3D vector, EulerAngles angles)
        {
            EgmPlanned.Builder planned = new EgmPlanned.Builder();

            planned.SetCartesian(PoseBuilder(CartesianBuilder(vector), EulerBuilder(angles)));
            _sensor.SetPlanned(planned);
        }

        /// <summary>
        /// Fills the message with a joints motion command
        /// </summary>
        /// <param name="joints">Motion joints values as a Joints</param>
        public void MoveJoints(Joints joints)
        {
            EgmPlanned.Builder planned = new EgmPlanned.Builder();

            planned.SetJoints(JointsBuilder(joints));
            _sensor.SetPlanned(planned);
        }

        /// <summary>
        /// Fills the message with a joints motion command
        /// </summary>
        /// <param name="joints">Motion joints values as a Joints</param>
        public void MoveExternalJoints(Joints joints)
        {
            EgmPlanned.Builder planned = new EgmPlanned.Builder();

            planned.SetExternalJoints(JointsBuilder(joints));
            _sensor.SetPlanned(planned);
        }



        /// <summary>
        /// Fills the message with a cartesian speed command
        /// </summary>
        /// <param name="speed">Cartesian speed command as a CartesianSpeed</param>
        public void SpeedCartesian(CartesianSpeed speed)
        {
            EgmSpeedRef.Builder command = new EgmSpeedRef.Builder();

            command.SetCartesians(CartesianSpeedBuilder(speed));
            _sensor.SetSpeedRef(command);
        }

        /// <summary>
        /// Fills the message with a joints speed command
        /// </summary>
        /// <param name="joints">Speed joints values as a Joints</param>
        public void SpeedJoints(Joints joints)
        {
            EgmSpeedRef.Builder speed = new EgmSpeedRef.Builder();

            speed.SetJoints(JointsBuilder(joints));
            _sensor.SetSpeedRef(speed);
        }

        /// <summary>
        /// Fills the message with an external joints speed command
        /// </summary>
        /// <param name="joints">Motion joints values as a Joints</param>
        public void SpeedExtJoints(Joints joints)
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

        /// <summary>
        /// Displays the contents of the message to send
        /// </summary>
        public void DisplayOutboundMessage()
        {
            Console.WriteLine("Message envoyé :");

            if(_sensor.HasHeader)
            {
                Console.WriteLine("Header : seqno = {0} ; tm = {1} ; MessageType = {2}", _sensor.Header.Seqno.ToString(), _sensor.Header.Tm.ToString(), _sensor.Header.Mtype.ToString());
            }
            else
            {
                Console.WriteLine("No Header");
            }

            if(_sensor.HasPlanned)
            {
                Console.WriteLine("Planned : ");
                if(_sensor.Planned.HasJoints)
                {
                    Console.WriteLine("Joints :");
                    Console.WriteLine("J1 = {0} ; J2 = {1} ; J3 = {2} ; J4 = {3} ; J5 = {4} ; J6={5}", _sensor.Planned.Joints.GetJoints(0), _sensor.Planned.Joints.GetJoints(1), _sensor.Planned.Joints.GetJoints(2), _sensor.Planned.Joints.GetJoints(3), _sensor.Planned.Joints.GetJoints(4), _sensor.Planned.Joints.GetJoints(5));
                }
                if(_sensor.Planned.HasExternalJoints)
                {
                    Console.WriteLine("External Joints :");
                    Console.WriteLine("EJ1 = {0} ; EJ2 = {1} ; EJ3 = {2} ; EJ4 = {3} ; EJ5 = {4} ; EJ6={5}", _sensor.Planned.ExternalJoints.GetJoints(0), _sensor.Planned.ExternalJoints.GetJoints(1), _sensor.Planned.ExternalJoints.GetJoints(2), _sensor.Planned.ExternalJoints.GetJoints(3), _sensor.Planned.ExternalJoints.GetJoints(4), _sensor.Planned.ExternalJoints.GetJoints(5));
                }
                if(_sensor.Planned.HasCartesian)
                {
                    if(_sensor.Planned.Cartesian.HasPos)
                    {
                        Console.WriteLine("Pose :");
                        Console.WriteLine("X = {0} ; Y = {1} ; Z ={2}", _sensor.Planned.Cartesian.Pos.X, _sensor.Planned.Cartesian.Pos.Y, _sensor.Planned.Cartesian.Pos.Z);
                    }
                    if(_sensor.Planned.Cartesian.HasEuler)
                    {
                        Console.WriteLine("Euler Angles :");
                        Console.WriteLine("Psi = {0} ; Theta = {1} ; Phi = {2}", _sensor.Planned.Cartesian.Euler.X, _sensor.Planned.Cartesian.Euler.Y, _sensor.Planned.Cartesian.Euler.Z);
                    }
                    if(_sensor.Planned.Cartesian.HasOrient)
                    {
                        Console.WriteLine("Quaternion :");
                        Console.WriteLine("Q1 = {0} ; Q2 = {1} ; Q3 = {2} ; Q4 = {3}", _sensor.Planned.Cartesian.Orient.U0, _sensor.Planned.Cartesian.Orient.U1, _sensor.Planned.Cartesian.Orient.U2, _sensor.Planned.Cartesian.Orient.U3);
                    }
                }
            }
            else
            {
                Console.WriteLine("No Planned");
            }

            if(_sensor.HasSpeedRef)
            {
                Console.WriteLine("SpeedRef :");
                if(_sensor.SpeedRef.HasJoints)
                {
                    Console.WriteLine("Joints Speed :");
                    Console.WriteLine("J1 = {0} ; J2 = {1} ; J3 = {2} ; J4 = {3} ; J5 = {4} ; J6={5}", _sensor.SpeedRef.Joints.GetJoints(0), _sensor.SpeedRef.Joints.GetJoints(1), _sensor.SpeedRef.Joints.GetJoints(2), _sensor.SpeedRef.Joints.GetJoints(3), _sensor.SpeedRef.Joints.GetJoints(4), _sensor.SpeedRef.Joints.GetJoints(5));
                }
                if (_sensor.SpeedRef.HasExternalJoints)
                {
                    Console.WriteLine("External Joints Speed :");
                    Console.WriteLine("EJ1 = {0} ; EJ2 = {1} ; EJ3 = {2} ; EJ4 = {3} ; EJ5 = {4} ; EJ6={5}", _sensor.SpeedRef.ExternalJoints.GetJoints(0), _sensor.SpeedRef.ExternalJoints.GetJoints(1), _sensor.SpeedRef.ExternalJoints.GetJoints(2), _sensor.SpeedRef.ExternalJoints.GetJoints(3), _sensor.SpeedRef.ExternalJoints.GetJoints(4), _sensor.SpeedRef.ExternalJoints.GetJoints(5));
                }
                if(_sensor.SpeedRef.HasCartesians)
                {
                    Console.WriteLine("Cartesian Speed");
                    Console.WriteLine("S1 = {0} ; S2 = {1} ; S3 = {2} ; S4 = {3} ; S5 = {4} ; S6={5}", _sensor.SpeedRef.Cartesians.GetValue(0), _sensor.SpeedRef.Cartesians.GetValue(1), _sensor.SpeedRef.Cartesians.GetValue(2), _sensor.SpeedRef.Cartesians.GetValue(3), _sensor.SpeedRef.Cartesians.GetValue(4), _sensor.SpeedRef.Cartesians.GetValue(5));
                }
            }
            else
            {
                Console.WriteLine("No SpeedRef");
            }

        }












        





    }
}
