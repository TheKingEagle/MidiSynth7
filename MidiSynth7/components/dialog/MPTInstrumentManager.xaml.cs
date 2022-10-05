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
    /// Interaction logic for MPTInstrumentManager.xaml
    /// </summary>
    public partial class MPTInstrumentManager : Page, IDialogView
    {
        public MPTInstrumentManager()
        {
            InitializeComponent();
        }

        MainWindow AppContext;
        Grid Overlay;
        TrackerSequence Sequence;

        public MPTInstrumentManager(TrackerSequence ts, MainWindow _AppContext, Grid _Container)
        {
            Sequence = ts;
            AppContext = _AppContext;
            Overlay = _Container;
            InitializeComponent();
        }

        public string DialogTitle { get => "Allocate Instruments"; set => throw new NotImplementedException(); }
        public bool HelpRequested { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool CanRequestHelp => false;

        public event EventHandler<DialogEventArgs> DialogClosed;

        public void InvokeHelpRequested(Control target)
        {
            throw new NotImplementedException();
        }

        private void bn_SaveInsMap_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Save.
            DialogClosed?.Invoke(this, new DialogEventArgs(AppContext, Overlay));
        }
    }
}
