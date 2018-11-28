﻿using System;
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
        /// <param name="filtererSize">Number of coefficients - Size of the measure window</param>
        public AverageFilter(int filterSize) : base(filterSize)
        {
            double[] array = new double[filterSize];
            for(int i=0;i<filterSize;i++)
            {
                array[i] = 1.0 / filterSize;
            }
            _coefficients = array;

            for(int i=0;i<filterSize;i++)
            {
                System.Diagnostics.Debug.Print(_coefficients[i].ToString());
            }
        }
    }
}
