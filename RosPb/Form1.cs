using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace RosPb
{
    public class PSTATE
    {
        public int Floor;
        public int Ceiling;
        public int Bias;
    }

    public partial class Form1 : Form
    {
        public Bitmap bmp = new Bitmap(800, 600);
        private PSTATE InbvProgressState = new PSTATE();

        public int ProgressBarLeft = 5;
        public int ProgressBarTop = 5;
        public int ProgressBarWidth = 185;
        public int ProgressBarHeight = 4;
        public Color ProgressBarForeground => Color.White;
        public Color ProgressBarBackground => Color.Gray;
        public bool ProgressBarShowBkg = true;
        public bool ProgressBarRounding = true;
        private PictureBox pic = new PictureBox();

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.Controls.Add(pic);
            this.Load += Form1_Load;
            this.FormClosing += (s, e) => Environment.Exit(0);

            var btn = new Button();
            btn.Text = "Restart";
            btn.Click += (b, s) => StartThread();
            this.Controls.Add(btn);

            var hslider = new NumericUpDown();
            hslider.Value = ProgressBarHeight;
            hslider.Left = 150;
            hslider.ValueChanged += (s, e) => ProgressBarHeight = (int)hslider.Value;
            this.Controls.Add(hslider);

            var roundCb = new CheckBox();
            roundCb.Text = "Rounding";
            roundCb.Checked = ProgressBarRounding;
            roundCb.Left = 150;
            roundCb.Top = 30;
            roundCb.Click += (s, e) => ProgressBarRounding = roundCb.Checked;
            this.Controls.Add(roundCb);

            roundCb = new CheckBox();
            roundCb.Text = "Background";
            roundCb.Checked = ProgressBarRounding;
            roundCb.Left = 150;
            roundCb.Top = 60;
            roundCb.Click += (s, e) => ProgressBarShowBkg = roundCb.Checked;
            this.Controls.Add(roundCb);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StartThread();
        }

        private void StartThread()
        {
            new Thread(() =>
            {
                InbvSetProgressBarSubset(0, 100);
                bmp = new Bitmap(400, 200);
                VidSolidColorFill(0, 0, 400, 200, Color.Black);

                for (int i = 0; i <= 100; i++)
                {
                    try
                    {
                        InbvUpdateProgressBar(i);
                    }
                    catch { }
                    Thread.Sleep(10);
                }

                bmp.Save("b.png", ImageFormat.Png);
            }).Start();
        }

        private void InbvSetProgressBarSubset(int Floor, int Ceiling)
        {
            /* Update the progress bar state */
            InbvProgressState.Floor = Floor * 100;
            InbvProgressState.Ceiling = Ceiling * 100;
            InbvProgressState.Bias = (Ceiling * 100) - Floor;
        }

        private void DrawProgressBar(int Left, int FillCount, bool AllowLeftRound, Color Color)
        {
            if (!ProgressBarRounding)
            {
                VidSolidColorFill(Left,
                      ProgressBarTop,
                      ProgressBarLeft + FillCount,
                      ProgressBarTop + ProgressBarHeight,
                      Color);
            }
            else
            {
                /* Left rounding */
                if (AllowLeftRound)
                {
                    VidSolidColorFill(Left,
                                      ProgressBarTop + 1,
                                      ProgressBarLeft,
                                      ProgressBarTop + ProgressBarHeight - 1,
                                      Color);
                }

                /* Main line */
                VidSolidColorFill(Left,
                                  ProgressBarTop,
                                  ProgressBarLeft + FillCount - 1,
                                  ProgressBarTop + ProgressBarHeight,
                                  Color);

                /* Right rounding */
                VidSolidColorFill(ProgressBarLeft + FillCount - 1,
                                  ProgressBarTop + 1,
                                  ProgressBarLeft + FillCount ,
                                  ProgressBarTop + ProgressBarHeight - 1,
                                  Color);
            }
        }

        private void InbvUpdateProgressBar(int Progress)
        {
            var BoundedProgress = (InbvProgressState.Floor / 100) + Progress;
            var FillCount = ProgressBarWidth * (InbvProgressState.Bias * BoundedProgress) / 1000000;

            DrawProgressBar(ProgressBarLeft, FillCount, true, ProgressBarForeground);

            if (ProgressBarShowBkg)
            {
                var BoundedProgressMax = (InbvProgressState.Floor / 100) + (InbvProgressState.Ceiling / 100);
                var FillCountMax = ProgressBarWidth * (InbvProgressState.Bias * BoundedProgressMax) / 1000000;
                DrawProgressBar(ProgressBarLeft + FillCount, FillCountMax, false, ProgressBarBackground);
            }

            this.Invoke(new Action(() =>
            {
                this.Controls.Remove(pic);
                pic = new PictureBox() { BackgroundImage = bmp, Top = 100, Width = 400 };
                this.Controls.Add(pic);
            }));
        }

        public void VidSolidColorFill(int x1, int y1, int x2, int y2, Color color)
        {
            for (var x = x1; x < x2; x++)
                for (var y = y1; y < y2; y++)
                    bmp.SetPixel(x, y, color);
        }
    }
}
