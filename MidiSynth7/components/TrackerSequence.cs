using System;
using System.Collections.Generic;
using Sanford.Multimedia.Midi;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Windows.Media;
using System.Windows;

namespace MidiSynth7.components
{
    /// <summary>
    /// GOALS: 
    ///  - Compatibility with OpenMPT's standard (At least mostly)
    ///      - Ability to copy-paste track data
    ///      - Ability to understand controller macros
    ///  - figure out how to display the tracker
    ///  
    /// </summary>

    public class TrackerSequence
    {
        string path = App.APP_DATA_DIR + "Sequences\\";
        string name = "";
        public string SequenceName { get=>name; set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    name = "Untitled Sequence";
                }
                string safeSeqName = Path.GetInvalidFileNameChars().Aggregate(value, (current, c) => current.Replace(c, '-'));
                string safePrvSeqName = Path.GetInvalidFileNameChars().Aggregate(name, (current, c) => current.Replace(c, '-'));

                if (File.Exists(path + safeSeqName+".mton") && value != name && !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(value))
                {
                    throw new Exception("A sequence with this name already exists.");

                }
                if(File.Exists(path + safePrvSeqName + ".mton"))
                {
                    File.Move(path + safePrvSeqName + ".mton", path + safeSeqName + ".mton");
                }
                name = value;
            }
        }
        public List<TrackerPattern> Patterns { get; set; }
        public List<TrackerInstrument> Instruments { get; set; }
        public int SelectedOctave { get; set; }
        public int SelectedInstrument{ get; set; }

        public void SaveSequence()
        {
            //verify row numbers
            foreach (TrackerPattern item in Patterns)
            {
                int r = 0;
                foreach (TrackerRow row in item.Rows)
                {
                    foreach (SeqData data in row.Notes)
                    {
                        if(data.Row != r)
                        {
                            data.Row = r;
                        } 
                    }
                    r++;
                }
            }
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string safeSeqName = Path.GetInvalidFileNameChars().Aggregate(SequenceName, (current, c) => current.Replace(c, '-'));
            if (File.Exists(path + safeSeqName+".mton"))
            {
                //delete then rewrite, to prevent corruptions
                File.Delete(path + safeSeqName + ".mton");
            }
            using (StreamWriter sw = new StreamWriter(path+safeSeqName + ".mton"))
            {
                sw.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
            }
        }

        public override string ToString()
        {
            return SequenceName;
        }
    }

    public class TrackerPattern
    {
        public int RowsPerMeasure { get; set; }
        public int RowsPerBeat { get; set; }
        public int RowCount { get; set; }
        public int ChannelCount { get; set; }
        public string PatternName { get; set; }
        public List<TrackerRow> Rows { get; set; }

        public static TrackerPattern GetEmptyPattern(TrackerSequence seq, int rows, int channels)
        {
            return new TrackerPattern()
            {
                ChannelCount = channels,
                RowCount = rows,
                PatternName = "New Pattern",
                Rows = TrackerRow.GetEmptyRows(seq, rows, channels),
                RowsPerBeat = 4,
                RowsPerMeasure = 16
            };
        }
    }

    public class TrackerRow
    {
        public TrackerRow( List<SeqData> notes)
        {
            Notes = notes;
        }
        public List<SeqData> Notes { get; set; }

        public static List<TrackerRow> GetEmptyRows(TrackerSequence seq, int rows, int ChannelCount)
        {
            List<TrackerRow> Rows = new List<TrackerRow>();
            for (int i = 0; i < rows; i++)
            {
                TrackerRow row = new TrackerRow(new List<SeqData>());
                for (int ii = 0; ii < ChannelCount; ii++)
                {
                    int midiChannel = GetMIDIChannelIndex(ii);
                    row.Notes.Add(new SeqData()
                    {
                        Row = i,
                        Column = ii,
                        Instrument = null,
                        midiChannel = midiChannel,
                        Parameter = null,
                        Pitch = null,
                        Velocity = null

                    });
                }
                Rows.Add(row);
            }
            return Rows;
        }

        private static int GetMIDIChannelIndex(int column)
        {
            int midiChannel = column;//channel 0-8
            if (column >= 9)
            {
                midiChannel = column + 1;//skip channel 9 (percussion)
            }
            if (column + 1 > 15)
            {
                midiChannel = 9;//channels beyond 15 will be routed to channel 9 as percussion.
            }

            return midiChannel;
        }

        public List<SeqParam> Play(TrackerSequence seq, MidiEngine engine, ControllerType[] ActiveControl)
        {
            List<SeqParam> ControlList = new List<SeqParam>();
            foreach (SeqData item in Notes)
            {
                TrackerInstrument ti = seq.Instruments.FirstOrDefault(x => x.Index == item.Instrument);
                int output = ti?.DeviceIndex ?? -1;
                ControlList.Add(item.Parameter);//even if null
                if (item.Parameter != null)
                {
                    if (item.Parameter.Mark == 'Z' && ActiveControl !=  null)//3 is unused, and will be considered "default/null"
                    {
                        engine.MidiNote_SetControl(ActiveControl[item.midiChannel], item.midiChannel, item.Parameter.Value, output);
                    }
                }
                if(ti != null)
                {
                    engine.MidiNote_SetProgram(ti.Bank, ti.Instrument, item.midiChannel, output);
                    if(item.Pitch.HasValue)
                    {
                        if(item.midiChannel != 9)
                        {
                            engine.MidiNote_SetControl(ControllerType.AllNotesOff, item.midiChannel, 127, output);//in theory just for the channel?
                        }
                        if(item.Pitch > -1)
                        {
                            engine.MidiNote_Play(item.midiChannel, item.Pitch.Value, item.Velocity ?? 127, false, output);
                        }
                        
                    }
                }
                if (item.Pitch.HasValue)
                {

                    if (item.Pitch == -2)
                    {
                        engine.MidiNote_SetControl(ControllerType.AllSoundOff, item.midiChannel, 127, output);//in theory just for the channel?
                    }
                    if (item.Pitch == -1)
                    {
                        engine.MidiNote_SetControl(ControllerType.AllNotesOff, item.midiChannel, 127, output);//in theory just for the channel?
                    }
                }
            }
            return ControlList;
        }
    }
    public class SeqData
    {
        #region Element Rendering Data
        private bool[] SelectedBits = new bool[] { false, false, false, false, false };

        //private bool NeedsRender = false;
        private Rect SqDatBit_Bounds;  // overall dimensions
        private Rect SqTxtBit_Bounds;  // text dimensions
        private List<Rect> boundList = new List<Rect>();
        internal readonly int Width = 120;
        internal readonly int Height = 22;

        public int Column { get; set; }
        public int Row { get; set; }

        public static SolidColorBrush BR_Empty = new SolidColorBrush(Color.FromArgb(255, 035, 067, 103));    // blank and some params
        public static SolidColorBrush BR_HotFG = new SolidColorBrush(Color.FromArgb(255, 223, 236, 255));    // Active row text
        public static SolidColorBrush BR_SelBG = new SolidColorBrush(Color.FromArgb(255, 032, 057, 097));    // Selection Background
        public static SolidColorBrush BR_SelFG = new SolidColorBrush(Color.FromArgb(255, 089, 149, 231));    // Selection Foreground
        public static SolidColorBrush BR_Pitch = new SolidColorBrush(Color.FromArgb(255, 223, 236, 255));    // Unselected Pitch
        public static SolidColorBrush BR_Patch = new SolidColorBrush(Color.FromArgb(255, 255, 133, 128));    // Unselected Patch
        public static SolidColorBrush BR_Veloc = new SolidColorBrush(Color.FromArgb(255, 128, 225, 139));    // Unselected Velocity
        public static SolidColorBrush BR_AParV = new SolidColorBrush(Color.FromArgb(255, 137, 185, 247));    // 'A' parameter
        public static SolidColorBrush BR_BdrEn = new SolidColorBrush(Color.FromArgb(255, 012, 016, 020));    // Dark border
        public static SolidColorBrush BR_BdrEx = new SolidColorBrush(Color.FromArgb(255, 039, 070, 120));    // Light border

        #endregion

        #region Sequence Data
        public int midiChannel;

        public int? Pitch { get; set; }
        public byte? Velocity { get; set; }
        public byte? Instrument { get; set; }
        public SeqParam Parameter { get; set; }
        #endregion

        public SeqData()
        {

        }

        public override string ToString()
        {
            string note = "...";
            if(Pitch.HasValue)
            {

                var n = MidiEngine.GetNote(Pitch.Value, "-");
                note = n.noteLabel + n.octave;
                if(Pitch.Value == -1)
                {
                    note = "===";
                }
                if (Pitch.Value == -2)
                {
                    note = "~~~";
                }
                if (Pitch.Value == -3)
                {
                    note = "^^^";
                }
            }
            string instindex = "..";
            if(Instrument != null)
            {
                instindex = Instrument.ToString();//It's not hex here since OpenMPT does not use hex for instruments
            }
            string velocity = "...";
            if (Velocity.HasValue)
            {
                velocity = "v" + (Velocity.Value/2);//Since OpenMPT does not support values over 64
            }
            string param = "...";
            if(Parameter != null)
            {
                param = Parameter.Mark.ToString() + Parameter.Value.ToString("X:2");
            }
            return "|"+note + instindex + velocity + param;
        }

        internal void Render(DrawingContext dc)
        {
            //if (!NeedsRender) return;
            boundList.Clear();
            double offset = 0;
            SqDatBit_Bounds = new Rect(Column * Width, Row * Height, Width, Height);
            SqTxtBit_Bounds = new Rect((Column * Width) + 3, (Row * Height) + 1, Width - 6, Height - 2);
            Rect PitchBit_Bounds = new Rect(SqTxtBit_Bounds.X, SqDatBit_Bounds.Y, 32, Height);
            offset += PitchBit_Bounds.Width;
            Rect InstrBit_Bounds = new Rect(SqTxtBit_Bounds.X + offset, SqDatBit_Bounds.Y, 22, Height);
            offset += InstrBit_Bounds.Width;
            Rect VelocBit_Bounds = new Rect(SqTxtBit_Bounds.X + offset, SqDatBit_Bounds.Y, 32, Height);
            offset += VelocBit_Bounds.Width;
            Rect SqParBit_Bounds = new Rect(SqTxtBit_Bounds.X + offset, SqDatBit_Bounds.Y, 10, Height);
            offset += SqParBit_Bounds.Width;
            Rect SqValBit_Bounds = new Rect(SqTxtBit_Bounds.X + offset, SqDatBit_Bounds.Y, 18, Height);
            boundList.Add(PitchBit_Bounds);
            boundList.Add(InstrBit_Bounds);
            boundList.Add(VelocBit_Bounds);
            boundList.Add(SqParBit_Bounds);
            boundList.Add(SqValBit_Bounds);
            //render text and selection if need
            for (int i = 0; i < SelectedBits.Length; i++)
            {
                Brush bg = SelectedBits[i] ? BR_SelBG : null;
                dc.DrawRectangle(bg, null, boundList[i]);
                Brush fg = BR_Empty;
                string text = "...";
                switch (i)
                {
                    case 0:
                        fg = SelectedBits[i] ? BR_SelFG : (Pitch.HasValue ? BR_Pitch : BR_Empty);
                        if (Pitch.HasValue)
                        {
                            var note = MidiEngine.GetNote(Pitch.Value, "-");
                            text = note.noteLabel + note.octave;
                            if(Pitch == -1)
                            {
                                fg = SelectedBits[i] ? BR_SelFG : BR_Empty;
                                text = "== ";
                            }
                            if (Pitch == -2)
                            {
                                fg = SelectedBits[i] ? BR_SelFG : BR_Empty;
                                text = "~~ ";
                            }
                            if (Pitch == -2)
                            {
                                fg = SelectedBits[i] ? BR_SelFG : BR_Empty;
                                text = "^^ ";
                            }
                        }
                        break;
                    case 1:
                        fg = SelectedBits[i] ? BR_SelFG : (Instrument.HasValue ? BR_Patch : BR_Empty);
                        text = "..";
                        if (Instrument.HasValue)
                        {
                            text = Instrument.Value.ToString("X2");
                            
                        }
                        break;
                    case 2:
                        fg = SelectedBits[i] ? BR_SelFG : (Velocity.HasValue ? BR_Veloc : BR_Empty);
                        text = " ..";
                        if (Velocity.HasValue)
                        {
                            text = "v"+Velocity.Value.ToString("X2");
                        }
                        break;
                    case 3:
                        text = ".";
                        fg = SelectedBits[i] ? BR_SelFG : (Parameter?.Mark == 'A' ? BR_AParV : BR_Empty);
                        if (Parameter != null)
                        {
                            text = Parameter.Mark.ToString();
                        } 
                        break;
                    case 4:
                        text = "..";

                        fg = SelectedBits[i] ? BR_SelFG : (Parameter?.Mark == 'A' ? BR_AParV : BR_Empty);
                        if (Parameter != null)
                        {
                            text = Parameter.Value.ToString("X2");
                        }
                        break;
                    default: break;
                }
                dc.DrawText(Text(text, fg), boundList[i].TopLeft);

            }
            //draw borders
            Point tr2 = SqDatBit_Bounds.TopRight;
            Point br2 = SqDatBit_Bounds.BottomRight;

            dc.DrawLine(new Pen(BR_BdrEn, 4), tr2, br2);
            dc.DrawLine(new Pen(BR_BdrEx, 2), SqDatBit_Bounds.TopRight, SqDatBit_Bounds.BottomRight);
            
            //NeedsRender = false;
        }
        private FormattedText Text(string text, Brush foreground)
        {
            return new FormattedText(text, System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,SystemComponent.MPTEditorFont, 16, foreground, 1);
        }
        internal void DetectSelection(Rect Selection)
        {
            //NeedsRender = true;
        }
    }

    public class TrackerInstrument
    {
        public byte Index { get; set; }
        public int DeviceIndex { get; set; }
        public int Bank { get; set; }
        public int Instrument { get; set; }
        public string DisplayName { get; set; }

        public TrackerInstrument(byte index,int device,int bank, int instrument, string name)
        {
            Index = index;
            DeviceIndex = device;
            Bank = bank;
            Instrument = instrument;
            DisplayName = name;

        }

        public override string ToString()
        {
            return $"{(int)Index:00}: {DisplayName}";
        }
    }

    public class SeqParam
    {
        public char Mark { get; set; }

        public byte Value { get; set; }
    }
}
