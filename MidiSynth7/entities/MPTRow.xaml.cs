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

namespace MidiSynth7.entities.controls
{
    /// <summary>
    /// Interaction logic for MPTPattern.xaml
    /// </summary>
    public partial class MPTRow : ItemsControl
    {
        int _chCount = 16;
        SequenceRow _rowData;
        [Category("Row Info")]
        public int ChannelCount { get => _chCount; set { _chCount = value; UpdateList(); } }
        [Category("Row Info")]
        public SequenceRow RowData { get => _rowData ?? null; set { _rowData = value; } }

        [Category("Row Info")]
        public int RowIndex { get => int.Parse(BL_RowIndex.Text); set => BL_RowIndex.Text = value.ToString(); }
        public List<MPTBit> bits = new List<MPTBit>();
        public MPTRow()
        {

            InitializeComponent();
            UpdateList();
        }
        public MPTRow(int index, int channelCount = 16, SequenceRow row = null)
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
            //pop generics
            for (int i = 0; i < ChannelCount; i++)
            {
                MPTBit bit = new MPTBit(i)
                {
                    Pitch = null,
                    Instrument = null,
                    Velocity = null,
                    Parameter = null,
                };
               bits.Add(bit);
            }
            //pain in the ass!!!!!
            MinWidth = 126 * ChannelCount;
            MinHeight = 21;
            row_container.ItemsSource = bits;
            //pop values
            if (_rowData != null)
            {
                foreach (var item in _rowData.Channels)
                {
                    MPTBit col = bits.OfType<MPTBit>().FirstOrDefault(x => x.Channel == item.col);
                    if (col == null) { continue; }//in theory, should never happen.
                    col.Pitch = item.data.Pitch;
                    col.Instrument = item.data.Instrument;
                    col.Velocity = item.data.Velocity;
                    col.Parameter = item.data.Parameter ?? null;
                }
            }
            

        }
    }
}
