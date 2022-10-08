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
        private SeqData _data { get; set; }
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
            get => _data.Pitch;
            set
            {
                _data.Pitch = value;
                UpdatePitch(value);
                
            }
        }

        [Category("MPTBit Properties")]
        public byte? Instrument
        {
            get => _data.Instrument;
            set
            {
                _data.Instrument = value;
                UpdateInstColors(value);

            }
        }


        [Category("MPTBit Properties")]
        public byte? Velocity
        {
            get => _data.Velocity;
            set
            {
                _data.Velocity = value;
                UpdateVelocityColor(value);

            }
        }

        [Category("MPTBit Properties")]
        public SeqParam Parameter
        {
            get => _data.Parameter;
            set
            {
                UpdateSeqParamColors(value);

                _data.Parameter = value;

            }
        }

        public int Channel { get; private set; }
        
        public MPTBit()
        {
            InitializeComponent();
        }
        
        public MPTBit(int col, SeqData data)
        {
            InitializeComponent();
            Channel = col;
            _data = data;
            UpdatePitch(data.Pitch);
            UpdateVelocityColor(data.Velocity);
            UpdateInstColors(data.Instrument);
            UpdateSeqParamColors(data.Parameter);
        }
        
        public SeqData GetSeqData()
        {
            
            return _data;
        }

        private void UpdateInstColors(byte? value)
        {
            if (value == null)
            {
                Bl_Instrument.Text = "..";
                Bl_Instrument.Foreground = Bl_Instrument.Foreground != BR_NHot ? BR_Null : BR_NHot;

            }
            else
            {
                Bl_Instrument.Text = value.Value.ToString("X2");
                Bl_Instrument.Foreground = BR_Inst;
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

        internal void ProcessKey(Key key, int octave, byte? instrument = null)
        {
            if (activeTblock == null)
            {
                Console.WriteLine("no activeTblock");
                return;
            }
            if(activeTblock == Bl_Notation)
            {
                if(key == Key.Q && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    Pitch += Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? 12 : 1;

                    if (Pitch > 127)Pitch = 127;
                    BitDataChanged?.Invoke(this, new BitEventArgs(GetSeqData(),EventType.note));
                    return;
                }
                if (key == Key.A && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    Pitch -= Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? 12 : 1;
                    if (Pitch < 0) Pitch = 0;
                    BitDataChanged?.Invoke(this, new BitEventArgs(GetSeqData(), EventType.note));
                    return;
                }
                int indx = Array.IndexOf(SystemComponent.MPTKeysTable, key);
                if (indx > -1)
                {
                    Pitch = indx + 21 + (12*octave);
                    Instrument = instrument;
                    //throw velocity in for funzies
                    Velocity = 127;
                    BitDataChanged?.Invoke(this, new BitEventArgs(GetSeqData(), EventType.note));

                }
                if(key == Key.Delete)
                {
                    Pitch = null;
                    Instrument = null;
                    Velocity = null;
                    BitDataChanged?.Invoke(this, new BitEventArgs(GetSeqData(), EventType.delete));

                    return;
                }
                if (key == KeyInterop.KeyFromVirtualKey(187))
                {
                    Pitch = -1;
                    Instrument = null;
                    Velocity = null;
                    BitDataChanged?.Invoke(this, new BitEventArgs(GetSeqData(), EventType.stop));

                    return;
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
    public class BitEventArgs : EventArgs
    {
        public SeqData NewSeqData { get; private set; }

        public EventType Type { get; set; }

        public BitEventArgs(SeqData data,EventType etype)
        {
            NewSeqData = data;
            Type = etype;
        }
    }

    public enum EventType
    {
        note,
        velo,
        stop,
        delete
    }
}
