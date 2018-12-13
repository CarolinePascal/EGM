using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HAL.ENPC.Filtering;
using HAL.ENPC.Sensoring.SensorData;
using System.Collections;
using HAL.ENPC.Messaging;
using HAL.ENPC.Sensoring;


namespace HAL.ENPC.Debug
{
    class RIIFilter : Filter<TorsorState>
    {  
        /// <summary>
        /// Coefficients of the RII filter - measured values
        /// </summary>
        protected double[] _coefficientsMeasures { get; set; }

        /// <summary>
        /// coefficents of the RII filter - filtered values
        /// </summary>
        protected double[] _coefficientsFiltered { get; set; }

        /// <summary>
        /// Queue dedicated to the storage of filtered values
        /// </summary>
        protected Queue<Torsor> _filteredbuffer = new Queue<Torsor>();

        /// <summary>
        /// Override of the ToString() method for RII filters
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = "Coefficients des valeurs mesurees : " + Environment.NewLine;
            for (int i = 0; i < _coefficientsMeasures.Length; i++)
            {
                str += "a" + i.ToString() + " = " + _coefficientsMeasures[i] + Environment.NewLine;
            }

            str += "Coefficients des valeurs filtrees : " + Environment.NewLine;
            for (int i = 0; i < _coefficientsFiltered.Length; i++)
            {
                str += "a" + i.ToString() + " = " + _coefficientsFiltered[i] + Environment.NewLine;
            }

            return (str);
        }

        /// <summary>
        /// Complete RII filter constructor
        /// </summary>
        /// <param name="filtersize">Number of measured values coefficients - Size of the measures window</param>
        /// <param name="order">Number of filtered values coefficients - Order of the filter</param>
        /// <param name="arrayMeasures">Measured values coefficients</param>
        /// <param name="arrayFiltered">Filtered values coefficients</param>
        public RIIFilter(int filtersize, int order, double[] arrayMeasures, double[] arrayFiltered, params HAL.ENPC.Control.OnlineController[] controllers) : base(filtersize, controllers) 
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
        /// Partial RII filter constructor - the coefficients are all set to 0
        /// </summary>
        /// <param name="filterSize">Number of measured values coefficients - Size of the measures window</param>
        /// <param name="order">Number of filtered values coefficients - Order of the filter</param>
        public RIIFilter(int filterSize,int order, params HAL.ENPC.Control.OnlineController[] controllers) : base(filterSize, controllers)
        {
            _coefficientsMeasures = new double[filterSize];
            _coefficientsFiltered = new double[order];
        }

        /// <summary>
        /// RII filter filtering process
        /// </summary>
        /// <returns></returns>
        public override TorsorState FilterMethod(TorsorState sensorData)
        {
            if (sensorData.IsErrorStatus)
            {
                throw new System.Exception("[Reception Captueur][Erreur]");
            }

            else if (FilterBuffer.Last() == null)
            {
                FilterBuffer[CurrentIndex] = sensorData;
                return (sensorData);
            }

            else
            {
                //Initialisation of the filtered values queue
                if (_filteredbuffer.Count==0)
                {
                    for (int i = 0; i < _coefficientsFiltered.Length; i++)
                    {
                        _filteredbuffer.Enqueue(FilterBuffer[i].Value);
                    }
                }

                //y_n = \sum_{i = l-1}^{0}a_ix_{n - i} + \sum_{i = k}^{1}b_iy_{n - i}
                Torsor torsor = Torsor.Default;
                for (int i = 0; i < _coefficientsMeasures.Length; i++)
                {
                    torsor = torsor.Add(Multiply(FilterBuffer[(CurrentIndex + i) % FilterSize].Value, _coefficientsMeasures[FilterSize - i - 1]));
                }

                int n = 0;
                for (int j = 0; j < _coefficientsFiltered.Length; j++)
                {
                    torsor = torsor.Add(Multiply(_filteredbuffer.Peek(), _coefficientsFiltered[_coefficientsFiltered.Length - j - 1]));
                    n++;
                }

                //Upload new filtered values
                _filteredbuffer.Dequeue();
                _filteredbuffer.Enqueue(torsor);

                return (new TorsorState(torsor,false));
            }
        
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
