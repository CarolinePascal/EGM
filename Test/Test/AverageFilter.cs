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
    class AverageFilter : Filter<Torsor>
    { 

        public AverageFilter(int bufferSize) : base(bufferSize)
        {

        }

        protected override Torsor FilterMethod()
        {
            //do something with that...generate filtered value.
            Torsor torsor = Torsor.Default;
            foreach (Torsor t in FilterBuffer)
            {
                torsor = torsor.Add(t);
            }

            return Divide(torsor,FilterBuffer.Length);
        }

        public static Torsor Divide(Torsor torsor, double divider)
        {
            return new Torsor(
                torsor.TX / divider, torsor.TY / divider, torsor.TZ / divider,
                torsor.RX / divider, torsor.RY / divider, torsor.RZ / divider);
        }
    }
}
