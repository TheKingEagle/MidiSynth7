using MidiSynth7.components;
using System.ComponentModel;
using System.Windows.Controls;

namespace MidiSynth7.entities
{
    /// <summary>
    /// Interaction logic for RowIndexBit.xaml
    /// </summary>
    public partial class RowIndexBit : Border
    {

        [Category("Row Info")]
        public int RowIndex { get => int.Parse(BL_RowIndex.Text); set => BL_RowIndex.Text = value.ToString(); }

        public RowIndexBit()
        {
            InitializeComponent();
            MinHeight = SeqData.Height;
            MaxHeight = SeqData.Height;
            Height = SeqData.Height;
        }
        public RowIndexBit(int index)
        {
            InitializeComponent();
            RowIndex = index;

            MinHeight = SeqData.Height;
            MaxHeight = SeqData.Height;
            Height = SeqData.Height;
        }
    }
}
