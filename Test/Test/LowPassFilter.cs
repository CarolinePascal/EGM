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
    class LowPassFilter : Filter<Torsor>
    {

        public double [] coefficients { get; set; }

        public LowPassFilter(int buffersize, double[] array):base(buffersize)  //Filtre d'ordre 2
        {
            if(buffersize!=array.Length)
            {
                throw new System.Exception("[LowPassFilter] Le vecteur de ponderation doit être de la même taille que le buffer");
            }
            else
            {
                coefficients = array;
            }
        }

        protected override Torsor FilterMethod()
        {
            Torsor torsor = Torsor.Default;
            for(int i=0;i<FilterBuffer.Length-1;i++)
            {
                torsor = torsor.Add(Multiply(FilterBuffer[i], coefficients[i]));
            }

            return torsor;
        }

        public static Torsor Multiply(Torsor torsor, double multiplier)
        {
            return new Torsor(
                torsor.TX * multiplier, torsor.TY * multiplier, torsor.TZ * multiplier,
                torsor.RX * multiplier, torsor.RY * multiplier, torsor.RZ * multiplier);
        }

    }
}
