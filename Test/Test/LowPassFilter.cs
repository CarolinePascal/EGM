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
        /// RIF low pass filter constructor - Lanczos window
        /// </summary>
        /// <param name="order">Number of coefficients - order of the filter [must be uneven]</param>
        /// <param name="cuttingFrequency">Cutting frequency in Hz</param>
        /// <param name="samplingFrequency">Sampling frequency in Hz</param>
        /// <param name="beta">Lanczos window parameter</param>
        public LowPassFilter(int order, double cuttingFrequency, double samplingFrequency, double beta=1) : base(order)
        {
            if (cuttingFrequency <= 0 || samplingFrequency <= 0)
            {
                throw new System.Exception("[RIF LPF] Les fréquences doivent être strictement positives");
            }

            if (order%2 != 1)
            {
                throw new System.Exception("[RIF LPF] L'ordre du filtre doit être impair");
            }
            
            else
            {
                double[] array = new double[order];

                //Normalization
                double omegaC = 2*cuttingFrequency / samplingFrequency;

                //Perfect low-pass filter
                for (int j = 0; j < order; j++)
                {
                    double arg = j - (order - 1) / 2;
                    double x = omegaC * arg * Math.PI;
                    if (x > -1.0E-5 && x < 1.0E-5)
                        array[j] = omegaC;
                    else
                        array[j] = omegaC * Math.Sin(x) / x;
                }

                //Windowing
                double[] window = new double[order];
                for(int i=0;i<order;i++)
                {
                    double x = (2 * i + 1 - order) * Math.PI / (order + 1);
                    if (x > -1.0E-5 && x < 1.0E-5)
                        window[i] = 1;
                    else
                        window[i] = Math.Sin(x) / x;
                    window[i] = Math.Pow(window[i], beta);
                }

                //Normalization
                double sum = window.Sum();

                for (int i = 0; i < order; i++)
                {
                    window[i] /= sum;
                    array[i] *= window[i];
                }

                _coefficients = array;

                for (int i = 0; i < order; i++)
                {
                    System.Diagnostics.Debug.Print(_coefficients[i].ToString());
                }
            }
        }
    }
}
