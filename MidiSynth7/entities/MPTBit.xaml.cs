using MidiSynth7.components;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
        private TextBlock activeTblock;
        private bool OtherBitsDeleted = false;
        public static SolidColorBrush BR_Null = new SolidColorBrush(Color.FromArgb(255, 035, 067, 103));
        public static SolidColorBrush BR_NHot = new SolidColorBrush(Color.FromArgb(255, 223, 236, 255));
        public static SolidColorBrush BR_NSel = new SolidColorBrush(Color.FromArgb(255, 032, 057, 097));
        public static SolidColorBrush BR_Note = new SolidColorBrush(Color.FromArgb(255, 223, 236, 255));
        public static SolidColorBrush BR_Inst = new SolidColorBrush(Color.FromArgb(255, 255, 133, 128));
        public static SolidColorBrush BR_Velo = new SolidColorBrush(Color.FromArgb(255, 128, 225, 139));
        public static SolidColorBrush BR_APar = new SolidColorBrush(Color.FromArgb(255, 137, 185, 247));
        public static SolidColorBrush BR_FSel = new SolidColorBrush(Color.FromArgb(255, 089, 149, 231));

        public event EventHandler<BitEventArgs> BitDataChanged;
        
        [Category("MPTBit Properties")]
        public int? Pitch
        {
            get => _pitch;
            set
            {
                _pitch = value;
                UpdatePitch(value);
            }
        }

        private void UpdatePitch(int? value)
        {
            if (value == null || value == -1)
            {
                Bl_Notation.Text = value != -1 ? "..." : "== ";
                Bl_Notation.Foreground = Bl_Notation.Foreground != BR_NHot ? BR_Null : BR_NHot;

            }
            else
            {
                var n = MidiEngine.GetNote(value.Value, "-");
                Bl_Notation.Text = n.noteLabel + n.octave;
                Bl_Notation.Foreground = BR_Note;
            }
        }

        [Category("MPTBit Properties")]
        public TrackerInstrument Instrument
        {
            get => _instrument;
            set
            {
                _instrument = value;
                UpdateInstColors(value);
            }
        }

        private void UpdateInstColors(TrackerInstrument value)
        {
            if (value == null)
            {
                Bl_Instrument.Text = "..";
                Bl_Instrument.Foreground = Bl_Instrument.Foreground != BR_NHot ? BR_Null : BR_NHot;

            }
            else
            {
                Bl_Instrument.Text = value.Index.ToString("X2");
                Bl_Instrument.Foreground = BR_Inst;
            }
        }

        [Category("MPTBit Properties")]
        public byte? Velocity
        {
            get => _velo;
            set
            {
                _velo = value;
                UpdateVelocityColor(value);
            }
        }

        private void UpdateVelocityColor(byte? value)
        {
            if (value == null)
            {
                Bl_Velocity.Text = " ..";
                Bl_Velocity.Foreground = Bl_Velocity.Foreground != BR_NHot ? BR_Null : BR_NHot;
            }
            else
            {
                Bl_Velocity.Text = "v" + value.Value.ToString("X2");
                Bl_Velocity.Foreground = BR_Velo;
            }
        }

        [Category("MPTBit Properties")]
        public SeqParam Parameter
        {
            get => _param;
            set
            {
                UpdateSeqParamColors(value);

                _param = value;
            }
        }

        private void UpdateSeqParamColors(SeqParam value)
        {
            
            if (value != null)
            {
                if (value.Mark == 'A')
                {
                    Bl_ParmMark.Foreground = BR_APar;
                    Bl_ParmValue.Foreground = BR_APar;

                }
                else
                {
                    Bl_ParmMark.Foreground = Bl_ParmMark.Foreground != BR_NHot ? BR_Null : BR_NHot;
                    Bl_ParmValue.Foreground = Bl_ParmValue.Foreground != BR_NHot ? BR_Null : BR_NHot; 
                }
            }
            else
            {
                Bl_ParmMark.Foreground = Bl_ParmMark.Foreground != BR_NHot ? BR_Null : BR_NHot;
                Bl_ParmValue.Foreground = Bl_ParmValue.Foreground != BR_NHot ? BR_Null : BR_NHot;
            }
        }

        public int Channel { get; private set; }
        
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

        internal void UpdateFocus(bool active)
        {

            foreach (TextBlock tbl in Bit_Container.Children.OfType<TextBlock>())
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    if (tbl.Foreground == BR_Null || tbl.Foreground == BR_NHot)
                    {
                        tbl.Foreground = active ? BR_NHot : BR_Null;
                    }

                });
            }
        }

        internal void ProcessKey(Key key, int octave, TrackerInstrument instrument = null)
        {
            if (activeTblock == null)
            {
                Console.WriteLine("no activeTblock");
                return;
            }
            if(activeTblock == Bl_Notation)
            {
                int indx = Array.IndexOf(SystemComponent.MPTKeysTable, key);
                if (indx > -1)
                {
                    Pitch = indx + 21 + (12*octave);
                    Instrument = instrument;
                    //throw velocity in for funzies
                    Velocity = 127;

                }
                if(key == Key.Delete)
                {
                    Pitch = null;
                    Velocity = null;
                }
            }
        }

        internal void DelSelection(Rect bounds, FrameworkElement ele)
        {
            OtherBitsDeleted = false;
            foreach (TextBlock tbl in Bit_Container.Children.OfType<TextBlock>().Where(x=>x.BoundsRelativeTo(ele).IntersectsWith(bounds)))
            {

                if (tbl == Bl_Instrument)
                {
                    Instrument = null;
                    OtherBitsDeleted = true;
                }
                if (tbl == Bl_Notation)
                {
                    Pitch = null;
                    OtherBitsDeleted = true;
                }
                if (tbl == Bl_ParmMark)
                {
                    Parameter = null;
                    OtherBitsDeleted = true;
                }
                if (tbl == Bl_Velocity)
                {
                    if (OtherBitsDeleted)
                    {
                        Velocity = null;
                    }
                    else
                    {
                        Velocity = 127;
                    }
                }

            }
        }

        public int GetSelection(Rect bounds, FrameworkElement ele, bool intendsMulti = false)
        {
            int select = 0;
            foreach (TextBlock tbl in Bit_Container.Children.OfType<TextBlock>().Where(x => x.Background == null))
            {
                if (tbl.BoundsRelativeTo(ele).IntersectsWith(bounds))
                {
                    tbl.Background = BR_NSel;
                    tbl.Foreground = BR_FSel;
                    if (!intendsMulti)
                    {
                        select = Bit_Container.Children.IndexOf(tbl);
                        activeTblock = tbl;
                        return select;
                    }
                    activeTblock = null;
                } else
                {
                    tbl.Background = null;
                }
                
            }
            return 0;
        }

        public void ClearSelection()
        {
            foreach (TextBlock item in Bit_Container.Children.OfType<TextBlock>().Where(x => x.Background != null))
            {
                item.Background = null;
                
            }
                UpdateSeqParamColors(Parameter);
                UpdatePitch(Pitch);
                UpdateInstColors(Instrument);
                UpdateVelocityColor(Velocity);
        }

    }

    public static class TBLExtender
    {
        public static Rect BoundsRelativeTo(this FrameworkElement child, Visual parent)
        {
            GeneralTransform gt = child.TransformToAncestor(parent);
            return gt.TransformBounds(new Rect(0, 0, child.ActualWidth, child.ActualHeight));
        }
    }
    public class BitEventArgs : EventArgs
    {
        public SeqData NewSeqData { get; private set; }

        public int Index { get; private set; }

        public BitEventArgs(SeqData data, int index)
        {
            NewSeqData = data;
            Index = index;
        }
    }
}
