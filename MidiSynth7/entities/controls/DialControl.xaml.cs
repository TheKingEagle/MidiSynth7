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
        private int mouseY;
        private bool suppressValueChange = true;
        private float addto = 0;
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
            float CalculatedAngle = percent * 230 + 128;
            float CalculatedAngle2 = (CalculatedAngle * 2) + (-CalculatedAngle);
            i.pi_Progressbar.Angle = (int)CalculatedAngle2;
            i.CanvasRotateTransform.Angle = -CalculatedAngle2 + 128;
            i.la_Value.Content = i.Value;
        }

        private void Tick(object sender, EventArgs e)
        {
            if (TickDelay < 1)
            {
                TickDelay = 1;
            }
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
            suppressValueChange = false;
            GetCursorPos(out cursorpos cp);
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
                    mouseY = cp.Y;
                }
                la_Value.Content = Value;
            }
            if (cp.Y > mouseY)
            {
                if (Value > Minimum)
                {
                    addto += mouseY - cp.Y;
                    if (addto <= -TickDelay)
                    {
                        Value += (int)(addto / TickDelay);
                        addto = 0;
                    }
                    mouseY = cp.Y;
                }
                la_Value.Content = Value;

            }
            if (Value < Minimum) Value = Minimum;
            if (Value > Maximum) Value = Maximum;
        }

        private void el_dial_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GetCursorPos(out cursorpos cp);
            mouseY = cp.Y;
            isDown = true;
            t.Start();
            Mouse.OverrideCursor = Cursors.ScrollNS;

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
        public int TickDelay { get; set; }
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
        #endregion

        #region Structs
        struct cursorpos
        {
            public int X, Y;
        }
        #endregion
    }
}
