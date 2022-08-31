using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
namespace MidiSynth7.entities.controls
{
    /// <summary>
    /// A Dial control is key to a cool looking synth.
    /// </summary>
    public partial class DialControl : UserControl
    {
        #region Fields
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
          "Maximum",
          typeof(int),
          typeof(DialControl),
          new PropertyMetadata(255, new PropertyChangedCallback(DC_DPChanged)));
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
          "Minimum",
          typeof(int),
          typeof(DialControl),
          new PropertyMetadata(0, new PropertyChangedCallback(DC_DPChanged)));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
          "Value",
          typeof(int),
          typeof(DialControl),
          new PropertyMetadata(0, new PropertyChangedCallback(DC_DPChanged)));

        private DispatcherTimer t = new DispatcherTimer();

        private bool isDown = false;
        private bool suppressValueChange = true;
        #endregion

        public DialControl()
        {
            InitializeComponent();
            t.Interval = TimeSpan.FromMilliseconds(1);
            t.Tick += Tick;
            tx_Value.IsHitTestVisible = false;
            tx_Value.Visibility = Visibility.Collapsed;
            la_Value.IsHitTestVisible = false;

        }

        #region Delagates
        private static void DC_DPChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DialControl i = d as DialControl;
            if (i == null) { return; };
            float percent = ((float)i.Value - i.Minimum) / ((float)i.Maximum - i.Minimum);
            percent = 1 - percent;
            float CalculatedAngle = (percent * 230) + 128;
            float CalculatedAngle2 = (CalculatedAngle * 2) + (-CalculatedAngle);
            i.pi_Progressbar.Angle = (int)CalculatedAngle2;
            i.CanvasRotateTransform.Angle = -CalculatedAngle2 + 128;
            i.la_Value.Content = i.Value;
        }

        private void Tick(object sender, EventArgs e)
        {
            isDown = GetAsyncKeyState(0x01) < 0;
            if (!isDown)
            {
                t.Stop();
                Mouse.OverrideCursor = Cursor;
            }
            if (!isDown)
            {
                return;
            }
            GetCursorPos(out cursorpos cp);
            SetValueUnsuppressed(GetValueFromPosition(new Point(cp.X, cp.Y)));
        }

        private void el_dial_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GetCursorPos(out cursorpos cp);
            isDown = true;
            t.Start();
            Mouse.OverrideCursor = Cursors.Cross;

        }

        private void el_dial_MouseUp(object sender, MouseButtonEventArgs e)
        {
            t.Stop();
            isDown = false;
            Mouse.OverrideCursor = Cursor;
        }

        private void el_dial_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!tx_Value.IsHitTestVisible)
            {
                tx_Value.IsHitTestVisible = true;
                tx_Value.Text = Value.ToString();
                tx_Value.Visibility = Visibility.Visible;
                tx_Value.IsEnabled = true;
                tx_Value.Focusable = true;
                tx_Value.Focus();
                tx_Value.SelectAll();
                InputCaptured = true;
                TextPromptStateChanged?.Invoke(this, new EventArgs());
            }
        }

        private void tx_Value_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                if (!int.TryParse(tx_Value.Text, out int v))
                {
                    System.Media.SystemSounds.Hand.Play();//wow OG shit right here
                    tx_Value.Text = Value.ToString();
                    tx_Value.SelectAll();
                    return;
                }
                else
                {
                    if (v < Minimum || v > Maximum)
                    {
                        System.Media.SystemSounds.Hand.Play();//wow OG shit right here
                        tx_Value.Text = Value.ToString();
                        tx_Value.SelectAll();

                        return;
                    }
                    SetValueUnsuppressed(v);//why do I do the things I do?
                }
                tx_Value.Visibility = Visibility.Collapsed;
                tx_Value.Focusable = false;
                tx_Value.IsEnabled = false;
                tx_Value.IsHitTestVisible = false;
                InputCaptured = false;
                TextPromptStateChanged?.Invoke(this, new EventArgs());
            }
        }

        private void UserControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Color c = IsEnabled ? Color.FromArgb(255, 0, 35, 255) : Color.FromArgb(255, 64, 64, 64);
            Color L = IsEnabled ? Colors.White : Colors.DimGray;
            EL_FillColor.Fill = new SolidColorBrush(c);
            la_Value.Foreground = new SolidColorBrush(L);
            la_CtrlID.Foreground = new SolidColorBrush(L);
        }

        #endregion

        #region Events
        public event EventHandler<EventArgs> ValueChanged;
        public event EventHandler<EventArgs> TextPromptStateChanged;
        #endregion

        #region Properties
        public int Maximum
        {
            get => (int)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }
        public int Minimum
        {
            get => (int)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }
        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set
            {
                if (value > Maximum) value = Maximum;
                if (value < Minimum) value = Minimum;
                SetValue(ValueProperty, value);
                if (!suppressValueChange)
                {
                    ValueChanged?.Invoke(this, new EventArgs());
                    suppressValueChange = true;
                }
            }
        }
        public string Text { get { return (string)la_CtrlID.Content; } set { la_CtrlID.Content = value; } }
        public bool InputCaptured { get; private set; } = false;
        #endregion

        #region P/invoke Methods
        [DllImport("user32.dll")]
        static extern void SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        static extern void GetCursorPos(out cursorpos lpoint);

        [DllImport("user32.dll")]
        static extern void ClipCursor(ref System.Drawing.Rectangle rect);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(UInt16 virtualKeyCode);

        #endregion

        #region Public Methods
        /// <summary>
        /// Modify the value and trigger the value changed event.
        /// </summary>
        /// <param name="value"></param>
        public void SetValueUnsuppressed(int value)
        {
            if (value > Maximum)
            {
                return;
            }
            if (value < Minimum)
            {
                return;
            }
            suppressValueChange = false;
            Value = value;
        }

        /// <summary>
        /// Modify the value without triggering the value changed event.
        /// </summary>
        /// <param name="value"></param>
        public void SetValueSuppressed(int value)
        {
            if (value > Maximum)
            {
                return;
            }
            if (value < Minimum)
            {
                return;
            }
            suppressValueChange = true;
            Value = value;
        }
        #endregion

        /// <summary>
        /// converts geometrical position into value.
        /// Heavily modified code from Jigar Desai on C-SharpCorner.com
        /// </summary>
        /// <param name="p">Point that is to be converted</param>
        /// <returns>Value derived from position</returns>
        private int GetValueFromPosition(Point p)
        {
            float degree = 0;
            Point screenpt = el_dial.PointToScreen(new Point(el_dial.ActualWidth/2, el_dial.ActualHeight/2));

            //DEBUGGING POSITIONS
            //System.Drawing.Graphics g;
            //using (g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))

            //{
            //    g.DrawEllipse(System.Drawing.Pens.Red, (int)screenpt.X - 10, (int)screenpt.Y - 10, 20, 20);
            //    g.DrawEllipse(System.Drawing.Pens.Red, (int)screenpt.X, (int)screenpt.Y, 1, 1);
            //    g.DrawEllipse(System.Drawing.Pens.Red, (int)p.X - 10, (int)p.Y - 10, 20, 20);
            //}
            if (p.X <= screenpt.X)
            {
                degree = (float)(screenpt.Y - p.Y) / (float)(screenpt.X - p.X);
                degree = (float)Math.Atan(degree);

                degree = (degree * (float)(180 / Math.PI)) + (180 - 156); // Why is 156 the magic number here?

            }
            else if (p.X > screenpt.X)
            {
                degree = (float)(p.Y - (screenpt.Y)) / (float)(p.X - (screenpt.X));
                degree = (float)Math.Atan(degree);

                degree = (degree * (float)(180 / Math.PI)) + 360 - 156; // Why is 156 the magic number?
            }

            int v = Minimum + (int)Math.Round(degree * (Maximum - Minimum) / 230); // Why is 230 my magic number here?
            if (v > Maximum) v = Maximum;
            if (v < Minimum) v = Minimum;
            return v;
        }

        #region Structs
        struct cursorpos
        {
            public int X, Y;
        }
        #endregion

    }
}
