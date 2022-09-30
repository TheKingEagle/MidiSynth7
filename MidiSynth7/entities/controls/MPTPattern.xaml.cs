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

//===================== NOTES =====================
// This is not great code. Please enjoy at your
// own risk. I cannot take responsibility for any
// injuries that may occur physically or mentally.
//===================== You've been warned ========

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
        Point SelPoint1 = new Point(0, 0);
        Point SelPoint2 = new Point(0, 0);
        bool mouseDowned = false;
        bool multiSelect = false;
        int selectedBit = 0;
        int selectedChannel = 0;
        TrackerPattern tpf = new TrackerPattern();
        SynchronizationContext uiContext = SynchronizationContext.Current;
        List<MPTRow> mptRows = new List<MPTRow>();
        BackgroundWorker bw = new BackgroundWorker();
        FrameworkElement Frame;

        /// <summary>
        /// The width of the row's number slot.
        /// </summary>
        private int MPTXOffset
        {
            get
            {
                if (mptRows != null)
                {
                    if (mptRows.Count > 0)
                    {
                        return (int)mptRows[0].bd_indx.ActualWidth;
                    }
                }
                return 0;
            }
        }

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

        public void GetSelection(Rect bounds, bool intendsMulti = false)
        {
            foreach (var selcl in rowContainer.Items.OfType<MPTRow>())
            {
                selcl.ClearSelection();
            }
            var f = rowContainer.Items.OfType<MPTRow>().Where(x => x.BoundsRelativeTo(Frame).IntersectsWith(bounds));
            foreach (var item in f)
            {
                item.GetSelection(bounds, Frame,intendsMulti);
                if (!intendsMulti)
                {
                    selectedChannel = item.SelectedChannel;
                    selectedBit = item.SelectedBit;
                }
            }
            //update active row; so we don't get font color misses
            var ar = rowContainer.Items.OfType<MPTRow>().FirstOrDefault(x => x.Active);
            if(ar != null) ar.UpdateFocus(ar.Active);

        }

        public void SetActiveRow(int index)
        {
            ActiveRow.UpdateFocus(false);
            ActiveRow = mptRows.FirstOrDefault(x => x.RowIndex == index);
            ActiveRow.UpdateFocus(true);
            Dispatcher.Invoke(()=>PatternSelectionChange?.Invoke(this, new SelectionEventArgs(index)));
        }

        public void SelectActiveChannel()
        {
            SelPoint1 = new Point((126 * selectedChannel) + MPTXOffset, 0);
            SelPoint2 = new Point((126 * selectedChannel) + 126+ MPTXOffset, 21*RowCount);
            Point Dp1 = PointToScreen(SelPoint1);
            Point Dp2 = PointToScreen(SelPoint2);
            Rect dr = new Rect(Dp1, Dp2);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
            {
                g.DrawRectangle(System.Drawing.Pens.Red, new System.Drawing.Rectangle((int)dr.X, (int)dr.Y, (int)dr.Width, (int)dr.Height));
            }
            Rect r = new Rect(SelPoint1, SelPoint2);
            GetSelection(r,true);
        }

        public void SelectActiveChannelBit()
        {
            int[] bitWidths = new int[] { 31, 23, 33, 12, 21 }; //weird implementation but go off
            int[] bitOffset = new int[] { 00, 31, 54, 87, 99 }; //oblong logic however fair
            SelPoint1 = new Point((126 * selectedChannel) + bitOffset[selectedBit]+MPTXOffset+4, 0);//+4 because padding overlap? ¯\_(ツ)_/¯
            SelPoint2 = new Point((126 * selectedChannel) + bitOffset[selectedBit] + bitWidths[selectedBit]+ MPTXOffset, 21 * RowCount);
            //===== DEBUG BOUNDARIES =====
            //Point Dp1 = PointToScreen(SelPoint1);
            //Point Dp2 = PointToScreen(SelPoint2);
            //Rect dr = new Rect(Dp1, Dp2);
            //using (System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
            //{
            //    g.DrawRectangle(System.Drawing.Pens.Red, new System.Drawing.Rectangle((int)dr.X, (int)dr.Y, (int)dr.Width, (int)dr.Height));
            //}
            //===== ================ =====

            Rect r = new Rect(SelPoint1, SelPoint2);
            GetSelection(r,true);
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

        public MPTPattern(int rowsPerMeasure,int rowsPerBeat,TrackerPattern pattern, FrameworkElement PatternContainer)
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
            Frame = PatternContainer;
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

        private void Pattern_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseDowned = true;
            CaptureMouse();
            SelPoint1 = e.GetPosition(Frame);
            SelPoint2 = e.GetPosition(Frame);
            Rect r = new Rect(SelPoint1, SelPoint2);
            multiSelect = false;
            GetSelection(r,false);
        }

        private void Pattern_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDowned)
            {
                SelPoint2 = e.GetPosition(Frame);
                Rect r = new Rect(SelPoint1, SelPoint2);

                double distance = Math.Abs(Point.Subtract(SelPoint2, SelPoint1).Length);

                if (distance > 5)
                {
                    multiSelect = true;
                }
                GetSelection(r,multiSelect);
                
            }
            
        }

        private void Pattern_MouseUp(object sender, MouseButtonEventArgs e)
        {

            ReleaseMouseCapture();
            mouseDowned = false;
            //SelPoint1 = e.GetPosition(Frame);
            //SelPoint2 = e.GetPosition(Frame);
            Rect r = new Rect(SelPoint1, SelPoint2);
            GetSelection(r,multiSelect);
            if (!multiSelect)
            {
                if (ActiveRow != null)
                {
                    ActiveRow.UpdateFocus(false);
                }

                ActiveRow = Dialog.GetElementUnderMouse<MPTRow>();
                if (ActiveRow != null)
                {
                    ActiveRowIndex = ActiveRow.RowIndex;

                    ActiveRow.UpdateFocus(true);
                    PatternSelectionChange?.Invoke(this, new SelectionEventArgs(ActiveRow.RowIndex));
                }
            }
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
