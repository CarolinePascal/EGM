using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EGMProjet
{
    public struct Joints
    {
        /// <summary>
        /// Joints values array
        /// </summary>
        public double[] Rotations { get; } 

        /// <summary>
        /// Joints instance values constructor
        /// </summary>
        /// <param name="j1">First joint value</param>
        /// <param name="j2">Second joint value</param>
        /// <param name="j3">Third joint value</param>
        /// <param name="j4">Fourth joint value</param>
        /// <param name="j5">Fifth joint value</param>
        /// <param name="j6">Sixth joint value</param>
        public Joints(double j1, double j2, double j3, double j4, double j5, double j6)
        {
            Rotations = new double[] { j1, j2, j3, j4, j5, j6 };
        }

        /// <summary>
        /// Joints instance array constructor
        /// </summary>
        /// <param name="rotations">6 joints values as an array</param>
        public Joints(double[] rotations)
        {
            Rotations = new double[6];
            Rotations = rotations;
        }
    }
}
