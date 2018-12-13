using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HAL.ENPC.Filtering;
using HAL.ENPC.Sensoring.SensorData;

namespace HAL.ENPC.Debug
{
    class LanczosFilter : RIFFilter
    {
        /// <summary>
        /// Cutting frequency of the low pass filter
        /// </summary>
        private double _cuttingFrequency;

        /// <summary>
        /// Sampling frquency of the measured values
        /// </summary>
        private double _samplingFrequency;

        public override string ToString()
        {
            return "Filtre passe-bas RIF avec fenêtre de Lanczos" + Environment.NewLine + "Frequence de coupure : "+_cuttingFrequency.ToString()+ " Hz -- Frequence d'echantillonnage : "+_samplingFrequency.ToString()+ " Hz"+Environment.NewLine+ base.ToString();

        }

        /// <summary>
        /// RIF low pass filter constructor - Lanczos window
        /// </summary>
        /// <param name="order">Number of coefficients - order of the filter [must be uneven]</param>
        /// <param name="cuttingFrequency">Cutting frequency in Hz</param>
        /// <param name="samplingFrequency">Sampling frequency in Hz</param>
        /// <param name="beta">Lanczos window parameter</param>
        public LanczosFilter(int order, double cuttingFrequency, double samplingFrequency, double beta=1, params HAL.ENPC.Control.OnlineController[] controllers) : base(order, controllers)
        {
            if (cuttingFrequency <= 0 || samplingFrequency <= 0)
            {
                throw new System.Exception("[Lanczos] Les fréquences doivent être strictement positives");
            }

            if (order%2 != 1)
            {
                throw new System.Exception("[Lanczos] L'ordre du filtre doit être impair");
            }
            
            else
            {
                _cuttingFrequency = cuttingFrequency;
                _samplingFrequency = samplingFrequency;

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

                for (int i = 0; i < order; i++)
                {
                    array[i] *= window[i];
                }

                //Normalization
                double sum = array.Sum();

                for (int i = 0; i < order; i++)
                {
                    array[i] /= sum;
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
