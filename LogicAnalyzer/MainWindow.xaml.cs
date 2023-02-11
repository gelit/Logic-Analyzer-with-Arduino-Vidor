// 16 jan 2023 : cette vidéo m'a sauvé !!!  https://www.youtube.com/watch?v=cvfkz0s6czA
//               car je ne comprenais pas https://www.appsloveworld.com/csharp/100/45/how-do-you-draw-a-line-on-a-canvas-in-wpf-that-is-1-pixel-thick

// length of type Char = 16 bit (Unicode) 

#pragma warning disable IDE0054 
#pragma warning disable IDE0017
#pragma warning disable IDE0090
#pragma warning disable CA1822
#pragma warning disable CA1806

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using System.IO;
using System.IO.Ports;
using System.Windows.Threading;
using System.Diagnostics;
using static System.Net.WebRequestMethods;
using System.Windows.Shapes;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Xml.Linq;

namespace LogicAnalyzer
{
    public partial class MainWindow : Window
    {
        static int V1, V2, UpDownV;
        static int Curso, Cu1V, Cu2V; // Cursor Value
        static SerialPort SP;
        static bool USB, Stop;
        static float Tech = 879; // Default Period ajusted from 100 micros Arduino Due
        static int Mode = 1;   // Cursor mode
        static string RxAll = "something";

