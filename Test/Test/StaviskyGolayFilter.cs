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
        /// <param name="buffersize">Number of coefficients - Size of the window</param>
        /// <param name="degree">Degree of the interpolated polynom</param>
        public StaviskyGolayFilter(int buffersize, int degree) : base(buffersize)
        {
            if (buffersize <= degree)
            {
                throw new System.Exception("[SG] Le degré du polynôme doit être plus grand que la fenêtre");
            }
            if (buffersize % 2 == 0)
            {
                throw new System.Exception("[SG] La taille de la fenêtre doit être impaire");
            }
            else
            {
                double[] fenetre = new double[buffersize];
                int l = buffersize / 2;
                for (int i = 0; i < l; i++)
                {
                    fenetre[i] = -(l - i);
                    fenetre[buffersize - i - 1] = l - i;
                }
                fenetre[l] = 0;

                double[,] jacobien = new double[buffersize, degree];
                for (int i = 0; i < buffersize; i++)
                {
                    for (int j = 0; j < degree; j++)
                    {
                        jacobien[i, j] = Math.Pow(fenetre[i], j);
                    }
                }

                double[,] matriceCoef = Matrix.Dot(Matrix.Inverse(Matrix.Dot(Matrix.Transpose(jacobien), jacobien)), Matrix.Transpose(jacobien));
                double[] ligneCoef = new double[buffersize];

                for (int i = 0; i < buffersize; i++)
                {
                    ligneCoef[i] = matriceCoef[0, i];
                }

                coefficients = ligneCoef;

                for (int i = 0; i < buffersize; i++)
                {
                    Console.WriteLine(coefficients[i]);
                }

            }
        }
    }
}
