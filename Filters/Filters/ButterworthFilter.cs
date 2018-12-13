using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAL.ENPC.Debug
{
    class ButterworthFilter : RIIFilter
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
            return "Filtre passe-bas RII de BUtterworth " + Environment.NewLine + "Frequence de coupure : " + _cuttingFrequency.ToString() + " Hz -- Frequence d'echantillonnage : " + _samplingFrequency.ToString() + " Hz" + Environment.NewLine + base.ToString();

        }

        /// <summary>
        /// RII low pass filter constructor - 1st, 2nd or 3rd order Butterwoth filter
        /// </summary>
        /// <param name="cuttingFrequency">Cutting frequency in Hz</param>
        /// <param name="samplingFrequency">Sampling frequency in Hz</param>
        /// <param name="order">Number of filtered values coefficients - Order of the filter</param>
        public ButterworthFilter(double cuttingFrequency, double samplingFrequency, int order, params HAL.ENPC.Control.OnlineController[] controllers) : base(order + 1,order, controllers)
        {

            if (cuttingFrequency <= 0 || samplingFrequency <= 0)
            {
                throw new System.Exception("[Butterworth] Les fréquences doivent être strictement positives");
            }

            else
            {
                _cuttingFrequency = cuttingFrequency;
                _samplingFrequency = samplingFrequency;

                //Frequency conversion from digital to analogic
                double f = (samplingFrequency / Math.PI) * Math.Tan(Math.PI * cuttingFrequency / samplingFrequency);

                double[] arrayM = new double[order + 1];
                double[] arrayF = new double[order];

                switch ((Order)order)
                {
                    case Order.Unset:
                        throw new System.Exception("[Butterworth] Les seuls ordres possibles sont 1, 2 ou 3");
                    case Order.One:
                        arrayM[0] = ((samplingFrequency / (f * Math.PI)) + 1);
                        arrayM[1] = arrayM[0];
                        arrayF[0] = ((samplingFrequency / (f * Math.PI)) - 1) / ((samplingFrequency / (f * Math.PI)) + 1);
                        break;
                    case Order.Two:
                        double a = (Math.Sqrt(2) * samplingFrequency) / (Math.PI * f);
                        double b = (samplingFrequency * samplingFrequency) / Math.Pow(Math.PI * f, 2);

                        arrayM[0] = 1 / (1 + a + b);
                        arrayM[1] = 2 * arrayM[0];
                        arrayM[2] = arrayM[0];
                        arrayF[0] = 2 * (b - 1) / (a + b + 1);
                        arrayF[1] = -(b - a + 1) / (a + b + 1);
                        break;
                    case Order.Three:
                        double c= (2 * samplingFrequency) / (Math.PI * f);
                        double d = (2 * samplingFrequency * samplingFrequency) / Math.Pow(Math.PI * f, 2);
                        double e = Math.Pow(samplingFrequency, 3) / Math.Pow(Math.PI * f, 3);

                        arrayM[0] = 1 / (1 + c + d + e);
                        arrayM[1] = 3 * arrayM[0];
                        arrayM[2] = 3 * arrayM[0];
                        arrayM[3] = arrayM[0];
                        arrayF[0] = (3 * e + d - c - 3) / (1 + c + d + e);
                        arrayF[1] = -(3 * e - d - c + 3) / (1 + c + d + e);
                        arrayF[2] = (e - d + c - 1) / (1 + c + d + e);
                        break;
                    default:
                        throw new System.Exception("[Butterworth] Les seuls ordres possibles sont 1, 2 ou 3");
                }

                _coefficientsFiltered = arrayF;
                _coefficientsMeasures = arrayM;

                for (int i = 0; i < _coefficientsFiltered.Length; i++)
                {
                    System.Diagnostics.Debug.Print(_coefficientsFiltered[i].ToString());
                }

                for (int i = 0; i < _coefficientsMeasures.Length; i++)
                {
                    System.Diagnostics.Debug.Print(_coefficientsMeasures[i].ToString());
                }

            }
        }


    }
    public enum Order
    {
        Unset,
        One,
        Two,
        Three
    }
}
