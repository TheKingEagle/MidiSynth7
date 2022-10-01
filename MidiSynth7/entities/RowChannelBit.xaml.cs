using System.ComponentModel;
using System.Windows.Controls;

namespace MidiSynth7.entities
{
    /// <summary>
    /// Interaction logic for RowChannelBit.xaml
    /// </summary>
    public partial class RowChannelBit : Border
    {
        [Category("Channel Info")]
        public int Channel { get => int.Parse(BL_ChannelIndex.Text.Replace("Channel ", "")); set => BL_ChannelIndex.Text = "Channel " + value.ToString(); }
        public RowChannelBit()
        {
            InitializeComponent();
        }
        public RowChannelBit(int ch)
        {
            InitializeComponent();
            Channel = ch;
        }
    }
}
