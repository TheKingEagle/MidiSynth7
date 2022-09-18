using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MidiSynth7.components
{
    public interface IDialogView
    {
        string DialogTitle { get; set; }

        bool HelpRequested { get; set; }

        event EventHandler<DialogEventArgs> DialogClosed;

        void InvokeHelpRequested(Control target);
    }


    public class DialogEventArgs : EventArgs
    {
        public MainWindow Window { get; private set; }

        public Grid Container { get; private set; }


        public DialogEventArgs(MainWindow win, Grid container)
        {
            Window = win;
            Container = container;
            
        }
    }
}
