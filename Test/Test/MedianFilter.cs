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
    class MedianFilter : Filter<Torsor>
    {

        public MedianFilter(int bufferSize) : base(bufferSize)
        {

        }

        protected override Torsor FilterMethod()
        {
            List<double> tx= new List<double>();
            List<double> ty = new List<double>();
            List<double> tz = new List<double>();
            List<double> rx = new List<double>();
            List<double> ry = new List<double>();
            List<double> rz = new List<double>();

            foreach (Torsor t in FilterBuffer)
            {
                tx.Add(t.TX);
                ty.Add(t.TY);
                tz.Add(t.TZ);
                rx.Add(t.RX);
                ry.Add(t.RY);
                rz.Add(t.RZ);
            }

            return new Torsor(Median(tx), Median(ty), Median(tz), Median(rx), Median(ry), Median(rz));
        }

        public static double Median(List<double> list)

        {
            double[] array = list.ToArray();
            if (array == null || array.Length == 0)
            {
                throw new System.Exception("La médianne d'un tableau vide n'est pas définie");
            }

            double[] sorted = (double[])array.Clone();
            Array.Sort(sorted);

            int size = array.Length;
            int mid = size / 2;

            double median = (size % 2 != 0) ? (double)sorted[mid] : ((double)sorted[mid] + (double)sorted[mid + 1]) / 2;
            return (median);
        }
    }
}
