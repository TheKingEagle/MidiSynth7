using MidiSynth7.components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MidiSynth7.entities
{
    /// <summary>
    /// Interaction logic for MPTRow.xaml
    /// </summary>

    public partial class MPTRow : UserControl
    {
        public bool Active { get; private set; }
        int _chCount = 16;
        MainWindow _win;
        public static SolidColorBrush bg_HotSelected1 = new SolidColorBrush(Color.FromArgb(255, 0, 67, 159));
        TrackerRow _rowData;
        [Category("Row Info")]
        public int ChannelCount { get => _chCount; set { _chCount = value; UpdateList(); } }
        [Category("Row Info")]
        public TrackerRow RowData { get => _rowData ?? null; set { _rowData = value; } }

        [Category("Row Info")]
        public int RowIndex { get; private set; }

        public int SelectedChannel { get; private set; }
        public int SelectedBit { get; private set; }
        public List<MPTBit> bits = new List<MPTBit>();

        public event EventHandler<RowDataEventArgs> RowDataUpdated;

        public MPTRow()
        {

            InitializeComponent();
            UpdateList();
        }
        public MPTRow(MainWindow appwin, int index, int channelCount = 16, TrackerRow row = null)
        {

            InitializeComponent();
            _chCount = channelCount;
            if (row != null)
            {
                _rowData = row;
            }
            RowIndex = index;
            _win = appwin;
            UpdateList();
        }
        internal void UpdateFocus(bool active, bool updateBits = true)
        {

            Active = active;
            row_container.Background = active ? bg_HotSelected1 : null;
            if (updateBits)
            {
                foreach (MPTBit item in bits)
                {
                    Dispatcher.Invoke(() => item.UpdateFocus(active));
                }
            }
            
        }

        public MPTBit GetSelection(Rect bounds, FrameworkElement relativeTo, bool IntendsMulti = false)
        {
            var f = row_container.Items.OfType<MPTBit>().Where(x => x.BoundsRelativeTo(relativeTo).IntersectsWith(bounds));
            //List<MPTBit> bits = new List<MPTBit>();
            foreach (var item in f)
            {
                SelectedBit = item.GetSelection(bounds, relativeTo, IntendsMulti);
                //bits.Add(item);
                if (!IntendsMulti)
                {
                    SelectedChannel = item.Channel;
                    return item;
                }
            }
            return null;
        }
        
        public MPTBit[] GetLogicalSelection(Rect bounds, FrameworkElement relativeTo)
        {
            return row_container.Items.OfType<MPTBit>().Where(x => x.BoundsRelativeTo(relativeTo).IntersectsWith(bounds)).ToArray();
        }

        public void ClearSelection()
        {
            foreach (MPTBit item in bits)
            {
                Dispatcher.Invoke(() => item.ClearSelection());
            }
        }
        public void UpdateList()
        {
            //clear
            bits.Clear();
            
            //pain in the ass!!!!!
            MinWidth = 126 * ChannelCount;
            MinHeight = 21;
            //pop values

            foreach (var item in _rowData.Notes)
            {
                MPTBit bit = new MPTBit(item.Column,item);
                bit.BitDataChanged += Bit_BitDataChanged;
                bits.Add(bit);
            }
            row_container.ItemsSource = bits;

        }

        private void Bit_BitDataChanged(object sender, BitEventArgs e)
        {
            MPTBit s = sender as MPTBit;
            RowData.Notes[bits.IndexOf(s)] = e.NewSeqData;
            RowDataUpdated?.Invoke(this, new RowDataEventArgs(RowData));
            if(e.Type == EventType.note || e.Type == EventType.stop)
            {
                RowData.Play(_win.ActiveSequence,_win.MidiEngine, null);
            }
        }
    }

    public class RowDataEventArgs : EventArgs
    {
        public TrackerRow NewRowData { get; private set; }

        public RowDataEventArgs(TrackerRow row)
        {
            NewRowData = row;
        }
    }
}

