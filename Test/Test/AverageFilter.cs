using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HAL.ENPC.Filtering;
using HAL.ENPC.Sensoring.SensorData;
using System.Collections;

namespace HAL.ENPC.Debug
{
    class AverageFilter : RIFFilter
    {
        /// <summary>
        /// Average filter constructor
        /// </summary>
        /// <param name="bufferSize">Number of coefficients - Size of the window</param>
        public AverageFilter(int bufferSize) : base(bufferSize)
        {
            double[] array = new double[bufferSize];
            for(int i=0;i<bufferSize;i++)
            {
                array[i] = 1.0 / bufferSize;
            }
            coefficients = array;

            for(int i=0;i<bufferSize;i++)
            {
                Console.WriteLine(array[i]);
            }
        }
    }
}
