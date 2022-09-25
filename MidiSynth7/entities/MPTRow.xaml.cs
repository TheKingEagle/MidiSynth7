using MidiSynth7.components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace MidiSynth7.entities
{
    /// <summary>
    /// Interaction logic for MPTRow.xaml
    /// </summary>

    public partial class MPTRow : UserControl
    {
        int _chCount = 16;
        TrackerRow _rowData;
        [Category("Row Info")]
        public int ChannelCount { get => _chCount; set { _chCount = value; UpdateList(); } }
        [Category("Row Info")]
        public TrackerRow RowData { get => _rowData ?? null; set { _rowData = value; } }

        [Category("Row Info")]
        public int RowIndex { get => int.Parse(BL_RowIndex.Text); set => BL_RowIndex.Text = value.ToString(); }
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

