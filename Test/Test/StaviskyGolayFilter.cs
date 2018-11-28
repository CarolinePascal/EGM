using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HAL.ENPC.Filtering;
using HAL.ENPC.Sensoring.SensorData;
using System.Collections;
using Accord.Math;

namespace HAL.ENPC.Debug
{
    class StaviskyGolayFilter : RIFFilter
    {
        /// <summary>
        /// Stavisky-Golay filter constructor
        /// </summary>
        /// <param name="filtersize">Number of coefficients - Size of the measure window</param>
        /// <param name="degree">Degree of the interpolated polynom</param>
        public StaviskyGolayFilter(int filtersize, int degree) : base(filtersize)
        {
            if (filtersize <= degree)
            {
                throw new System.Exception("[SG] Le degré du polynôme doit être strictement plus grand que la fenêtre");
            }
            if (filtersize % 2 == 0)
            {
                throw new System.Exception("[SG] La taille de la fenêtre doit être impaire");
            }
            else
            {
                double[] fenetre = new double[filtersize];
                int l = filtersize / 2;

                //Centering
                for (int i = 0; i < l; i++)
                {
                    fenetre[i] = -(l - i);
                    fenetre[filtersize - i - 1] = l - i;
                }
                fenetre[l] = 0;

                //Computation of the interolation
                double[,] jacobien = new double[filtersize, degree];
                for (int i = 0; i < filtersize; i++)
                {
                    for (int j = 0; j < degree; j++)
                    {
                        jacobien[i, j] = Math.Pow(fenetre[i], j);
                    }
                }

                double[,] matriceCoef = Matrix.Dot(Matrix.Inverse(Matrix.Dot(Matrix.Transpose(jacobien), jacobien)), Matrix.Transpose(jacobien));
                double[] ligneCoef = new double[filtersize];

                for (int i = 0; i < filtersize; i++)
                {
                    ligneCoef[i] = matriceCoef[0, i];
                }

                _coefficients = ligneCoef;

                for (int i = 0; i < filtersize; i++)
                {
                    System.Diagnostics.Debug.Print(_coefficients[i].ToString());
                }

            }
        }
    }
}
