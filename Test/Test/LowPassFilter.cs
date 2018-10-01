using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HAL.ENPC.Filtering;
using HAL.ENPC.Sensoring.SensorData;

namespace HAL.ENPC.Debug
{
    class LowPassFilter : RIFFilter
    {
        /// <summary>
        /// RIF low pass filter constructor - Kaiser-Bessel filter
        /// </summary>
        /// <param name="bufferSize">Number of coefficients - Size of the window</param>
        /// <param name="cuttingFrequency">Cutting frequency in Hz</param>
        /// <param name="samplingFrequency">Sampling frequency in Hz</param>
        public LowPassFilter(int bufferSize, double cuttingFrequency, double samplingFrequency) : base(bufferSize)
        {
            if (cuttingFrequency <= 0 || samplingFrequency <= 0)
            {
                throw new System.Exception("[RIF LPF] Les fréquences doivent être strictement positives");
            }
            if (bufferSize % 2 == 0)
            {
                throw new System.Exception("[RIF LPF] Le nombre de coefficients - bufferSize - doit être impair");
            }
            else
            {
                double[] array = new double[bufferSize];
                double omegaC = 2 * cuttingFrequency / samplingFrequency;

                for (int j = 0; j < bufferSize; j++)
                {
                    double arg = j - (bufferSize - 1) / 2;
                    double x = omegaC * arg * Math.PI;
                    if (x > -1.0E-5 && x < 1.0E-5)
                        array[j] = omegaC;
                    else
                        array[j] = omegaC * Math.Sin(x) / x;
                }

                coefficients = array;

                for (int i = 0; i < bufferSize; i++)
                {
                    Console.WriteLine(coefficients[i]);
                }
            }
        }

    }
}
