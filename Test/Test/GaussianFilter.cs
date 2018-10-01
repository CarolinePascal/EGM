using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HAL.ENPC.Filtering;
using HAL.ENPC.Sensoring.SensorData;

namespace HAL.ENPC.Debug
{
    class GaussianFilter : RIFFilter
    {
        /// <summary>
        /// Gaussian filter constructor
        /// </summary>
        /// <param name="bufferSize">Number of coefficients - Size of the window</param>
        /// <param name="mu">Gaussian mean</param>
        /// <param name="sigma">Gaussian standard deviation</param>
        public GaussianFilter(int bufferSize, double mu, double sigma):base(bufferSize)
        {
            if (sigma < 0)
            {
                throw new System.Exception("[Gaussian] L'écart type de la loi gaussienne doit être positif ou nul");
            }
            double[] array = new double[bufferSize];

            if (bufferSize % 2 != 0)
            {
                array[bufferSize / 2] = (Math.Exp(-(0 - mu) * (0 - mu) / (2 * sigma * sigma)) / Math.Sqrt(2 * Math.PI * sigma * sigma));
            }

            for (int i = 0; i < bufferSize / 2; i++)
            {
                int x = (bufferSize / 2) - i;
                array[i] = (Math.Exp(-(x - mu) * (x - mu) / (2 * sigma * sigma)) / Math.Sqrt(2 * Math.PI * sigma * sigma));
                array[bufferSize - i - 1] = (Math.Exp(-(x - mu) * (x - mu) / (2 * sigma * sigma)) / Math.Sqrt(2 * Math.PI * sigma * sigma));
            }

            coefficients = array;
            double somme = coefficients.Sum();

            for (int i = 0; i < bufferSize; i++)
            {
                coefficients[i] /= somme;
            }

            for (int i=0;i<bufferSize;i++)
            {
                Console.WriteLine(coefficients[i]);
            }
        }
    }
}
