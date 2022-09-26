using MidiSynth7.entities.controls;
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
        public SequenceEditor(MainWindow win, Grid container)
        {
            _win = win;
            _container = container;
            InitializeComponent();
            
        }
        MainWindow _win;
        Grid _container;

        MPTPattern ActivePattern;
        string _ttl = "Sequence Tracker v1.0";
        public string DialogTitle { get => _ttl; set => _ttl = value; }
        public bool HelpRequested { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool CanRequestHelp => false;

        public event EventHandler<DialogEventArgs> DialogClosed;

        public void InvokeHelpRequested(Control target)
        {
            throw new NotImplementedException();
        }

        public void LoadPattern(int index=0 )
        {
            string ttl = DialogTitle;
            loader.Visibility = Visibility.Visible;
            UpdateLayout();
            PatternContainer.Children.Clear();

            Console.WriteLine("Yup");
            ActivePattern = new MPTPattern(16,4,TrackerPattern.GetEmptyPattern(32,20));
            ActivePattern.PatternSelectionChange += ActivePattern_PatternSelectionChange;
            PatternContainer.Children.Add(ActivePattern);
            loader.Visibility = Visibility.Collapsed;
            PatternContainer.Margin = new Thickness(0, (PatternScroller.ViewportHeight-21) / 2, 0, (PatternScroller.ViewportHeight-21) / 2);

        }

        private void ActivePattern_PatternSelectionChange(object sender, SelectionEventArgs e)
        {
            PatternScroller.ScrollToVerticalOffset(((PatternScroller.ViewportHeight - 21) / 2) + (e.SelectedIndex * 21) - ((PatternScroller.ViewportHeight - 21) / 2));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PatternContainer.Children.Clear();
            ActivePattern = null;

            DialogClosed?.Invoke(this, new DialogEventArgs(_win, _container));
        }

        private void PatternScroller_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if(e.Key == Key.Up)
            {
                ActivePattern.ActiveRowIndex--;
                if(ActivePattern.ActiveRowIndex < 0)
                {
                    //TODO: Load previous pattern
                    ActivePattern.ActiveRowIndex = ActivePattern.RowCount - 1;
                }
                ActivePattern.SelectRow(ActivePattern.ActiveRowIndex);
            }
            if (e.Key == Key.Down)
            {
                ActivePattern.ActiveRowIndex++;
                if (ActivePattern.ActiveRowIndex > ActivePattern.RowCount - 1)
                {
                    //TODO: Load previous pattern
                    ActivePattern.ActiveRowIndex = 0;
                }
                ActivePattern.SelectRow(ActivePattern.ActiveRowIndex);
            }
        }
    }
}
