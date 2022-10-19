using MidiSynth7.entities;
using MidiSynth7.entities.controls;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            
            bw.DoWork += Bw_DoWork;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            InitializeComponent();

        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            loader.Visibility = Visibility.Collapsed;
            
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            (TrackerSequence sequence, int index) = ((TrackerSequence sequence, int index))e.Argument ;
            BWLoadPattern(sequence, index);
        }

        private MainWindow _win;
        private Grid _container;
        private TrackerSequence ActiveSequence;
        private VirtualizedMPTPattern ActivePattern;
        bool isPatternPlaying = false;
        int currentPatternIndex = 0;
        int activePatternIndex = 0;
        private BackgroundWorker bw = new BackgroundWorker();
        string _ttl = "Sequence Tracker v1.0";
        public string DialogTitle { get => _ttl; set => _ttl = value; }
        public bool HelpRequested { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool CanRequestHelp => false;

        public event EventHandler<DialogEventArgs> DialogClosed;

        public void InvokeHelpRequested(Control target)
        {
            throw new NotImplementedException();
        }
        public void LoadPattern(TrackerSequence sequence, int index = 0)
        {
            
            string ttl = DialogTitle;
            loader.Visibility = Visibility.Visible;
            UpdateLayout();
            currentPatternIndex = index;
            (TrackerSequence sequence, int index) arg = (sequence, index);
            //Populate instruments; This should be moved.
            int prvInst = sequence.SelectedInstrument;
            CB_MPTInstrument.Items.Clear();
            CB_MPTInstrument.Items.Add("No Instrument");
            foreach (var item in sequence.Instruments)
            {
                CB_MPTInstrument.Items.Add(item);
            }
            CB_MPTInstrument.SelectedIndex = prvInst;
            bw.RunWorkerAsync(arg);
        }
        private void BWLoadPattern(TrackerSequence sequence, int index=0 )
        {
            Thread.Sleep(50);//this is just so ui can show the loader. Which is also stupid, but go off for now.
            Dispatcher.Invoke(()=> PatternContainer.Children.Clear());
            
            ActiveSequence = sequence;
            StopMIDI();//just for safe

            Console.WriteLine("Yup");
            Dispatcher.Invoke(() =>
            {
                
                ActivePattern = new VirtualizedMPTPattern(ActiveSequence.Patterns[index],PatternContainer);
                ActivePattern.ActiveRowChanged += ActivePattern_ActiveRowChanged; ;
                PatternContainer.Children.Add(ActivePattern);

                PatternContainer.Margin = new Thickness(0, (PatternScroller.ViewportHeight - 21) / 2, 0, (PatternScroller.ViewportHeight - 21) / 2);
                RowHeadContainer.Margin = new Thickness(0, (PatternScroller.ViewportHeight - 21) / 2, 0, (PatternScroller.ViewportHeight - 21) / 2);
                RowHeadScroller.Padding = new Thickness(0, 0, 0, 21);
                ChannelHeadScroller.Padding = new Thickness(0, 0, SystemParameters.VerticalScrollBarWidth + 2, 0);
                RowHeadContainer.Children.Clear();
                ChannelHeadContainer.Children.Clear();
                //populate row index container
                for (int i = 0; i < ActiveSequence.Patterns[index].RowCount; i++)
                {
                    RowHeadContainer.Children.Add(new RowIndexBit(i));
                }
                //populate ch index container
                for (int i = 0; i < ActiveSequence.Patterns[index].ChannelCount; i++)
                {
                    ChannelHeadContainer.Children.Add(new RowChannelBit(i + 1));
                }
                TBX_SequenceName.Text = ActiveSequence.SequenceName;
                TBX_PatternName.Text = ActiveSequence.Patterns[index].PatternName;
                CTRL_MPTOctave.SetValueSuppressed(ActiveSequence.SelectedOctave);
                LC_PatternSel.SetLight(index);
                
            });
        }

        private void ActivePattern_ActiveRowChanged(object sender, ActiveRowEventArgs e)
        {
            PatternScroller.ScrollToVerticalOffset(((PatternScroller.ViewportHeight - 21) / 2) + (e.selectedIndex * 22) - ((PatternScroller.ViewportHeight - 21) / 2));

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            isPatternPlaying = false;

            await Task.Run(()=>ActiveSequence.SaveSequence());//TODO: someday use something that isn't json.
            PatternContainer.Children.Clear();
            ActivePattern = null;

            DialogClosed?.Invoke(this, new DialogEventArgs(_win, _container));
        }

        private void PatternScroller_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if(e.Key == Key.Up)
            {
                ActivePattern.UpdateRow(ActivePattern.ActiveRow, false);
                ActivePattern.ActiveRow--;
                if (ActivePattern.ActiveRow < 0)
                {
                    //TODO: Load previous pattern
                    ActivePattern.ActiveRow = ActivePattern.RowCount - 1;
                }
                ActivePattern.UpdateRow(ActivePattern.ActiveRow, true);
            }
            
            if (e.Key == Key.Down)
            {
                ActivePattern.UpdateRow(ActivePattern.ActiveRow, false);
                ActivePattern.ActiveRow++;
                if (ActivePattern.ActiveRow > ActivePattern.RowCount - 1)
                {
                    //TODO: Load next pattern
                    ActivePattern.ActiveRow = 0;
                }
                ActivePattern.UpdateRow(ActivePattern.ActiveRow, true);
            }
            if (e.Key == Key.L && Keyboard.Modifiers == ModifierKeys.Control)
            {
                //ActivePattern.SelectActiveChannel();
                return;
            }
            if (e.Key == Key.L && Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                //ActivePattern.SelectActiveChannelBit();
                return;

            }
            if (e.Key == Key.Delete)
            {
                //if (ActivePattern.activeBits?.Count > 1)
                //{
                //    foreach (MPTBit item in ActivePattern.activeBits)
                //    {
                //        item.DelSelection(ActivePattern.GetSelectedBounds(), PatternContainer);
                //    }
                //    return;
                //}
            }

            //int bit = ActivePattern.selectedBit;
            //int ch = ActivePattern.selectedChannel;
            //if (e.Key == Key.Left)
            //{
            //    bit--;
            //}
            //if (e.Key == Key.Right)
            //{
            //    bit++;
            //}
            //if (bit > 4)
            //{
            //    bit = 0;
            //    ch++;
            //    if(ch > ActivePattern.ChannelCount-1)
            //    {
            //        ch = 0;
            //    }
            //}
            //if (bit < 0)
            //{
            //    bit = 4;
            //    ch--;
            //    if (ch < 0)
            //    {
            //        ch = ActivePattern.ChannelCount - 1;
            //    }
            //}
            //ActivePattern.MoveBitActiveRow(ch, bit);
            ////will this actually?
            //Rect selectBit = ActivePattern.ActiveBit.BoundsRelativeTo(ActivePattern);
            //PatternContainer.BringIntoView(selectBit);
            
            
            //if (ActivePattern.ActiveBit != null)
            //{
            //    ActivePattern.ActiveBit.ProcessKey(e.Key, CTRL_MPTOctave.Value,(CB_MPTInstrument.SelectedItem as TrackerInstrument).Index);
            //}
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
            isPatternPlaying = false;
            LoadPattern(ActiveSequence, e.LightIndex);
            activePatternIndex = e.LightIndex;
        }

        private void BN_MPTInsManager_Click(object sender, RoutedEventArgs e)
        {
            Dialog g = new Dialog();
            g.ShowDialog(new MPTInstrumentManager(ActiveSequence, _win, _container), _win, _container,true);
        }

        private void BN_cancel_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Add a confirmation dialog that blocks until a result is returned.
            var mbd = MessageBox.Show("Abandon changes?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if(mbd == MessageBoxResult.Yes)
            {
                isPatternPlaying = false;
                PatternContainer.Children.Clear();
                ActivePattern = null;
                DialogClosed?.Invoke(this, new DialogEventArgs(_win, _container));
            }
            
        }

        private async void Bn_PlayRow_Click(object sender, RoutedEventArgs e)
        {
             await  Task.Run(() =>{
                Dispatcher.Invoke( () => ActivePattern.UpdateRow(ActivePattern.ActiveRow, false,true));
                ActiveSequence.Patterns[currentPatternIndex].Rows[ActivePattern.ActiveRow].Play(ActiveSequence, _win.MidiEngine, null);
                ActivePattern.ActiveRow++;
                if (ActivePattern.ActiveRow > ActiveSequence.Patterns[currentPatternIndex].RowCount - 1)
                {
                //TODO: Load next pattern
                    ActivePattern.ActiveRow = 0;
                }
                Dispatcher.Invoke(() => ActivePattern.UpdateRow(ActivePattern.ActiveRow, true,true));
            });
            await Task.Delay(1);
            
        }

        private void Bn_StopPattern_Click(object sender, RoutedEventArgs e)
        {
            isPatternPlaying = false;
        }

        private void StopMIDI()
        {
            
            //I do need this.
            List<int> devices = ActiveSequence.Instruments.DistinctBy(x => x.DeviceIndex).Select(x => x.DeviceIndex).ToList();
            foreach (int item in devices)
            {
                _win.MidiEngine.MidiEngine_Panic(item);
            }
        }

        private async void Bn_PlayPattern_Click(object sender, RoutedEventArgs e)
        {
            if (isPatternPlaying)
            {
                return;
            }
            isPatternPlaying = true;
            int ticksPerDot = 6; // TODO: Ensure this value is set by the sequence
            int DotDuration = (int)((float)(2500 / (float)(120 * 1000)) * (ticksPerDot * 1000));
            Console.WriteLine("DotDuration:" + DotDuration);
            // Play the selected sequence
            
            await Task.Run(() =>
            {

                while (isPatternPlaying)
                {
                    for (int step = 0; step < ActivePattern.RowCount; step++)
                    {

                        
                        if (!isPatternPlaying)
                        {
                            StopMIDI();
                            return;
                        }
                        int pstep = step - 1; if (pstep < 0) pstep = ActivePattern.RowCount - 1;
                        Dispatcher.InvokeAsync(() => ActivePattern.UpdateRow(pstep, false),System.Windows.Threading.DispatcherPriority.Render);
                        Dispatcher.InvokeAsync(() => ActivePattern.UpdateRow(step, true), System.Windows.Threading.DispatcherPriority.Render);
                        _win.ActiveSequence.Patterns[activePatternIndex].Rows[step].Play(_win.ActiveSequence, _win.MidiEngine, null);
                        //TODO: Further process the sequence parameters within it.
                        
                        Thread.Sleep(DotDuration);//This is beyond not ideal LOL
                    }

                }

            });
        }
    }
}