        public MainWindow()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10); // Active Time_Tick every 0.01 sec
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
            USB = false; Curso = 1; Cu1V = 0; Cu2V = 0; Stop = false; UpDownV = 1;
            DisplayTim();
            PrintLargeTime();
        }

        public void Timer_Tick(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                if (!USB)
                {
                    if (port == "COM3") { SP = new SerialPort("COM3", 800000); SP.Open(); USB = true; User.Text = "Vigor detected"; SerialTx(Convert.ToByte('R')); }
                }
            }

            if (!USB) { User.Foreground = new SolidColorBrush(Colors.Red); User.Text = "NO DETECTION !"; Console.Beep(1900, 1000); }
            else
            {
                if (SP.BytesToRead > 0)
                {
                    String Rx = SP.ReadLine(); // attend CR
                    Debug.Write(Rx); // parfois très utile

                    byte[] B = Encoding.ASCII.GetBytes(Rx);  // from https://www.c-sharpcorner.com/article/c-sharp-string-to-byte-array/
                    int L = Rx.Length;

                    if (B[0] == 'N') { if (RxAll.Length != 0) { RxAll = RxAll.Remove(0); } } // New Transfer --> Clear RxAll

                    else if (B[0] == 'L')  // Level to display
                    { 
                        switch (B[1])
                        {
                            case 49: if (B[2] == '1') { A1.Text = Convert.ToString("H"); } else if (B[2] == '0') { A1.Text = Convert.ToString("L"); } else { A1.Text = Convert.ToString("T"); } break;
                            case 50: if (B[2] == '1') { A2.Text = Convert.ToString("H"); } else if (B[2] == '0') { A2.Text = Convert.ToString("L"); } else { A2.Text = Convert.ToString("T"); } break;
                            case 51: if (B[2] == '1') { A3.Text = Convert.ToString("H"); } else if (B[2] == '0') { A3.Text = Convert.ToString("L"); } else { A3.Text = Convert.ToString("T"); } break;
                            case 52: if (B[2] == '1') { A4.Text = Convert.ToString("H"); } else if (B[2] == '0') { A4.Text = Convert.ToString("L"); } else { A4.Text = Convert.ToString("T"); } break;
                            case 53: if (B[2] == '1') { A5.Text = Convert.ToString("H"); } else if (B[2] == '0') { A5.Text = Convert.ToString("L"); } else { A5.Text = Convert.ToString("T"); } break;
                            case 54: if (B[2] == '1') { A6.Text = Convert.ToString("H"); } else if (B[2] == '0') { A6.Text = Convert.ToString("L"); } else { A6.Text = Convert.ToString("T"); } break;
                            case 55: if (B[2] == '1') { A7.Text = Convert.ToString("H"); } else if (B[2] == '0') { A7.Text = Convert.ToString("L"); } else { A7.Text = Convert.ToString("T"); } break;
                        }
                    }

                    else
                    {
                        if (L > 0 && L < 8)
                        {
                            UpDownV = 1;
                            RxAll = String.Concat(RxAll, Rx);  // Save if Frequency change
                            
                            switch (L - 3)  // Supprimer premier, second & dernier
                            {
                                case 1: V2 = B[2] - 48; break;
                                case 2: V2 = (B[2] - 48) * 10 + B[3] - 48; break;
                                case 3: V2 = (B[2] - 48) * 100 + (B[3] - 48) * 10 + B[4] - 48; ; break;
                                case 4: V2 = (B[2] - 48) * 1000 + (B[3] - 48) * 100 + (B[4] - 48) * 10 + B[5] - 48; ; break;
                            }

                            int Canal = B[0] - 48;
                            if (Canal > 0 && Canal < 8)
                            {
                                switch (B[1])
                                {
                                    case 72: LH(Canal, V1, V2); V1 = V2; break;  // High
                                    case 76: LL(Canal, V1, V2); V1 = V2; break;  // Low
                                    case 67: Clear(Canal); V1 = 0; break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LL(int Canal, int p1, int p2)  // Low
        {
            switch (Canal)
            {
                case 1: La(1, p1, 125, p2, 125); break;
                case 2: La(2, p1, 225, p2, 225); break;
                case 3: La(3, p1, 325, p2, 325); break;
                case 4: La(4, p1, 425, p2, 425); break;
                case 5: La(5, p1, 525, p2, 525); break;
                case 6: La(6, p1, 625, p2, 625); break;
                case 7: La(7, p1, 725, p2, 725); break;
            }
        }
        private void LH(int Canal, int p1, int p2)  // High
        {
            switch (Canal)
            {
                case 1: La(1, p1,  75, p2,  75); break;
                case 2: La(2, p1, 175, p2, 175); break;
                case 3: La(3, p1, 275, p2, 275); break;
                case 4: La(4, p1, 375, p2, 375); break;
                case 5: La(5, p1, 475, p2, 475); break;
                case 6: La(6, p1, 575, p2, 575); break;
                case 7: La(7, p1, 675, p2, 675); break;
            }
        }

        private void Clear(int Canal)  // Clear 
        {
            Line lineH; lineH = new Line();
            lineH.Stroke = Brushes.Black;
            switch (Canal)
            {
                case 1: lineH.X1 = 0; lineH.Y1 =  75; lineH.X2 = 1900; lineH.Y2 = 75; break;       // High
                case 2: lineH.X1 = 0; lineH.Y1 = 175; lineH.X2 = 1900; lineH.Y2 = 175; break;
                case 3: lineH.X1 = 0; lineH.Y1 = 275; lineH.X2 = 1900; lineH.Y2 = 275; break;
                case 4: lineH.X1 = 0; lineH.Y1 = 375; lineH.X2 = 1900; lineH.Y2 = 375; break;
                case 5: lineH.X1 = 0; lineH.Y1 = 475; lineH.X2 = 1900; lineH.Y2 = 475; break;
                case 6: lineH.X1 = 0; lineH.Y1 = 575; lineH.X2 = 1900; lineH.Y2 = 575; break;
                case 7: lineH.X1 = 0; lineH.Y1 = 675; lineH.X2 = 1900; lineH.Y2 = 675; break;
            }
            lineH.StrokeThickness = 4;
            lineH.HorizontalAlignment = HorizontalAlignment.Left;
            lineH.VerticalAlignment = VerticalAlignment.Top;
            myGrid.Children.Add(lineH);
            lineH.SnapsToDevicePixels = true;  // https://stackoverflow.com/questions/2879033/how-do-you-draw-a-line-on-a-canvas-in-wpf-that-is-1-pixel-thick
            lineH.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            Line lineL; lineL = new Line();
            lineL.Stroke = Brushes.Black;
            switch (Canal)
            {
                case 1: lineL.X1 = 0; lineL.Y1 = 125; lineL.X2 = 1900; lineL.Y2 = 125; break;       // Low
                case 2: lineL.X1 = 0; lineL.Y1 = 225; lineL.X2 = 1900; lineL.Y2 = 225; break;
                case 3: lineL.X1 = 0; lineL.Y1 = 325; lineL.X2 = 1900; lineL.Y2 = 325; break;
                case 4: lineL.X1 = 0; lineL.Y1 = 425; lineL.X2 = 1900; lineL.Y2 = 425; break;
                case 5: lineL.X1 = 0; lineL.Y1 = 525; lineL.X2 = 1900; lineL.Y2 = 525; break;
                case 6: lineL.X1 = 0; lineL.Y1 = 625; lineL.X2 = 1900; lineL.Y2 = 625; break;
                case 7: lineL.X1 = 0; lineL.Y1 = 725; lineL.X2 = 1900; lineL.Y2 = 725; break;
            }
            lineL.StrokeThickness = 4;
            lineL.HorizontalAlignment = HorizontalAlignment.Left;
            lineL.VerticalAlignment = VerticalAlignment.Top;
            myGrid.Children.Add(lineL);
            lineL.SnapsToDevicePixels = true;
            lineL.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
        }
        private void La(int Canal, int x1, int y1, int x2, int y2)  // Line
        {
            Line myline; myline = new Line();
            switch (Canal)
            {
                case 1: myline.Stroke = Brushes.Blue; break;
                case 2: myline.Stroke = Brushes.Red; break;
                case 3: myline.Stroke = Brushes.Green; break;
                case 4: myline.Stroke = Brushes.Magenta; break;
                case 5: myline.Stroke = Brushes.Yellow; break;
                case 6: myline.Stroke = Brushes.Brown; break;
                case 7: myline.Stroke = Brushes.Orange; break;
            }

            myline.X1 = x1; myline.Y1 = y1; myline.X2 = x2; myline.Y2 = y2;
            myline.StrokeThickness = 2;
            myline.HorizontalAlignment = HorizontalAlignment.Left;
            myline.VerticalAlignment = VerticalAlignment.Top;
            myGrid.Children.Add(myline);  // Merci à https://www.youtube.com/watch?v=cvfkz0s6czA
        }
        private void Cu(int Cursor)  // Full Cursor
        {
            Cu1(Cursor, 130, 170); Cu1(Cursor, 230, 270); Cu1(Cursor, 330, 370); Cu1(Cursor, 430, 470); Cu1(Cursor, 530, 570); Cu1(Cursor, 630, 670); Cu1(Cursor, 730, 750);

        }
        private void Cu1(int Cursor, int P1, int P2)  // 1 Cursor
        {
            Line myline; myline = new Line();
            myline.X1 = Cursor; myline.Y1 = P1; myline.X2 = Cursor; myline.Y2 = P2;
            myline.Stroke = Brushes.White; myline.StrokeThickness = 1;
            myline.HorizontalAlignment = HorizontalAlignment.Left; myline.VerticalAlignment = VerticalAlignment.Top; myGrid.Children.Add(myline);
            myline.SnapsToDevicePixels = true;
            myline.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
        }
        private void CuC(int Cursor)  // Full Cursor
        {
            Cu1C(Cursor, 130, 170); Cu1C(Cursor, 230, 270); Cu1C(Cursor, 330, 370); Cu1C(Cursor, 430, 470); Cu1C(Cursor, 530, 570); Cu1C(Cursor, 630, 670); Cu1C(Cursor, 730, 750);
        }
        private void Cu1C(int Cursor, int P1, int P2)  // 1 Cursor Clear
        {
            Line myline; myline = new Line();
            if (Cursor == 1) { myline.X1 = Cu1V; myline.Y1 = P1; myline.X2 = Cu1V; myline.Y2 = P2; }
            else { myline.X1 = Cu2V; myline.Y1 = P1; myline.X2 = Cu2V; myline.Y2 = P2; }
            myline.Stroke = Brushes.Black; myline.StrokeThickness = 2;
            myline.HorizontalAlignment = HorizontalAlignment.Left; myline.VerticalAlignment = VerticalAlignment.Top; myGrid.Children.Add(myline);
            myline.SnapsToDevicePixels = true;
            myline.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
        }
        private void PrintLargeTime()
        {
            if (Tech < 1000) { float v = (1900 * Tech) / 1000; string s = v.ToString("0"); MaxTime.Text = s + " micros"; }
            else { float v = (1900 * Tech) / 1000000; string s = v.ToString("0"); MaxTime.Text = s + " millis"; }
        }
        private void DisplayTim()
        {
            PosV.Text = Convert.ToString(Cu1V);
            if (Cu1V == Cu2V) { TimeV.Text = "0"; }
            else if (Cu1V > Cu2V)
            {
                if (Cu1V - Cu2V < 1000) { float v = ((Cu1V - Cu2V) * Tech) / 1000; string s = v.ToString("0"); TimeV.Text = s; }
                else { float v = ((Cu1V - Cu2V) * Tech) / 1000000; string s = v.ToString("0"); TimeV.Text = s; }
            }
            else
            {
                if (Cu2V - Cu1V < 1000) { float v = ((Cu2V - Cu1V) * Tech) / 1000; string s = v.ToString("0"); TimeV.Text = s; }
                else { float v = ((Cu2V - Cu1V) * Tech) / 1000000; string s = v.ToString("0"); TimeV.Text = s; }
            }
        }

        private void GLPreviewKeyDown(object sender, KeyEventArgs e)
        // 29 avril 2022 : https://social.msdn.microsoft.com/Forums/en-US/a5d60518-eb40-4392-a79c-afa81acd3af5/cant-detect-enter-key-or-space-key-in-wpf-c-visual-studio?forum=wpf
        // Ne pas oublier de modifier xaml

        {
            //Debug.Print(Convert.ToString(e.Key));

            if (e.Key == Key.S)
            {
                if (Stop) { State.Foreground = new SolidColorBrush(Colors.Blue); State.Text = "Running"; }   // Stop acquisition
                else { State.Foreground = new SolidColorBrush(Colors.Red); State.Text = "Stopped"; }
                Stop = !Stop; SerialTx(Convert.ToByte('S'));
            }

            if (e.Key == Key.B) { SerialTx(Convert.ToByte('B')); Trig2.Text = "Begin"; }  // Trigger (default)
            if (e.Key == Key.C) { SerialTx(Convert.ToByte('C')); Trig2.Text = "Center"; }
            if (e.Key == Key.E) { SerialTx(Convert.ToByte('E')); Trig2.Text = "End"; }

            if (e.Key == Key.LeftShift) { if (Mode == 1) { Mode = 2; } else { Mode = 1; } }

            if (e.Key == Key.Right)
            {
                CuC(Curso); 
                if (Curso == 1) { if (Mode == 1) { Cu1V++; } else { Cu1V += 10; } Cu(Cu1V); DisplayTim(); }
                else            { if (Mode == 1) { Cu2V++; } else { Cu2V += 10; } Cu(Cu2V); DisplayTim(); }
            }

            if (e.Key == Key.Left)
            {
                CuC(Curso); 
                if (Curso == 1) { if (Mode == 1) { Cu1V--; } else { Cu1V -= 10; } Cu(Cu1V); DisplayTim(); }
                else            { if (Mode == 1) { Cu2V--; } else { Cu2V -= 10; } Cu(Cu2V); DisplayTim(); }
            }

            if (e.Key == Key.Tab) { if (Curso == 1) { Curso = 2; } else { Curso = 1; } }
            if (e.Key == Key.Z) { CuC(1); CuC(2); Curso = 1; Cu1V = 0; Cu2V = 0; DisplayTim(); }  // Init Cursor

            if (e.Key == Key.Down) { SerialTx(Convert.ToByte('D')); if (Tech < 30000) { Tech = 2 * Tech; } PrintLargeTime(); Refresh('D'); }
            if (e.Key == Key.Up)   { SerialTx(Convert.ToByte('U')); if (Tech > 100) { Tech = Tech / 2; }   PrintLargeTime(); Refresh('U'); }

            if (e.Key == Key.D1) { SerialTx(Convert.ToByte('1')); }  // Generator
            if (e.Key == Key.D2) { SerialTx(Convert.ToByte('2')); }
            if (e.Key == Key.D3) { SerialTx(Convert.ToByte('3')); }
            if (e.Key == Key.D4) { SerialTx(Convert.ToByte('4')); }
            if (e.Key == Key.D5) { SerialTx(Convert.ToByte('5')); }
            if (e.Key == Key.D6) { SerialTx(Convert.ToByte('6')); }
            if (e.Key == Key.D7) { SerialTx(Convert.ToByte('7')); }
            if (e.Key == Key.D8) { SerialTx(Convert.ToByte('8')); }
            if (e.Key == Key.D9) { SerialTx(Convert.ToByte('9')); }

            //if (e.Key == Key.Enter)  // Ne pas utiliser Enter qui a une fonction liée à la fenêtre
            // to disable this function https://stackoverflow.com/questions/70203939/how-to-disable-enter-key-for-a-button
        }
        void Refresh(char UpDown)  // with Up Down key
        {
            
            if (UpDown == 'U')
            {
                if (UpDownV > 0) { UpDownV = 2 * UpDownV; } // 2 4 8 ...
                else { UpDownV = UpDownV / 2; if (UpDownV == -1) { UpDownV = 1; } }

                CuC(1); Cu1V *= 2;  Cu(Cu1V); CuC(2); Cu2V *= 2; Cu(Cu2V); DisplayTim();
            }
            else if (UpDown == 'D')
            {
                if (UpDownV == 1) { UpDownV = -2; }
                else if (UpDownV < 0) { UpDownV = 2 * UpDownV; } // -2 -4 -8 ...
                else { UpDownV = UpDownV / 2;  }  // 8 4 2

                CuC(1); Cu1V /= 2; Cu(Cu1V); CuC(2); Cu2V /= 2; Cu(Cu2V); DisplayTim();
            }

            int V3 = 0; int V4 = 0; int Canal = 0; int Level = 0;
            byte[] B = Encoding.ASCII.GetBytes(RxAll);
            int L = RxAll.Length;

            for (int N = 0; N < L; N++)
            {
                if (B[N] == 13) // CR
                {
                    if (B[N - 1] >= '0' && B[N - 1] <= '9')
                    {
                        if (B[N - 2] >= '0' && B[N - 2] <= '9')
                        {
                            if (B[N - 3] >= '0' && B[N - 3] <= '9')
                            {
                                if (B[N - 4] >= '0' && B[N - 4] <= '9')
                                {
                                    if (B[N - 5] >= '0' && B[N - 5] <= '9') { Debug.WriteLine("Error3"); }  // Impossible
                                    else { V4 = (B[N - 4] - 48) * 1000 + (B[N - 3] - 48) * 100 + (B[N - 2] - 48) * 10 + B[N - 1] - 48; Level = B[N - 5]; } // Lettre + 4 digit 
                                }
                                else { V4 = (B[N - 3] - 48) * 100 + (B[N - 2] - 48) * 10 + B[N - 1] - 48; Level = B[N - 4]; } // Lettre + 3 digit 
                            }
                            else { V4 = (B[N - 2] - 48) * 10 + B[N - 1] - 48; Level = B[N - 3]; }// Lettre + 2 digit 
                        }
                        else { V4 = B[N - 1] - 48; Level = B[N - 2]; }  // Lettre + 1 digit 
                    }
                    else  if (B[N - 1] == 'C') { Canal = B[N - 2] - 48; Clear(Canal); V3 = 0; V4 = 0; }  // Clear

                    if (V4 != 0)
                    {
                        if (UpDownV > 0) { V4 = UpDownV * V4; if (V4 > 1900) { V4 = 1900; } }
                        else if (UpDownV < 0) { V4 = V4 / -UpDownV; }
                    }

                    if (Level == 'H' && V4 < 1905) { LH(Canal, V3, V4); V3 = V4; } // High
                    else if (Level == 'L')         { LL(Canal, V3, V4); V3 = V4; }  // Low
                }
            }
        }
        void SerialTx(byte B) { if (USB) { SP.Write(new byte[] { B }, 0, 1); } }  // valeur 0 n'est pas envoyée
    }
}


