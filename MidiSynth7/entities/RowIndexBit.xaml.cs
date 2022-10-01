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
        }
        public RowIndexBit(int index)
        {
            InitializeComponent();
            RowIndex = index;
        }
    }
}
