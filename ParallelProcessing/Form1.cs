using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ParallelProcessing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            DrawGraphics();
        }

        private decimal F(decimal x)
        {
            return x * x * x + 10 * x * x;
        }
        private int topVal = 10;
        private int lowVal = -10;
        private int method;
        private decimal N = 1;

        MiddleRectangle res1;
        Simpson res2;
        Monte_Carlo res3;

        private void DrawGraphics()
        {
            chart1.Series[0].Points.Clear();
            //наибольшее наименьшее значение
            chart1.ChartAreas[0].AxisX.Minimum = lowVal;
            chart1.ChartAreas[0].AxisX.Maximum = topVal;
            // Определяем шаг сетки ну пусть будет так
            chart1.ChartAreas[0].AxisX.MajorGrid.Interval = 2;
            chart1.ChartAreas[0].AxisX.Crossing = 0;
            chart1.ChartAreas[0].AxisY.Crossing = 0;
            chart1.ChartAreas[0].AxisX.LineWidth = 2;
            chart1.ChartAreas[0].AxisY.LineWidth = 2;
            decimal x = lowVal;
            while (x <= topVal)
            {
                decimal y = F(x);
                chart1.Series[0].Points.AddXY(x, y);
                x = x + 1;
            }
        }
        private void UpdateVal()
        {
            if (!Int32.TryParse(TopValue.Text, out topVal))
            {
                topVal = 10;
                TopValue.Text = "10";
            }
            if (!Int32.TryParse(LowValue.Text, out lowVal))
            {
                lowVal = -10;
                LowValue.Text = "-10";
            }
            if (lowVal > topVal)
            {
                TopValue.Text = lowVal.ToString();
                LowValue.Text = topVal.ToString();
                lowVal = topVal;
                topVal = Convert.ToInt32(LowValue.Text);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            UpdateVal();
            DrawGraphics();
            for (int i = 1; i<6;i++)
            {
                chart1.Series[i].IsVisibleInLegend = false;
                chart1.Series[i].Points.Clear();
            }
            if (method == 0) //метод средних прямоугольников
            {
                progressBar1.Maximum =(int)N;
                chart1.Series[1].IsVisibleInLegend = true;
                res1 = new MiddleRectangle(F, lowVal, topVal, N);
                res1.EventProgress += OnProgress;
                res1.EventFinish += OnFinish;
                res1.Start();
                label2.Text = res1.Result.ToString();
                for (int i = 0; i < N * 4; i++)
                    chart1.Series[1].Points.AddXY(res1.MasPoint[i].X, res1.MasPoint[i].Y);
            }
            if (method ==1) //метод Симпмона
            {
                progressBar1.Maximum = (int)N*2 + 1;
                chart1.Series[2].IsVisibleInLegend = true;
                res2 = new Simpson(F, lowVal, topVal, N);
                //progressBar1.Maximum = res2.;
                res2.EventProgress += OnProgress;
                res2.EventFinish += OnFinish;
                res2.Start();
                foreach (var i in res2.list)
                    chart1.Series[2].Points.AddXY(i.X, i.Y);
                //chart1.Series[1].Sort(PointSortOrder.Ascending, "X");
                label2.Text = res2.Result.ToString();
                //res2.Stop();
                res2.ClearList();
                //чистить список и опять не до конца а еще что тто с треугольниками!!!!!!
            }
            if (method ==2) //метод Монте-Карло
            {
                progressBar1.Maximum =(int) N;
                chart1.Series[3].IsVisibleInLegend = true;
                res3 = new Monte_Carlo(F, lowVal, topVal, N);
                res3.EventProgress += OnProgress;
                res3.EventFinish += OnFinish;
                res3.Start();
                for (int i = 0; i<4;i++)
                {
                    chart1.Series[5].Points.AddXY(res3.Rect[i].X, res3.Rect[i].Y);
                }
                chart1.Series[5].Points.AddXY(res3.Rect[0].X, res3.Rect[0].Y);
                for (int i = 0;i<N;i++)
                {
                    if(res3.MasPoint[i].InArea)
                        chart1.Series[3].Points.AddXY(res3.MasPoint[i].X, res3.MasPoint[i].Y);
                    else
                        chart1.Series[4].Points.AddXY(res3.MasPoint[i].X, res3.MasPoint[i].Y);
                }
                label2.Text = res3.Result.ToString();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            method = comboBox1.SelectedIndex;
            if (method == 0 || method == 1)
            {
                label3.Visible = true;
                label4.Visible = false;
            }
            else
            {
                label3.Visible = false;
                label4.Visible = true;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            N = Convert.ToInt32(numericUpDown1.Value);
        }

        private void OnProgress(int value)
        {

            if (!progressBar1.InvokeRequired)
            {
                progressBar1.Value = value;
                /*if (progressBar1.Value < progressBar1.Maximum)
                    progressBar1.Value = progressBar1.Maximum;*/
            }
            /*else
            {

                if (res1 != null) Invoke(new MiddleRectangle.Progress(OnProgress), value);
                if (res2 != null)
                    Invoke(new Simpson.Progress(OnProgress), value);
                if (res3 != null) Invoke(new Monte_Carlo.Progress(OnProgress), value);
            }*/
        }

        private void OnFinish(decimal resVal)
        {
            if (!label2.InvokeRequired || resVal >= progressBar1.Maximum)
            {
                label2.Text = "Ответ " + resVal;
            }
            else
            {
                if (res1 != null) Invoke(new MiddleRectangle.Finish(OnFinish), resVal);
                if (res2 != null) Invoke(new Simpson.Finish(OnFinish), resVal);
                if (res3 != null) Invoke(new Monte_Carlo.Finish(OnFinish), resVal);

            }
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }
    }
}

   