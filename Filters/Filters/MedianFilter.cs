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
    class MedianFilter : Filter<TorsorState>
    {
        /// <summary>
        /// Median filter constructor
        /// </summary>
        /// <param name="filterSize">Number of coefficients - Size of the measure window</param>
        public MedianFilter(int filterSize, params HAL.ENPC.Control.OnlineController[] controllers) : base(filterSize, controllers)
        {

        }

        /// <summary>
        /// Override of the ToString() method for median filters
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Filtre median - taille de la fenêtre de mesure : " + FilterSize.ToString();
        }

        /// <summary>
        /// Median filter method
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
                List<double> tx = new List<double>();
                List<double> ty = new List<double>();
                List<double> tz = new List<double>();
                List<double> rx = new List<double>();
                List<double> ry = new List<double>();
                List<double> rz = new List<double>();

                foreach (TorsorState t in FilterBuffer)
                {
                    tx.Add(t.Value.TX);
                    ty.Add(t.Value.TY);
                    tz.Add(t.Value.TZ);
                    rx.Add(t.Value.RX);
                    ry.Add(t.Value.RY);
                    rz.Add(t.Value.RZ);
                }

                return (new TorsorState(new Torsor(Median(tx), Median(ty), Median(tz), Median(rx), Median(ry), Median(rz)),false));
            }       
        }

        /// <summary>
        /// Median calculation method
        /// </summary>
        /// <param name="list">List of doubles in random order</param>
        /// <returns></returns>
        public static double Median(List<double> list)
        {
            double[] array = list.ToArray();
            if (array == null || array.Length == 0)
            {
                throw new System.Exception("[Median] La médianne d'un tableau vide n'est pas définie");
            }

            double[] sorted = (double[])array.Clone();
            Array.Sort(sorted);

            int size = array.Length;
            int mid = size / 2;

            double median = (size % 2 != 0) ? (double)sorted[mid] : ((double)sorted[mid-1] + (double)sorted[mid]) / 2;
            return (median);
        }
    }
}
