using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelProcessing
{
    class Monte_Carlo
    {
        private Func<decimal, decimal> F;
        private int left;
        private int right;
        private decimal n;
        private decimal k, p;
        private int donePercent = 0;
        private int parts = 4;

        public struct Point
        {
            private decimal x, y;
            private bool inArea;
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
            public bool InArea
            {
                get { return inArea; }
                set { inArea = value; }
            }
        };
        public Point[] MasPoint { get; private set; }
        //public List
        public PointF[] Rect { get; private set; }
        public decimal Result { get; private set; }
        decimal Max;
        decimal Min;
        Random rnd = new Random();

        public delegate void Progress(int value);
        public event Progress EventProgress;
        public delegate void Finish(decimal resultValue);
        public event Finish EventFinish;
        public Monte_Carlo(Func<decimal, decimal> F, int left, int right, decimal n)
        {
            this.F = F;
            this.left = left;                      
            this.right = right;
            this.n = n;
            MasPoint = new Point[(int)n];
            Result = 0;
            k = 0; p = 0;
            Max = maxi();
            Min = mini();
            Rect = new PointF[4];
            Rect[0] = new PointF(left, (float)Min);
            Rect[1] = new PointF(left, (float)Max);
            Rect[2] = new PointF(right, (float)Max);
            Rect[3] = new PointF(right, (float)Min);
            
        }

        public void Start()
        {
            Parallel.For(0, parts, _Calculate);
            Result = (Math.Abs(Max) + Math.Abs(Min)) * Math.Abs(right - left) * (k - p) / n;
            if (donePercent != n)
                EventProgress((int)n);
            EventFinish?.Invoke(Result);
        }
        private void Schitaem(int i)       
        {
            decimal x = Convert.ToDecimal(rnd.Next(left * 1000, right * 1000)/(1000.1));
            decimal y = rnd.Next((int)(Min*1000), (int)(Max*1000)) / 1000;
            MasPoint[i].X = x;
            MasPoint[i].Y = y;
            if (Math.Abs(y)<=Math.Abs(F(x)))
            {
                if ((F(x) > 0 && y > 0))
                {
                    MasPoint[i].InArea = true;
                    k++;
                }
                if (F(x) < 0 && y < 0)
                {
                    MasPoint[i].InArea = true;
                    p++;
                }

            }
            else
            {
                MasPoint[i].InArea = false;
            }
            donePercent += 1;
            EventProgress?.Invoke(donePercent);
        }


        decimal maxi()
        {
            if (right > 0) return F(right);
            const decimal epsilon = (decimal)(1e-10);
            decimal a1 = left;
            decimal b1 = right;

            decimal goldenRatio = ((decimal)(1 + Math.Sqrt(5))) / 2; // "Золотое" число

            decimal x1, x2; // Точки, делящие текущий отрезок в отношении золотого сечения
            while (Math.Abs(b1 - a1) > epsilon)
            {
                x1 = b1 - (b1 - a1) / goldenRatio;
                x2 = a1 + (b1 - a1) / goldenRatio;
                if (F(x1) <= F(x2)) a1 = x1;
                else b1 = x2;
            }
            return F((a1 + b1) / 2);
        }

        decimal mini()
        {
            if (left < (-20 / 3)) return F(left);
            const decimal epsilon = (decimal)(1e-10);
            decimal a1 = left;
            decimal b1 = right;

            decimal goldenRatio = ((decimal)(1 + Math.Sqrt(5))) / 2; // "Золотое" число

            decimal x1, x2; // Точки, делящие текущий отрезок в отношении золотого сечения
            while (Math.Abs(b1 - a1) > epsilon)
            {
                x1 = b1 - (b1 - a1) / goldenRatio;
                x2 = a1 + (b1 - a1) / goldenRatio;
                if (F(x1) >= F(x2)) a1 = x1;
                else b1 = x2;
            }
            return F((a1 + b1) / 2);
        }

        private void _Calculate(int part)
        {
            int partsSize = (int)((int)(n) / parts);
            int ost = (int)n - partsSize * parts;
            int st = part * partsSize + ((part < ost) ? part : ost);
            int fn = (part + 1) * partsSize + ((part + 1 < ost) ? part : (ost - 1));
            //int Count = 0;
            for (int i = st; i <= fn; i++)
            {
                decimal x = Convert.ToDecimal(rnd.Next(left * 1000, right * 1000) / (1000.1));
                decimal y = rnd.Next((int)(Min * 1000), (int)(Max * 1000)) / 1000;
                MasPoint[i].X = x;
                MasPoint[i].Y = y;
                if (Math.Abs(y) <= Math.Abs(F(x)))
                {
                    if ((F(x) > 0 && y > 0))
                    {
                        MasPoint[i].InArea = true;
                        k++;
                    }
                    if (F(x) < 0 && y < 0)
                    {
                        MasPoint[i].InArea = true;
                        p++;
                    }

                }
                else
                {
                    MasPoint[i].InArea = false;
                }
                donePercent += 1;
                EventProgress?.Invoke(donePercent);

            }
        }
    }
}
