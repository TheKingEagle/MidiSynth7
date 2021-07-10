using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using MidiSynth7.entities;
namespace MidiSynth7.entities.controls
{
    /// <summary>
    /// Interaction logic for DialControl.xaml
    /// </summary>
    public partial class DialControl : UserControl
    {
        struct cursorpos
        {
            public int X, Y;
        }
        [DllImport("user32.dll")]
        static extern void SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        static extern void GetCursorPos(out cursorpos lpoint);

        [DllImport("user32.dll")]
        static extern void ClipCursor(ref System.Drawing.Rectangle rect);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(UInt16 virtualKeyCode);

        public DialControl()
        {
            InitializeComponent();
            t.Interval = TimeSpan.FromMilliseconds(1);
            t.Tick += Tick;
            Maximum = 255;



        }
        public int TickDelay { get; set; }
        float addto = 0;
        void Tick(object sender, EventArgs e)
        {
            if (TickDelay < 1)
            {
                TickDelay = 1;
            }
            //Console.WriteLine(GetAsyncKeyState(0x01));
            isDown = GetAsyncKeyState(0x01) < 0;
            if (!isDown)
            {
                t.Stop();
                Mouse.OverrideCursor = this.Cursor;
            }
            if (!isDown)
            {
                return;
            }
            suppressValueChange = false;
            cursorpos cp = new cursorpos();
            GetCursorPos(out cp);
            if (cp.Y < mouseY)
            {
                if (Value < Maximum)
                {

                    addto += mouseY - cp.Y;
                    if (addto >= TickDelay)
                    {

                        Value += (int)(addto / TickDelay);
                        addto = 0;
                    }
                    //set
                    mouseY = cp.Y;
                }
                la_Value.Content = Value;
                //pi_Progressbar.Angle--;
                //pi_Progressbar.Rotation--;
            }
            if (cp.Y > mouseY)
            {
                if (Value > 0)
                {

                    addto += mouseY - cp.Y;
                    if (addto <= -TickDelay)
                    {

                        Value += (int)(addto / TickDelay);
                        addto = 0;
                    }
                    //set
                    mouseY = cp.Y;
                }
                la_Value.Content = Value;
                //pi_Progressbar.Angle--;
                //pi_Progressbar.Rotation--;
            }
            //if (cp.X < mouseX)
            //{
            //    if (Value > 0)
            //    {
            //        addto += (float)1 / ((float)TickDelay + 1);
            //        Value-=(int)addto;
            //    }
            //    la_Value.Content = Value;
            //   // pi_Progressbar.Angle++;
            //   // pi_Progressbar.Rotation++;
            //}

            //Value = -((int)(((pi_Progressbar.Angle-128) / (128-358)) * (double)-Maximum))+Maximum;

            //pi_Progressbar.Angle = ((int)(((_value + 128) / (128 + 358)) * (double)+Maximum)) - Maximum;
            //SetCursorPos(mouseX, mouseY);
            if (Value < 0) Value = 0;
            if (Value > Maximum) Value = Maximum;
        }

        public void SetValueUnsuppressed(int value)
        {
            if (value > Maximum)
            {
                return;
            }
            if (value < 0)
            {
                return;
            }
            suppressValueChange = false;
            Value = value;
        }
        protected virtual void onValueChange(EventArgs e)
        {
            EventHandler<EventArgs> temp = ValueChanged;
            if (temp != null)
            {
                temp(this, e);
            }
        }
        bool suppressValueChange = true;
        public string Text { get { return (string)la_CtrlID.Content; } set { la_CtrlID.Content = value; } }
        public int Value
        {
            get {
                if (_value > _max) { Value = _max; return _value; }
                if (_value < 0) { Value = 0; return _value; }
                return _value;
            }
            set
            {
                _value = value; la_Value.Content = value;
                float percent = (float)_value / (float)Maximum;
                percent = 1 - percent;
                float CalculatedAngle = percent * 230 + 128;

                float CalculatedAngle2 = (CalculatedAngle * 2) + (-CalculatedAngle);
                pi_Progressbar.Angle = (int)CalculatedAngle2;

                CanvasRotateTransform.Angle = -CalculatedAngle2 + 128;
                if (!suppressValueChange)
                {
                    onValueChange(new EventArgs());
                    suppressValueChange = true;
                }
            }
        }
        public int Maximum
        {
            get { return _max; }
            set
            {
                float percent = (float)Value / (float)value;
                percent = 1 - percent;
                float CalculatedAngle = percent * 230 + 128;

                float CalculatedAngle2 = CalculatedAngle;
                pi_Progressbar.Angle = (int)CalculatedAngle2;
                _max = value;
            }
        }
        DispatcherTimer t = new DispatcherTimer();

        int mouseX, mouseY;
        int _value = 0;

        private void el_dial_KeyDown(object sender, KeyEventArgs e)
        {


        }

        private void el_dial_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cursorpos cp = new cursorpos();
            GetCursorPos(out cp);
            mouseX = cp.X;
            mouseY = cp.Y;
            isDown = true;
            t.Start();
            Mouse.OverrideCursor = Cursors.ScrollNS;

        }

        private void el_dial_MouseMove(object sender, MouseEventArgs e)
        {

        }
        bool isDown = false;
        private int _max;
        private void el_dial_MouseUp(object sender, MouseButtonEventArgs e)
        {

            t.Stop();
            isDown = false;
            Mouse.OverrideCursor = this.Cursor;


        }


        public event EventHandler<EventArgs> ValueChanged;

        private void UserControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Color c = IsEnabled ? Color.FromArgb(255, 0, 35, 255) : Color.FromArgb(255, 64, 64, 64);
            Color L = IsEnabled ? Colors.White : Colors.DimGray;
            EL_FillColor.Fill = new SolidColorBrush(c);
            la_Value.Foreground = new SolidColorBrush(L);
            la_CtrlID.Foreground = new SolidColorBrush(L);
        }










    }
}
