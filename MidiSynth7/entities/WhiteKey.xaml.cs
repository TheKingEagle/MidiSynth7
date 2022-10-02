using MidiSynth7.components;
using MidiSynth7.entities.controls;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace MidiSynth7.entities
{
    /// <summary>
    /// Interaction logic for WhiteKey.xaml
    /// </summary>
    public partial class WhiteKey : UserControl, ISynthKey
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
        public void SendOn()
        {
            border.Background = (Brush)this.TryFindResource("ONBrush");
            if (!down)
            {
                down = true;

                OnInvokeKeyDown(new PKeyEventArgs(KeyID));

            }
        }

        public void SendOff()
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
            down = false;
        }
        public void FSendOn()
        {
            if (down) return;
            border.Background = (Brush)this.TryFindResource("ONBrush");
            down = true;
        }
        public void FSendOnA()
        {
            if (down) return;
            border.Background = (Brush)this.TryFindResource("ALTONBrush");
            down = true;
        }
        public void FSendOnC(Brush background)
        {
            if (down) return;
            border.Background = background;
            down = true;

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
