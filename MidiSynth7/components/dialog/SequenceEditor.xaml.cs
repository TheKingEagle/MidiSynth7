using MidiSynth7.entities;
using MidiSynth7.entities.controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        private MainWindow _win;
        private Grid _container;
        private TrackerSequence ActiveSequence;
        private MPTPattern ActivePattern;
        string _ttl = "Sequence Tracker v1.0";
        public string DialogTitle { get => _ttl; set => _ttl = value; }
        public bool HelpRequested { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool CanRequestHelp => false;

        public event EventHandler<DialogEventArgs> DialogClosed;

        public void InvokeHelpRequested(Control target)
        {
            throw new NotImplementedException();
        }

        public void LoadPattern(TrackerSequence sequence, int index=0 )
        {
            PatternContainer.Children.Clear();
            string ttl = DialogTitle;
            loader.Visibility = Visibility.Visible;
            UpdateLayout();
            ActiveSequence = sequence;
            

            Console.WriteLine("Yup");
            ActivePattern = new MPTPattern(ActiveSequence.Patterns[index], PatternContainer);
            ActivePattern.PatternSelectionChange += ActivePattern_PatternSelectionChange;
            PatternContainer.Children.Add(ActivePattern);
            loader.Visibility = Visibility.Collapsed;
            PatternContainer.Margin = new Thickness(0, (PatternScroller.ViewportHeight- 21) / 2, 0, (PatternScroller.ViewportHeight- 21) / 2);
            RowHeadContainer.Margin = new Thickness(0, (PatternScroller.ViewportHeight-21) / 2, 0, (PatternScroller.ViewportHeight-21) / 2);
            RowHeadScroller.Padding = new Thickness(0, 0, 0, 21);
            ChannelHeadScroller.Padding = new Thickness(0, 0, SystemParameters.VerticalScrollBarWidth+2, 0);
            RowHeadContainer.Children.Clear();
            ChannelHeadContainer.Children.Clear();
            //populate row index container
            for (int i = 0; i < ActivePattern.RowCount; i++)
            {
                RowHeadContainer.Children.Add(new RowIndexBit(i));
            }
            //populate ch index container
            for (int i = 0; i < ActivePattern.ChannelCount; i++)
            {
                ChannelHeadContainer.Children.Add(new RowChannelBit(i+1));
            }
            TBX_SequenceName.Text = ActiveSequence.SequenceName;
            TBX_PatternName.Text = ActiveSequence.Patterns[index].PatternName;
            CTRL_MPTOctave.SetValueSuppressed(ActiveSequence.SelectedOctave);
            LC_PatternSel.SetLight(index);

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
                ActivePattern.SetHotRow(ActivePattern.ActiveRowIndex);
            }
            
            if (e.Key == Key.Down)
            {
                ActivePattern.ActiveRowIndex++;
                if (ActivePattern.ActiveRowIndex > ActivePattern.RowCount - 1)
                {
                    //TODO: Load next pattern
                    ActivePattern.ActiveRowIndex = 0;
                }
                ActivePattern.SetHotRow(ActivePattern.ActiveRowIndex);
            }
            if(e.Key == Key.L && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ActivePattern.SelectActiveChannel();
                return;
            }
            if (e.Key == Key.L && Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                ActivePattern.SelectActiveChannelBit();
                return;

            }
            if (e.Key == Key.Delete)
            {
                if (ActivePattern.activeBits != null)
                {
                    foreach (MPTBit item in ActivePattern.activeBits)
                    {
                        item.DelSelection(ActivePattern.GetSelectedBounds(), PatternContainer);
                    }
                    return;
                }
            }

            int bit = ActivePattern.selectedBit;
            int ch = ActivePattern.selectedChannel;
            if (e.Key == Key.Left)
            {
                bit--;
            }
            if (e.Key == Key.Right)
            {
                bit++;
            }
            if (bit > 4)
            {
                bit = 0;
                ch++;
                if(ch > ActivePattern.ChannelCount-1)
                {
                    ch = 0;
                }
            }
            if (bit < 0)
            {
                bit = 4;
                ch--;
                if (ch < 0)
                {
                    ch = ActivePattern.ChannelCount - 1;
                }
            }
            ActivePattern.MoveBitActiveRow(ch, bit);
            //will this actually?
            Rect selectBit = ActivePattern.ActiveBit.BoundsRelativeTo(ActivePattern);
            PatternContainer.BringIntoView(selectBit);
            
            
            if (ActivePattern.ActiveBit != null)
            {
                ActivePattern.ActiveBit.ProcessKey(e.Key, CTRL_MPTOctave.Value);
            }
        }

        private void Scroller_Scrolled(object sender, ScrollChangedEventArgs e)
        {
            if(sender == PatternScroller)
            {
                RowHeadScroller.ScrollToVerticalOffset(e.VerticalOffset);
                ChannelHeadScroller.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
            
        }

        private void RowHeadScroller_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void RowHeadScroller_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        private void RowHeadScroller_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void LC_PatternSel_LightIndexChanged(object sender, LightCellEventArgs e)
        {
            LoadPattern(ActiveSequence, e.LightIndex);
        }

        private void BN_MPTInsManager_Click(object sender, RoutedEventArgs e)
        {
            Dialog g = new Dialog();
            g.ShowDialog(new MPTInstrumentManager(ActiveSequence, _win, _container), _win, _container,true);
        }
    }
}
