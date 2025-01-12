﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HAL.ENPC.Filtering;
using HAL.ENPC.Sensoring.SensorData;
using HAL.ENPC.Sensoring;
using System.Collections;
using Accord.Math;
using HAL.ENPC.Messaging;


namespace HAL.ENPC.Debug
{

    public abstract class RIFFilter : Filter<TorsorState>
    {
        /// <summary>
        /// Coefficients of the RIF filter
        /// </summary>
        protected double[] _coefficients { get; set; }

        /// <summary>
        /// Override of the ToString() method for RIF filters
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = "Coefficients : " + Environment.NewLine;
            for (int i = 0; i < _coefficients.Length; i++)
            {
                str += "a" + i.ToString() + " = " + _coefficients[i] + Environment.NewLine;
            }
            return (str);
        }

        /// <summary>
        /// Complete RIF filter constructor
        /// </summary>
        /// <param name="filterSize">Number of coefficients - Size of the measures window or order of the filter</param>
        /// <param name="array">Coefficents of the RIF filter</param>
        public RIFFilter(int filterSize, double[] array, params HAL.ENPC.Control.OnlineController[] controllers) : base(filterSize, controllers)
        {
            if (filterSize != array.Length)
            {
                throw new System.Exception("[RIF] Le vecteur de ponderation doit être de la même taille que le buffer");
            }
            else
            {
                _coefficients = array;
            }
        }

        /// <summary>
        /// Partial RIF filter constructor - the coefficients are all set to 0
        /// </summary>
        /// <param name="filterSize">Size of the measures window or order of the filter</param>
        public RIFFilter(int filterSize, params HAL.ENPC.Control.OnlineController[] controllers) : base(filterSize, controllers)
        {
            _coefficients = new double[filterSize];
        }

        /// <summary>
        /// FIR filter filtering process
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
                Torsor torsor = Torsor.Default;

                for (int i = 0; i < FilterSize; i++)
                {
                    //y_n = \sum_{i=l-1}^{0} a_{i}x_{n-i}
                    torsor = torsor.Add(Multiply(FilterBuffer[(CurrentIndex + i) % FilterSize].Value, _coefficients[FilterSize - i - 1]));
                }

                return (new TorsorState(torsor, false));
            }

        }

        /// <summary>
        /// Term by term double multiplication method for the Torsor structure
        /// </summary>
        /// <param name="torsor">Torsor to be multiplied</param>
        /// <param name="multiplier">Multiplier as a double</param>
        /// <returns></returns>
        public Torsor Multiply(Torsor torsor, double multiplier)
        {
            return new Torsor(
                torsor.TX * multiplier, torsor.TY * multiplier, torsor.TZ * multiplier,
                torsor.RX * multiplier, torsor.RY * multiplier, torsor.RZ * multiplier);
        }

    }
}
