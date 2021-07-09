using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MidiSynth7.components;
namespace MidiSynth7
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Declarations

        private bool _Closing = false;
        private bool _Minimized = false;

        private Storyboard WindowStoryboard;
        private DoubleAnimation WindowDoubleAnimation;
        private List<NumberedEntry> OutputDevices = new List<NumberedEntry>();
        private List<NumberedEntry> InputDevices = new List<NumberedEntry>();

        #endregion
        public MainWindow()
        {
            InitializeComponent();
        }

        #region External API
        public const int WM_CLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hw, int message, int wp, int lp);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        #endregion

        private void GR_Title_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SendMessage(new WindowInteropHelper(this).Handle, WM_CLBUTTONDOWN, HT_CAPTION, 0);
            e.Handled = false;
        }

        private void XButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _Closing = true;
                WindowDoubleAnimation.From = 1;
                WindowDoubleAnimation.To = 0;
                WindowStoryboard.Begin();
                WindowScale(1, 0.8);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Bn_minimize_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //WindowState = System.Windows.WindowState.Minimized;
                _Closing = false;
                _Minimized = true;
                WindowDoubleAnimation.From = 1;
                WindowDoubleAnimation.To = 0;
                WindowStoryboard.Begin();
                WindowScale(1, 0.8);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Bn_SETTINGS_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Bn_Maximize_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MIO_bn_about_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Initialized(object sender, EventArgs e)
        {

            WindowStoryboard = new Storyboard();
            WindowDoubleAnimation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(240)), FillBehavior.Stop);
            WindowDoubleAnimation.AutoReverse = false;
            QuadraticEase qe = new QuadraticEase();
            qe.EasingMode = EasingMode.EaseInOut;
            WindowDoubleAnimation.EasingFunction = qe;
            Storyboard.SetTargetProperty(WindowStoryboard, new PropertyPath(OpacityProperty));
            Storyboard.SetTarget(WindowDoubleAnimation, this);
            WindowStoryboard.Children.Add(WindowDoubleAnimation);
            WindowStoryboard.Completed += WindowStoryboard_Completed;
            
        }

        private void WindowStoryboard_Completed(object sender, EventArgs e)
        {
            if (_Closing)
            {
                this.Close();
            }
            if (_Minimized)
            {

                this.WindowState = System.Windows.WindowState.Minimized;
                WindowScale(1, 1);
                this.Opacity = 1;

            }
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowScale(0.8, 1);
            WindowStoryboard.Begin();
        }

        private void WindowScale(double from, double to)
        {
            ScaleTransform trans = new ScaleTransform();
            this.RenderTransform = trans;
            this.RenderTransformOrigin = new Point(0.5d, 0.5d);
            
            if(from != to)
            {
                DoubleAnimation anim = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(240));
                CubicEase ease = new CubicEase();
                ease.EasingMode = EasingMode.EaseInOut;
                anim.EasingFunction = ease;
                trans.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                trans.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
            }
            
        }

        private void window_StateChanged(object sender, EventArgs e)
        {
            if(window.WindowState != WindowState.Minimized)
            {
                _Minimized = false;
                WindowDoubleAnimation.From = 0;
                WindowDoubleAnimation.To = 1;
                WindowStoryboard.Begin();
                WindowScale(0.8, 1);
            }
        }
    }
}
