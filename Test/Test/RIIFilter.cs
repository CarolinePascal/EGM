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
    class RIIFilter : Filter<Torsor>
    {

        public double[] coefficientsMeasures { get; set; }
        public double[] coefficientsFiltered { get; set; }

        private Queue<Torsor> Filteredbuffer;

        public RIIFilter(int buffersize, double[] arrayMeasures, double[] arrayFiltered) : base(buffersize) 
        {
            if (arrayMeasures.Length != buffersize)
            {
                throw new System.Exception("[RII] Le vecteur de ponderation doit être de la même taille que le buffer");
            }
            else if (arrayMeasures.Length-1 != arrayFiltered.Length)
            {
                throw new System.Exception("[RII] Le vecteur des valeurs filtrées doit contenir une valeur de moins que le vecteur de mesures");
            }
            else
            {
                coefficientsMeasures = arrayMeasures;
                coefficientsFiltered = arrayFiltered;
                Filteredbuffer = new Queue<Torsor>(coefficientsFiltered.Length);
            }
        }

        public RIIFilter(double cuttingFrequency, double samplingFrequency):base()
        {
            FilterSize = 3;

            if(cuttingFrequency<=0 || samplingFrequency <= 0)
            {
                throw new System.Exception("[RII] Les fréquences doivent être strictement positives");
            }
            else
            {
                double ff = cuttingFrequency / samplingFrequency;
                double ita = 1 / Math.Tan(Math.PI * ff);
                double q = Math.Sqrt(2);

                double[] arrayM = new double[3];
                double[] arrayF = new double[2];

                arrayM[0] = 1 / (1 + q * ita + ita * ita);
                arrayM[1] = 2 * arrayM[0];
                arrayM[2] = arrayM[0];

                arrayF[0] = 2 * (ita * ita - 1) * arrayM[0];
                arrayF[1] = -(1 - q * ita + ita * ita) * arrayM[0];

                coefficientsFiltered = arrayF;
                coefficientsMeasures = arrayM;
                Console.WriteLine("Filtered" + arrayF[0] + " " + arrayF[1]);
                Console.WriteLine("Measured" + arrayM[0] + " " + arrayM[1] + " " + arrayM[2]);
            }
        }

        protected override Torsor FilterMethod()
        {
            if(Filteredbuffer.Count==0)
            {
                for(int i=0;i<coefficientsFiltered.Length;i++)
                {
                    Filteredbuffer.Enqueue(FilterBuffer[i]);
                }
            }

            Torsor torsor = Torsor.Default;
            for(int i=0;i<coefficientsMeasures.Length;i++)
            {
                torsor = torsor.Add(Multiply(FilterBuffer[i], coefficientsMeasures[i]));
            }
            
            int n = 0;
            foreach (Torsor t in Filteredbuffer)
            {
                torsor = torsor.Add(Multiply(t, coefficientsFiltered[n]));
                n++;
            }

            Filteredbuffer.Dequeue();
            Filteredbuffer.Enqueue(torsor);

            return (torsor);
        }

        public static Torsor Multiply(Torsor torsor, double multiplier)
        {
            return new Torsor(
                torsor.TX * multiplier, torsor.TY * multiplier, torsor.TZ * multiplier,
                torsor.RX * multiplier, torsor.RY * multiplier, torsor.RZ * multiplier);
        }

    }
}