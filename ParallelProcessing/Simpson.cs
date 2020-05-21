using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace ParallelProcessing
{
    class Simpson
    {
        private Func<decimal, decimal> F;
        private int left;
        private int right;
        private decimal n;
        private decimal step;
        private int parts = 4;
        private int donePercent = 0;

        public struct Point
        {
            private decimal x, y;
            public decimal X
            {
                get { return x; }
                set { x = value; }
            }
            public decimal Y
            {
                get { return y; }
                set { y = value; }
            }
        }
        public List<Point> list = null;
        public decimal Result { get; private set; }


        public delegate void Progress(int value);
        public event Progress EventProgress;
        public delegate void Finish(decimal resultValue);
        public event Finish EventFinish;

        public Simpson(Func<decimal, decimal> F, int left, int right, decimal n)
        {
            this.F = F;
            this.left = left;                            
            this.right = right;
            this.n = n;
            step = (right - left) / (2 * n);
            s2n = 0;
            sn = 0;
            donePercent = 0;
            list = new List<Point>();
        }
        public void Start()
        {
            Parallel.For(0, parts, _Calculate);
            var i = 1;
            while (i < 2 * n + 1)
            {
                var x = left + i * step;
                Add(x);
                i = i + 2;
            }
            list.Add(new Point { X = right, Y = F(right) });
            Result = step * (F(left) +F(right) +4*sn +2*s2n) / 3;
            if (donePercent != 2 * n + 1)
                EventProgress((int)n*2 + 1);
            EventFinish?.Invoke(Result);
            //list.Sort();

        }

        private decimal sn;
        private decimal s2n;
        private void Calculate(int i)
        {
            donePercent += 1;
            EventProgress?.Invoke(donePercent);
            decimal x = left + i * step;
            if (x != left && x != right)
            {
                if (i % 2 == 0)
                {
                    s2n = s2n + F(x);
                    
                }
                else
                {
                    sn = sn + F(x);
                }
            }

        }

        private void _Calculate(int part)
        {
            int partsSize = (int)(n) / (parts);
            int ost = ((int)n) - partsSize * parts;
            int st = part * partsSize + ((part < ost) ? part : ost);
            int fn = (part + 1) * partsSize + ((part + 1 < ost) ? part : (ost - 1));
            //decimal sum2 = 0;
            //decimal sum4 = 0;
            for (int i = st; i < fn; i++)
            {
                decimal x = left + i * step;
                if (x != left && x != right)
                {
                    if (i % 2 == 0)
                        s2n = s2n + F(x);
                    else
                        sn = sn + F(x);
                }
                donePercent += 1;
                EventProgress?.Invoke(donePercent);
            }
        }
        private void Add(decimal x)
        {
            decimal x1 = x - step, x2 = x, x3 = x + step;
            decimal y1 = F(x1), y2 = F(x2), y3 = F(x3);
            decimal a = (y3 - (x3 * (y2 - y1) + x2 * y1 - x1 * y2) / (x2 - x1)) / (x3 * (x3 - x1 - x2) + x1 * x2);
            decimal b = -a * (x1 + x2) + (y2 - y1) / (x2 - x1);
            decimal c = (x2 * y1 - x1 * y2) / (x2 - x1) + a * x1 * x2;
            var d = x1;
            Point point = new Point { X = 0, Y = 0 };
            //point.X = 0;
            while (d < x3)
            {
                point.Y = a * d * d + b * d + c;
                point.X = d;
                //point = new Point { X = d, Y = a * x * x + b * x + c };
                list.Add(point);
                d = d + 1;
            }
        }
        public void ClearList()
        {
            list.Clear();
        }
    }
}
