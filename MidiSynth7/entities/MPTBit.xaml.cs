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
    /// Interaction logic for MPTBit.xaml
    /// </summary>
    public partial class MPTBit : ItemsControl
    {
        int? _pitch;
        TrackerInstrument _instrument;
        SeqParam _param;
        byte? _velo;
        SolidColorBrush BR_Null = new SolidColorBrush(Color.FromArgb(255, 35, 67, 103));
        SolidColorBrush BR_Note = new SolidColorBrush(Color.FromArgb(255, 223, 236, 255));
        SolidColorBrush BR_Inst = new SolidColorBrush(Color.FromArgb(255, 255, 133, 128));
        SolidColorBrush BR_Velo = new SolidColorBrush(Color.FromArgb(255, 128, 225, 139));
        SolidColorBrush BR_APar = new SolidColorBrush(Color.FromArgb(255, 137, 185, 247));
        [Category("MPTBit Properties")]
        public int? Pitch
        {
            get => _pitch;
            set
            {
                _pitch = value;
                if (value == null || value == -1)
                {
                    Bl_Notation.Text = value != -1 ? "..." : "== ";
                    Bl_Notation.Foreground = BR_Null;
                }
                else
                {
                    var n = MidiEngine.GetNote(value.Value, "-");
                    Bl_Notation.Text = n.noteLabel + n.octave;
                    Bl_Notation.Foreground = BR_Note;
                }
            }
        }
        [Category("MPTBit Properties")]
        public TrackerInstrument Instrument
        {
            get => _instrument;
            set
            {
                _instrument = value;
                if (value == null)
                {
                    Bl_Instrument.Text = "..";
                    Bl_Instrument.Foreground = BR_Null;
                }
                else
                {
                    Bl_Instrument.Text = value.Index.ToString("X2");
                    Bl_Instrument.Foreground = BR_Inst;
                }
            }
        }
        [Category("MPTBit Properties")]
        public byte? Velocity
        {
            get => _velo;
            set
            {
                _velo = value;
                if (value == null)
                {
                    Bl_Velocity.Text = " ..";
                    Bl_Velocity.Foreground = BR_Null;
                }
                else
                {
                    Bl_Velocity.Text = "v" + value.Value.ToString("X2");
                    Bl_Velocity.Foreground = BR_Velo;
                }
            }
        }
        [Category("MPTBit Properties")]
        public SeqParam Parameter
        {
            get => _param;
            set
            {
                if(value != null)
                {
                    if (value.Mark == 'A')
                    {
                        Bl_Instrument.Foreground = BR_APar;
                    }
                    else
                    {
                        Bl_Instrument.Foreground = BR_Null;
                    }
                }
            }
        }

        public int Channel { get; private set; }
        [Category("MPTBit Properties")]
        public MPTBit()
        {
            InitializeComponent();
        }

        public MPTBit(int channel)
        {
            InitializeComponent();
            Channel = channel;
        }

        public SeqData GetSeqData()
        {
            SeqData d = new SeqData()
            {
                Instrument = Instrument,
                Parameter = Parameter,
                Pitch = Pitch,
                Velocity = Velocity
            };
            return d;
        }

    }
}
