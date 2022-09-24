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

namespace MidiSynth7.components.dialog
{
    /// <summary>
    /// Interaction logic for SequenceEditor.xaml
    /// </summary>
    public partial class SequenceEditor : Page, IDialogView
    {
        public SequenceEditor()
        {
            InitializeComponent();
        }

        public string DialogTitle { get => "Sequence Tracker v1.0"; set => throw new NotImplementedException(); }
        public bool HelpRequested { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool CanRequestHelp => false;

        public event EventHandler<DialogEventArgs> DialogClosed;

        public void InvokeHelpRequested(Control target)
        {
            throw new NotImplementedException();
        }

        private void MPTPattern_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
