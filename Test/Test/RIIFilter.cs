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
