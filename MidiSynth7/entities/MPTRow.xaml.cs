using MidiSynth7.components;
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
        public MPTRow()
        {

            InitializeComponent();
            UpdateList();
        }
        public MPTRow(int index, int channelCount = 16, TrackerRow row = null)
        {

            InitializeComponent();
            _chCount = channelCount;
            if (row != null)
            {
                _rowData = row;
            }
            RowIndex = index;
            UpdateList();
        }
        internal void UpdateFocus(bool active)
        {
            Active = active;
            row_container.Background = active ? bg_HotSelected1 : null;
            foreach (MPTBit item in bits)
            {
                Dispatcher.Invoke(()=>item.UpdateFocus(active));
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
            row_container.ItemsSource = bits;
            //pop values

            foreach (var item in _rowData.Notes)
            {
                MPTBit bit = new MPTBit(item.Column);
                bit.Pitch = item.Pitch;
                bit.Instrument = item.Instrument;
                bit.Velocity = item.Velocity;
                bit.Parameter = item.Parameter;
                bits.Add(bit);
            }
            row_container.ItemsSource = bits;

        }
    }
}

