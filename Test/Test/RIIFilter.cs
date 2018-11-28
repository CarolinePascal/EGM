using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HAL.ENPC.Filtering;
using HAL.ENPC.Sensoring.SensorData;
using System.Collections;
using HAL.ENPC.Messaging;


namespace HAL.ENPC.Debug
{
    class RIIFilter : Filter<Torsor>
    {  
        /// <summary>
        /// Coefficients of the RII filter - measured values
        /// </summary>
        private double[] _coefficientsMeasures { get; set; }

        /// <summary>
        /// coefficents of the RII filter - filtered values
        /// </summary>
        private double[] _coefficientsFiltered { get; set; }

        /// <summary>
        /// Queue dedicated to the storage of filtered values
        /// </summary>
        private Queue<Torsor> _filteredbuffer;

        /// <summary>
        /// Complete RII filter constructor
        /// </summary>
        /// <param name="filtersize">Number of measured values coefficients - Size of the measures window</param>
        /// <param name="order">Number of filtered values coefficients - Order of the filter</param>
        /// <param name="arrayMeasures">Measured values coefficients</param>
        /// <param name="arrayFiltered">Filtered values coefficients</param>
        public RIIFilter(int filtersize, int order, double[] arrayMeasures, double[] arrayFiltered) : base(filtersize) 
        {
            if (arrayMeasures.Length != filtersize)
            {
                throw new System.Exception("[RII] Le vecteur de ponderation des valeurs mesurees doit être de la même taille que la fenetre de mesures");
            }
            else if (arrayFiltered.Length != order)
            {
                throw new System.Exception("[RII] Le vecteur de ponderation des valeurs filtrees doit être de la même taille que l'ordre du filtre");
            }
            else
            {
                _coefficientsMeasures = arrayMeasures;
                _coefficientsFiltered = arrayFiltered;
                _filteredbuffer = new Queue<Torsor>(_coefficientsFiltered.Length);
            }
        }

        /// <summary>
        /// RII low pass filter constructor - 1st, 2nd or 3rd order Butterwoth filter
        /// </summary>
        /// <param name="cuttingFrequency">Cutting frequency in Hz</param>
        /// <param name="samplingFrequency">Sampling frequency in Hz</param>
        /// <param name="order">Number of filtered values coefficients - Order of the filter</param>
        public RIIFilter(double cuttingFrequency, double samplingFrequency, int order):base(order+1)
        {

            if(cuttingFrequency<=0 || samplingFrequency <= 0)
            {
                throw new System.Exception("[RII LPF] Les fréquences doivent être strictement positives");
            }

            if(order != 1 || order !=2 || order!=3)
            {
                throw new System.Exception("[RII LPF] Les seuls ordres possibles sont 1, 2 ou 3");
            }

            else
            {
                //Frequency conversion from digital to analogic
                double f = (samplingFrequency / Math.PI) * Math.Tan(Math.PI * cuttingFrequency / samplingFrequency);

                double[] arrayM = new double[order+1];
                double[] arrayF = new double[order];

                //1st order filter
                if (order == 1)
                {
                    arrayM[0] = (4 * Math.Pow(Math.PI * f, 3)) / (samplingFrequency + Math.PI * f);
                    arrayM[1] = -arrayM[0];
                    arrayF[0] = (samplingFrequency - Math.PI * f) / (samplingFrequency + Math.PI * f);
                }

                //2nd order filter
                else if (order == 2)
                {
                    double a = (Math.Sqrt(2) * samplingFrequency) / (Math.PI * f);
                    double b = (samplingFrequency * samplingFrequency) / Math.Pow(Math.PI * f, 2);

                    arrayM[0] = 1 / (1 + a + b);
                    arrayM[1] = -2 * arrayM[0];
                    arrayM[2] = arrayM[0];
                    arrayF[0] = 2 * (b - 1) / (a + b + 1);
                    arrayF[1] = (b - a + 1) / (a + b + 1);
                }

                //3rd order filter
                else
                {
                    double a = (2 * samplingFrequency) / (Math.PI * f);
                    double b = (2 * samplingFrequency * samplingFrequency) / Math.Pow(Math.PI * f, 2);
                    double c = Math.Pow(samplingFrequency, 3) / Math.Pow(Math.PI * f, 3);

                    arrayM[0] = 1 / (1 + a + b + c);
                    arrayM[1] = -3 * arrayM[0];
                    arrayM[2] = 3 * arrayM[0];
                    arrayM[3] = -arrayM[0];
                    arrayF[0] = (3 * c + b - a - 3) / (1 + a + b + c);
                    arrayF[1] = (3 * c - b - a + 3) / (1 + a + b + c);
                    arrayF[2] = (c - b + a - 1) / (1 + a + b + c);
                }

                //Normalization
                double sum = arrayF.Sum();

                for(int i = 0; i < order; i++)
                {
                    arrayF[i] /= sum;
                }

                _coefficientsFiltered = arrayF;
                _coefficientsMeasures = arrayM;

                for(int i=0;i<_coefficientsFiltered.Length;i++)
                {
                    System.Diagnostics.Debug.Print(_coefficientsFiltered[i].ToString());
                }

                for (int i = 0; i < _coefficientsMeasures.Length; i++)
                {
                    System.Diagnostics.Debug.Print(_coefficientsMeasures[i].ToString());
                }

            }
        }

        /// <summary>
        /// RII filter filtering process
        /// </summary>
        /// <returns></returns>
        protected override Torsor FilterMethod()
        {
            //Initialisation of the filtered values queue
            if(_filteredbuffer.Count==0)
            {
                for(int i=0;i<_coefficientsFiltered.Length;i++)
                {
                    _filteredbuffer.Enqueue(FilterBuffer[i]);
                }
            }

            //y_n = \sum_{i = l-1}^{0}a_ix_{n - i} + \sum_{i = k}^{1}b_iy_{n - i}
            Torsor torsor = Torsor.Default;
            for(int i=0;i<_coefficientsMeasures.Length;i++)
            {
                torsor = torsor.Add(Multiply(FilterBuffer[(CurrentIndex + i) % FilterSize], _coefficientsMeasures[FilterSize - i - 1]));
            }
            
            int n = 0;
            for(int j=0;j<_coefficientsFiltered.Length;j++)
            {
                torsor = torsor.Add(Multiply(_filteredbuffer.Peek(), _coefficientsFiltered[FilterSize-j-1]));
                n++;
            }

            //Upload new filtered values
            _filteredbuffer.Dequeue();
            _filteredbuffer.Enqueue(torsor);

            return (torsor);
        }

        /// <summary>
        /// Term by term double multiplication method for the Torsor structure
        /// </summary>
        /// <param name="torsor">Torsor to be multiplied</param>
        /// <param name="multiplier">Multiplier as a double</param>
        /// <returns></returns>
        public static Torsor Multiply(Torsor torsor, double multiplier)
        {
            return new Torsor(
                torsor.TX * multiplier, torsor.TY * multiplier, torsor.TZ * multiplier,
                torsor.RX * multiplier, torsor.RY * multiplier, torsor.RZ * multiplier);
        }

    }
}
