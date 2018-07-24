using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EGMProjet
{
    public struct EulerAngles
    {
        /// <summary>
        /// First Euler Angle - Precession angle
        /// </summary>
        public double Psi { get; }

        /// <summary>
        /// Second Euler angle - Nutation angle
        /// </summary>
        public double Theta { get; }

        /// <summary>
        /// Third Euler angle - Intrinsic rotation angle
        /// </summary>
        public double Phi { get; }

        /// <summary>
        /// EulerAngles instance constructor
        /// </summary>
        /// <param name="psi">First Euler angle - Precession angle</param>
        /// <param name="theta">Second Euler angle - Nutation angle</param>
        /// <param name="phi">Third Euler angle - Intrinsic rotation angle</param>
        public EulerAngles(double psi, double theta, double phi)
        {
            Psi = psi;
            Theta = theta;
            Phi = phi;
        }

        public override string ToString()
        {
            String str = Psi.ToString() + " " + Theta.ToString() + " " + Phi.ToString();
            return str;
        }
    }
}
