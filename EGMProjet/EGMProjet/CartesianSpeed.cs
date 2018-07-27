using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EGMProjet
{
    public struct CartesianSpeed
    {
        /// <summary>
        /// Cartesian speed values array
        /// </summary>
        public double[] Speed { get; }

        /// <summary>
        /// CartesianSpeed instance values constructor
        /// </summary>
        /// <param name="vx">Speed on the X axis in mm/s</param>
        /// <param name="vy">Speed on the Y axis in mm/s</param>
        /// <param name="vz">Speed on the Z axis in mm/s</param>
        /// <param name="vpsi">Speed according to the first Euler angle in degrees/mm</param>
        /// <param name="vtheta">Speed according to the second Euler angle in degrees/mm</param>
        /// <param name="vphi">Speed according to the third Euler angle in degrees/mm</param>
        public CartesianSpeed(double vx, double vy, double vz, double vpsi, double vtheta, double vphi)
        {
            Speed = new double[] { vx, vy, vz, vpsi, vtheta, vphi };
        }

        /// <summary>
        /// CartesianSpeed instance array constructor
        /// </summary>
        /// <param name="speed">6 speed values as an array - The 3 first are in mm/s on the X,Y,Z axis ans the 3 last are in degrees/s on the Euler angles axis</param>
        public CartesianSpeed(double[] speed)
        {
            Speed = new double[6];
            Speed = speed;
        }

        public override string ToString()
        {
            string str = string.Empty;

            for (int i=0;i<6;i++)
            {
                str += Speed[i].ToString() + " ";
            }
            return (str);
        }

    }
}
