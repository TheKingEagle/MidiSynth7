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
using MidiSynth7.entities.controls;
namespace MidiSynth7.entities
{
    /// <summary>
    /// Interaction logic for WhiteKey.xaml
    /// </summary>
    public partial class WhiteKey : UserControl
    {
        public int KeyID { get; set; }
        public string NoteText { get; set; }

        public event EventHandler<PKeyEventArgs> VKeyUp;
        bool down = false;
        public event EventHandler<PKeyEventArgs> VKeyDown;

        public WhiteKey()
        {
            InitializeComponent();
            
        }
        protected virtual void OnInvokeKeyUp(PKeyEventArgs e)
        {
            VKeyUp?.Invoke(this, e);
        }
        protected virtual void OnInvokeKeyDown(PKeyEventArgs e)
        {
            VKeyDown?.Invoke(this, e);
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
            border.Background = (Brush)this.TryFindResource("ONBrush"); 
            OnInvokeKeyDown(new PKeyEventArgs(KeyID));
        }
        public void SendOn(bool InvokeEvent)
        {
            //border.Background = (Brush)this.TryFindResource("ONBrush");
            if (!down)
            {
                down = true;

                OnInvokeKeyDown(new PKeyEventArgs(KeyID));

            }
        }

        public void SendOff(bool InvokeEvent)
        {
            border.Background = (Brush)this.TryFindResource("OFFBrush");

            OnInvokeKeyUp(new PKeyEventArgs(KeyID));

            if (down)
            {
                down = false;
            }
        }
        public void FSendOff()
        {
            border.Background = (Brush)this.TryFindResource("OFFBrush");
            if (down)
            {
                
            }
           
        }
        public void FSendOn()
        {border.Background = (Brush)this.TryFindResource("ONBrush");
            if (!down)
            {
                
                
            }

        }
        public void FSendOnA()
        {
            border.Background = (Brush)this.TryFindResource("ALTONBrush");

        }
        public void FSendOnC(Brush background)
        {
            border.Background = background;

        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            border.Background = (Brush)this.TryFindResource("OFFBrush");
            OnInvokeKeyUp(new PKeyEventArgs(KeyID));
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                border.Background = (Brush)this.TryFindResource("OFFBrush");
                OnInvokeKeyUp(new PKeyEventArgs(KeyID));
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                border.Background = (Brush)this.TryFindResource("ONBrush");
                OnInvokeKeyDown(new PKeyEventArgs(KeyID));

            }
        }
        public void SetLetter(string NoteID)
        {
            cp_NoteID.SetValue(ContentProperty, (string)NoteID);
        }
    }
}
