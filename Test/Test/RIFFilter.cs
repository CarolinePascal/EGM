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
    abstract class RIFFilter : Filter<Torsor>
    {
        /// <summary>
        /// Coefficients of the RIF filter
        /// </summary>
        protected double [] coefficients { get; set; }

        /// <summary>
        /// Complete RIF filter constructor
        /// </summary>
        /// <param name="bufferSize">Number of coefficients - Size of the window</param>
        /// <param name="array">Coefficents of the RIF filter</param>
        public RIFFilter(int bufferSize, double[] array):base(bufferSize)
        {
            if(bufferSize!=array.Length)
            {
                throw new System.Exception("[RIF] Le vecteur de ponderation doit être de la même taille que le buffer");
            }
            else
            {
                coefficients = array;
            }
        }

        /// <summary>
        /// Partial RIF filter constructor - the coefficients are all set to 0
        /// </summary>
        /// <param name="bufferSize">Size of the value window</param>
        public RIFFilter(int bufferSize):base(bufferSize)
        {
            coefficients = new double[bufferSize];
        }

        /// <summary>
        /// FIR filter filtering process
        /// </summary>
        /// <returns></returns>
        protected override Torsor FilterMethod()
        {
            Torsor torsor = Torsor.Default;
            for(int i=0;i<FilterBuffer.Length-1;i++)
            {
                torsor = torsor.Add(Multiply(FilterBuffer[i], coefficients[i]));
            }
            return torsor;
        }

        /// <summary>
        /// Term by term multiplication method for the Torsor structure
        /// </summary>
        /// <param name="torsor"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        public Torsor Multiply(Torsor torsor, double multiplier)
        {
            return new Torsor(
                torsor.TX * multiplier, torsor.TY * multiplier, torsor.TZ * multiplier,
                torsor.RX * multiplier, torsor.RY * multiplier, torsor.RZ * multiplier);
        }

    }
}
