using MidiSynth7.components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace MidiSynth7.entities.controls
{
    /// <summary>
    /// Interaction logic for MPTPattern.xaml
    /// </summary>
    public partial class MPTPattern : UserControl
    {
        private int _channelCount = 5;
        private int _rowCount = 32;
        private int _rowsPerBeat = 4;
        private int _rowsPerMeasure = 16;
        private SolidColorBrush bg_subdivision1 = new SolidColorBrush(Color.FromArgb(255, 24, 30, 40));
        private SolidColorBrush bg_subdivision2 = new SolidColorBrush(Color.FromArgb(255, 20, 26, 34));
        private SolidColorBrush bg_subdivisionN = new SolidColorBrush(Color.FromArgb(255, 12, 16, 20));
        
        private MPTRow ActiveRow;
        public int ActiveRowIndex = 0;

        TrackerPattern tpf = new TrackerPattern();
        SynchronizationContext uiContext = SynchronizationContext.Current;
        List<MPTRow> mptRows = new List<MPTRow>();
        BackgroundWorker bw = new BackgroundWorker();

        public event EventHandler<SelectionEventArgs> PatternSelectionChange;

        [Category("MPT")]
        public int ChannelCount
        {
            get => _channelCount;
            set
            {
                _channelCount = value;
            }
        }
        [Category("MPT")]
        public int RowCount
        {
            get => _rowCount;
            private set
            {
                _rowCount = value;

            }
        }
        public void SelectRow(int index)
        {
            ActiveRow.UpdateFocus(false);
            ActiveRow = mptRows.FirstOrDefault(x => x.RowIndex == index);
            ActiveRow.UpdateFocus(true);
            Dispatcher.Invoke(()=>PatternSelectionChange?.Invoke(this, new SelectionEventArgs(index)));
        }
        [Category("MPT")]
        public int RowsPerBeat
        {
            get => _rowsPerBeat;
            private set
            {
                _rowsPerBeat = value;
            }
        }
        [Category("MPT")]

        public int RowsPerMeasure
        {
            get => _rowsPerMeasure;
            private set
            {
                _rowsPerMeasure = value;
            }
        }
        public MPTPattern()
        {
            InitializeComponent();
        }

        public MPTPattern(int rowsPerMeasure,int rowsPerBeat,TrackerPattern pattern)
        {
            tpf = pattern;
            InitializeComponent();
            rowContainer.Visibility = Visibility.Collapsed;
            RowCount = tpf.RowCount;
            ChannelCount = tpf.ChannelCount;
            RowsPerBeat = rowsPerBeat;
            RowsPerMeasure = rowsPerMeasure;
            bw.DoWork += Bw_DoWork;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            bw.RunWorkerAsync();
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Dispatcher.Invoke(() => rowContainer.ItemsSource = mptRows);
            Dispatcher.Invoke(() => rowContainer.Visibility = Visibility.Visible);

        }
        bool locker = false;
        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            void threader()
            {
                int id = 0;
                foreach (TrackerRow item in tpf.Rows)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var row = new MPTRow(id, ChannelCount, item);
                        row.MouseLeftButtonUp += (object s, MouseButtonEventArgs m)=>{
                            if(ActiveRow != null)
                            {
                                Dispatcher.Invoke(() => ActiveRow.UpdateFocus(false));
                            }
                            ActiveRow = (MPTRow)s;
                            ActiveRowIndex = ActiveRow.RowIndex;
                            
                            Dispatcher.Invoke(() => ActiveRow.UpdateFocus(true));
                            Dispatcher.Invoke(() => PatternSelectionChange?.Invoke(this, new SelectionEventArgs(ActiveRow.RowIndex)));
                        };
                        row.Background = bg_subdivisionN;
                        if (id % RowsPerBeat == 0)
                        {
                            row.Background = bg_subdivision2;
                        }
                        if (id % RowsPerMeasure == 0)
                        {
                            row.Background = bg_subdivision1;
                        }

                        mptRows.Add(row);
                        id++;
                    });

                }
                locker = true;
            }
            Thread t = new Thread(threader);
            t.SetApartmentState(ApartmentState.STA);//good god.
            t.Start();
            SpinWait.SpinUntil(() => locker);//even more good god...
        }
        
        
    }

    public class SelectionEventArgs : EventArgs
    {
        public int SelectedIndex { get; private set; }
        public SelectionEventArgs(int selectedIndex)
        {
            SelectedIndex = selectedIndex; 
        }
    }
}
