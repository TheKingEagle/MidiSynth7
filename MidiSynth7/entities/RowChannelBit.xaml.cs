using MidiSynth7.components;
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
            MinWidth = SeqData.Width;
            MaxWidth = SeqData.Width;
            Width = SeqData.Width;
        }
        public RowChannelBit(int ch)
        {
            InitializeComponent();
            Channel = ch;
            MinWidth = SeqData.Width;
            MaxWidth = SeqData.Width;
            Width = SeqData.Width;
        }
    }
}
