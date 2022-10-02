using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
namespace MidiSynth7.entities
{
    /// <summary>
    /// Interaction logic for TransposeNotification.xaml
    /// </summary>
    public partial class TransposeNotification : UserControl
    {
        DispatcherTimer displayTimer;
        public TransposeNotification()
        {
            InitializeComponent();
            //Visibility = System.Windows.Visibility.Hidden;
            displayTimer = new DispatcherTimer();
            displayTimer.Interval = TimeSpan.FromSeconds(2);
            displayTimer.Stop();
            displayTimer.Tick += displayTimer_Tick;
            
        }
        bool showing = false;
        bool ticked = false;
        void displayTimer_Tick(object sender, EventArgs e)
        {
            if(showing && !ticked)
            {
                ticked = true;
                showing = false;
                displayTimer.Stop();
                FadeUI(1, 0, this);
            }
        }

        public void Show(int transpose)
        {
            ticked = false;
            string prefix = (transpose > 0) ? "+" : "";
            la_indicator.Text = prefix+transpose.ToString();
            if (!showing)  FadeUI(0, 1, this);
            showing = true;
            if(showing)
            {
                displayTimer.Stop();
                displayTimer.Start();
            }
        }

        private void FadeUI(double from, double to, UIElement uielm)
        {
            //_elementFromanim = uielm;
            if (uielm.Visibility != Visibility.Visible)
            {
                uielm.Opacity = 0;
                uielm.Visibility = Visibility.Visible;
            }
            Storyboard UIStoryboard = new Storyboard();
            DoubleAnimation WindowDoubleAnimation = new DoubleAnimation(from, to, new Duration(TimeSpan.FromMilliseconds(120)), FillBehavior.HoldEnd);
            WindowDoubleAnimation.AutoReverse = false;
            QuadraticEase qe = new QuadraticEase();
            qe.EasingMode = EasingMode.EaseInOut;
            WindowDoubleAnimation.EasingFunction = qe;
            Storyboard.SetTargetProperty(UIStoryboard, new PropertyPath(OpacityProperty));
            Storyboard.SetTarget(WindowDoubleAnimation, uielm);
            UIStoryboard.Children.Add(WindowDoubleAnimation);
            UIStoryboard.Completed += UIStoryboard_Completed;
            UIStoryboard.Begin((FrameworkElement)uielm, HandoffBehavior.Compose);
        }

        private void UIStoryboard_Completed(object sender, EventArgs e)
        {
            if (!showing && Opacity == 0)
            {
                Visibility = Visibility.Collapsed;
                this.Focusable = false;
            }
            if(showing && !ticked) displayTimer.Start();
        }
    }
}
