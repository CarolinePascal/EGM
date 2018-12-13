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
        /// Degree of the interpolated polynom
        /// </summary>
        private int _degree;

        public override string ToString()
        {
            return "Filtre de Stavisky-Golay" + Environment.NewLine + "Degre du polynome interpolé : " + _degree.ToString() + Environment.NewLine + base.ToString();
        }

        /// <summary>
        /// Stavisky-Golay filter constructor
        /// </summary>
        /// <param name="filtersize">Number of coefficients - Size of the measure window</param>
        /// <param name="degree">Degree of the interpolated polynom</param>
        public StaviskyGolayFilter(int filtersize, int degree, params HAL.ENPC.Control.OnlineController[] controllers) : base(filtersize, controllers)
        {
            if (filtersize < degree)
            {
                throw new System.Exception("[SG] Le degré du polynôme doit être plus petit (ou égal) que la fenêtre");
            }
            if (filtersize % 2 == 0)
            {
                throw new System.Exception("[SG] La taille de la fenêtre doit être impaire");
            }
            else
            {
                _degree = degree;

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
                //HAL.Numerics.Matrix m = new Numerics.Matrix(filtersize, degree);
                for (int j = 0; j < degree; j++)
                {
                    //double[] values = new double[filtersize];
                    for (int i = 0; i < filtersize; i++)
                    {
                        jacobien[i, j] = Math.Pow(fenetre[i], j);
                        //values[i] = Math.Pow(fenetre[i], j);
                    }
                    //HAL.Numerics.Vector vector = new HAL.Numerics.Vector(values);
                    //m.SetColumn(j,vector.Values);
                }
                //Numerics.Matrix matCoef = m.Transpose().Multiply(m).Inverse().Multiply(m.Transpose());


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
