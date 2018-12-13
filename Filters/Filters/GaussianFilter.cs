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
        /// <param name="filterSize">Number of coefficients - Size of the measure window</param>
        /// <param name="mu">Gaussian distribution average</param>
        /// <param name="sigma">Gaussian distribution standard deviation</param>
        public GaussianFilter(int filterSize, double mu, double sigma, params HAL.ENPC.Control.OnlineController[] controllers) :base(filterSize, controllers)
        {
            if (sigma < 0)
            {
                throw new System.Exception("[Gaussian] L'écart type de la loi gaussienne doit être positif ou nul");
            }

            double[] array = new double[filterSize];

            //Weighting
            if (filterSize % 2 != 0)
            {
                array[filterSize / 2] = (Math.Exp(-(0 - mu) * (0 - mu) / (2 * sigma * sigma)) / Math.Sqrt(2 * Math.PI * sigma * sigma));
            }

            for (int i = 0; i < filterSize / 2; i++)
            {
                int x = (filterSize / 2) - i;
                array[i] = (Math.Exp(-(x - mu) * (x - mu) / (2 * sigma * sigma)) / Math.Sqrt(2 * Math.PI * sigma * sigma));
                array[filterSize - i - 1] = (Math.Exp(-(x - mu) * (x - mu) / (2 * sigma * sigma)) / Math.Sqrt(2 * Math.PI * sigma * sigma));
            }

            _coefficients = array;

            //Normalization
            double sum = _coefficients.Sum();

            for (int i = 0; i < filterSize; i++)
            {
                _coefficients[i] /= sum;
            }

            for (int i=0;i<filterSize;i++)
            {
                System.Diagnostics.Debug.Print(_coefficients[i].ToString());
            }
        }
    }
}
