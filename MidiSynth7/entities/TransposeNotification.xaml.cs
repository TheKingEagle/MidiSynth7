using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        void displayTimer_Tick(object sender, EventArgs e)
        {
            showing = false;
            displayTimer.Stop();
            Visibility = System.Windows.Visibility.Hidden;
        }

        public void Show(int transpose)
        {
            string prefix = (transpose > 0) ? "+" : "";
            la_indicator.Content = prefix+transpose.ToString();
            //if (!showing)
            //{
            Visibility = System.Windows.Visibility.Visible;
            displayTimer.Stop();
            displayTimer.Start();
            
            showing = true;
            //}
        }
    }
}
