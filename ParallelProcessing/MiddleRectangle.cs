using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelProcessing
{
    class MiddleRectangle
    {
        private Func<decimal, decimal> F;
        private int left;
        private int right;
        private decimal n;
        private decimal step;
        private int donePercent = 0;
        private int parts = 4;
        public decimal Result { get;  set; }

        public delegate void Progress(int value);
        public event Progress EventProgress;

        public delegate void Finish(decimal resultValue);
        public event Finish EventFinish;
        public struct Point
        {
            private decimal x, y;
            public decimal X { 
                get { return x; }
                set { x = value; } 
            }
            public decimal Y
            {
                get { return y; }
                set { y = value; }
            }
        };
        public Point[] MasPoint { get; private set; } 
        public MiddleRectangle(Func<decimal,decimal> F,int left, int right, decimal n)
        {
            this.F = F;
            this.left = left; 
            this.right = right;
            this.n = n;
            step = (right - left) / n;              
            MasPoint = new Point[(int)(n * 4)];
            Result = 0;
            
        }
        public void Start()
        {
            Parallel.For(0, parts, _Calculate);
            if (donePercent != n)
                EventProgress((int)n );
            EventFinish?.Invoke(Result);
        }
        /*private void Calculate(int i)
        {
            //вычислить площадь для рузультата
            decimal x = left + i * step + step / 2;
            Result = Result + F(x) * step;
            //вычислить координаты точек
            var k = i * 4;
            MasPoint[k].X = x - step / 2;
            MasPoint[k].Y = 0;
            MasPoint[k+1].X = x - step / 2;
            MasPoint[k+1].Y = F(x);
            MasPoint[k+2].X = x + step / 2;
            MasPoint[k+2].Y = F(x);
            MasPoint[k+3].X = x + step / 2;
            MasPoint[k+3].Y = 0;
            donePercent += 1;
            EventProgress?.Invoke(donePercent);
        }*/


        private void _Calculate(int part)
        {

            //Result = (-func(a) + func(b)) / 2;

            int partsSize = (int)n / parts;
            int ost = (int)n - partsSize * parts;
            int st = part * partsSize + ((part < ost) ? part : ost);
            int fn = (part + 1) * partsSize + ((part + 1 < ost) ? part : (ost - 1));
            double s = 0;
            for (int i = st; i <= fn; i++)
            {
                decimal x = left + i * step + step / 2;
                Result = Result + F(x) * step;
                //вычислить координаты точек
                var k = i * 4;
                MasPoint[k].X = x - step / 2;
                MasPoint[k].Y = 0;
                MasPoint[k + 1].X = x - step / 2;
                MasPoint[k + 1].Y = F(x);
                MasPoint[k + 2].X = x + step / 2;
                MasPoint[k + 2].Y = F(x);
                MasPoint[k + 3].X = x + step / 2;
                MasPoint[k + 3].Y = 0;
                donePercent += 1;
                EventProgress?.Invoke(donePercent);
            }
        }
    }
}
