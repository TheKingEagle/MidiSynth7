using MidiSynth7.components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
        private bool locker = false;
        private bool mouseDowned = false;
        private bool multiSelect = false;

        private SolidColorBrush bg_subdivision1 = new SolidColorBrush(Color.FromArgb(255, 24, 30, 40));
        private SolidColorBrush bg_subdivision2 = new SolidColorBrush(Color.FromArgb(255, 20, 26, 34));
        private SolidColorBrush bg_subdivisionN = new SolidColorBrush(Color.FromArgb(255, 12, 16, 20));
        private MPTRow ActiveRow;
        MainWindow appwin;
        public MPTBit ActiveBit { get; private set; }
        public List<MPTBit> activeBits { get; private set; } = new List<MPTBit>();
        private Point SelPoint1 = new Point(0, 0);
        private Point SelPoint2 = new Point(0, 0);
        public Rect GetSelectedBounds()
        {
            return new Rect(SelPoint1, SelPoint2);
        }
        private TrackerPattern tpf = new TrackerPattern();
        private List<MPTRow> mptRows = new List<MPTRow>();
        private BackgroundWorker bw = new BackgroundWorker();
        private FrameworkElement Frame;

        public int selectedBit { get; private set; } = 0;

        public int selectedChannel { get; private set; } = 0;

        public int ActiveRowIndex { get; internal set; }

        public int ChannelCount { get; set; } = 16;

        public int RowCount { get; set; } = 32;

        public int RowsPerBeat { get; set; } = 4;

        public int RowsPerMeasure { get; set; } = 16;

        public event EventHandler<SelectionEventArgs> PatternSelectionChange;

        public MPTPattern()
        {
            InitializeComponent();
        }

        public MPTPattern(MainWindow main, TrackerPattern pattern, FrameworkElement PatternContainer)
        {
            tpf = pattern;
            InitializeComponent();
            rowContainer.Visibility = Visibility.Collapsed;
            RowCount = tpf.RowCount;
            ChannelCount = tpf.ChannelCount;
            RowsPerBeat = pattern.RowsPerBeat;
            RowsPerMeasure = pattern.RowsPerBeat;
            bw.DoWork += Bw_DoWork;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            bw.RunWorkerAsync();
            Frame = PatternContainer;
            appwin = main;
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Dispatcher.Invoke(() => rowContainer.ItemsSource = mptRows);
            Dispatcher.Invoke(() => rowContainer.Visibility = Visibility.Visible);

        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            void threader()
            {
                int id = 0;
                foreach (TrackerRow item in tpf.Rows)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var row = new MPTRow(appwin,id, ChannelCount, item);
                        row.RowDataUpdated += Row_RowDataUpdated;
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

        private void Row_RowDataUpdated(object sender, RowDataEventArgs e)
        {
            tpf.Rows[mptRows.IndexOf((MPTRow)sender)] = e.NewRowData;
        }

        private void Pattern_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!mouseDowned)
            {
                activeBits?.Clear();
            }
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
            if (multiSelect)
            {
                IEnumerable<MPTRow> f = rowContainer.Items.OfType<MPTRow>().Where(x => x.BoundsRelativeTo(Frame).IntersectsWith(r));
                foreach (MPTRow item in f)
                {
                    var selection = item.GetLogicalSelection(r, Frame);
                    activeBits.AddRange(selection);
                    
                }
            }
        }

        public void GetSelection(Rect bounds, bool intendsMulti = false)
        {
            foreach (var selcl in rowContainer.Items.OfType<MPTRow>())
            {
                selcl.ClearSelection();
            }
            IEnumerable<MPTRow> f = rowContainer.Items.OfType<MPTRow>().Where(x => x.BoundsRelativeTo(Frame).IntersectsWith(bounds));
            foreach (MPTRow item in f)
            {
                var selection = item.GetSelection(bounds, Frame, intendsMulti);
                if (!intendsMulti)
                {
                    ActiveBit = selection;
                    selectedChannel = item.SelectedChannel;
                    selectedBit = item.SelectedBit;
                    break;
                }
            }
            //update active row; so we don't get font color misses
            MPTRow ar = rowContainer.Items.OfType<MPTRow>().FirstOrDefault(x => x.Active);
            if (ar != null) ar.UpdateFocus(ar.Active);

        }

        public void SetHotRow(int index)
        {
            ActiveRow.UpdateFocus(false);
            ActiveRow = mptRows.FirstOrDefault(x => x.RowIndex == index);
            ActiveRow.UpdateFocus(true);
            Dispatcher.Invoke(() => PatternSelectionChange?.Invoke(this, new SelectionEventArgs(index)));
        }

        public void SelectActiveChannel()
        {
            SelPoint1 = new Point(126 * selectedChannel, 0);
            SelPoint2 = new Point((126 * selectedChannel) + 126, 21 * RowCount);
#if BOUNDS
            DebugBounds();
#endif
            Rect r = new Rect(SelPoint1, SelPoint2);
            GetSelection(r, true);
        }

        public void SelectActiveChannelBit()
        {
            int[] bitWidths = new int[] { 31, 23, 33, 12, 21 }; //weird implementation but go off
            int[] bitOffset = new int[] { 00, 31, 54, 87, 99 }; //oblong logic however fair
            SelPoint1 = new Point((126 * selectedChannel) + bitOffset[selectedBit] + 4, 0);//+4 because padding overlap? ¯\_(ツ)_/¯
            SelPoint2 = new Point((126 * selectedChannel) + bitOffset[selectedBit] + bitWidths[selectedBit], 21 * RowCount);
#if BOUNDS
            DebugBounds();
#endif
            Rect r = new Rect(SelPoint1, SelPoint2);
            GetSelection(r, true);
        }

        public void MoveBitActiveRow(int ch, int bit)
        {
            int[] bitWidths = new int[] { 31, 23, 33, 12, 21 }; //weird implementation but go off
            int[] bitOffset = new int[] { 00, 31, 54, 87, 99 }; //oblong logic however fair
            SelPoint1 = new Point((126 * ch) + bitOffset[bit] + 4, (21 * ActiveRowIndex) + 2);
            SelPoint2 = new Point((126 * ch) + bitOffset[bit] + bitWidths[bit] + 4, (21 * ActiveRowIndex) + 2);
#if BOUNDS
            DebugBounds();
#endif
            Rect r = new Rect(SelPoint1, SelPoint2);
            GetSelection(r, false);
        }

#if BOUNDS
        private void DebugBounds()
        {
            Point Dp1 = PointToScreen(SelPoint1);
            Point Dp2 = PointToScreen(SelPoint2);
            Rect dr = new Rect(Dp1, Dp2);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
            {
                g.DrawRectangle(System.Drawing.Pens.Red, new System.Drawing.Rectangle((int)dr.X, (int)dr.Y, (int)dr.Width, (int)dr.Height));
            }
        }
#endif
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
