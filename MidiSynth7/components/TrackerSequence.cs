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
    ///  - Performance improvements
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

        public static TrackerPattern GetEmptyPattern(int rows, int channels)
        {
            return new TrackerPattern()
            {
                ChannelCount = channels,
                RowCount = rows,
                PatternName = "New Pattern",
                Rows = TrackerRow.GetEmptyRows(rows, channels),
                RowsPerBeat = 4,
                RowsPerMeasure = 16
            };
        }
    }

    public class TrackerRow
    {
        internal Rect rowbounds;
        internal DrawingGroup RowRender = new DrawingGroup();
        
        public List<SeqData> Notes { get; set; }

        public TrackerRow(List<SeqData> notes)
        {
            Notes = notes;
            rowbounds = new Rect(0, notes[0].Row * 22, SeqData.Width * notes.Count, 22);
        }

        public static List<TrackerRow> GetEmptyRows(int rows, int ChannelCount)
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
                row.rowbounds = new Rect(0, row.Notes[0].Row * 22, SeqData.Width * row.Notes.Count, 22);
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

        public void UpdateRow(int index, SolidColorBrush back, bool hot=false, bool ignorebits = false, Rect? debugrect=null)
        {
            var dc = RowRender.Open();
            dc.DrawRectangle(back, null, new Rect(0, index * 22, 126 * Notes.Count, 22));
            
            foreach (var item in Notes)
            {
                if (!ignorebits) item.UpdateBit(hot);
                item.Render(dc);
            }
            
            dc.Close();
        }

        public void Render(DrawingContext dc)
        {
            dc.DrawDrawing(RowRender);
        }

        internal bool DetectSelection(Rect Selection, out int cellIndex, out int bitIndex, int active=0 )
        {
            cellIndex = 0;
            bitIndex = 0;
            var sel = Notes.Where(x => x.SqDatBit_Bounds.IntersectsWith(Selection));
            foreach (SeqData item in sel)
            {
                item.DetectSelection(Selection,out bitIndex,active);
            }
            if(Selection.Width < 2 && Selection.Height < 2)
            {
                cellIndex = sel.ToList()[0].Column;
            }
            return Selection.Width > 2 || Selection.Height > 2;
        }
    }
    
    public class SeqData
    {
        #region Data Properties
        public int midiChannel;
        public int? Pitch { get; set; }
        public byte? Velocity { get; set; }
        public byte? Instrument { get; set; }
        public SeqParam Parameter { get; set; }
        #endregion

        #region Rendering Properties
        internal DrawingGroup Renderer = new DrawingGroup();
        internal DrawingGroup[] TextRenderer = new DrawingGroup[]
        {
            new DrawingGroup(),
            new DrawingGroup(),
            new DrawingGroup(),
            new DrawingGroup(),
            new DrawingGroup()
        };
        internal bool[] SelectedBits = new bool[] { false, false, false, false, false };

        public static readonly int[] BitWidths = new int[]
        {
            34,24,34,10,20
        };
        public static readonly int[] BitOffsets = new int[]
        {
            0,34,58,92,102
        };
        internal Rect SqDatBit_Bounds;  // overall dimensions
        internal Rect SqTxtBit_Bounds;  // text dimensions
        private List<Rect> boundList = new List<Rect>();
        private FormattedText[] BitFormattedText = new FormattedText[5];

        public static readonly int Width = 126;
        public static readonly int Height = 22;

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

        public int Column { get; set; }
        public int Row { get; set; }

        #endregion

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

        public void Render(DrawingContext dc)
        {
            dc.DrawDrawing(Renderer);
        }

        public void UpdateBit(bool hot = false, Rect? debugsel = null)
        {
            boundList.Clear();
            double offset = 0;
            SqDatBit_Bounds = new Rect(Column * Width, Row * Height, Width, Height);
            SqTxtBit_Bounds = new Rect((Column * Width) + 2, (Row * Height) + 1, Width - 6, Height - 2);
            Rect PitchBit_Bounds = new Rect(SqTxtBit_Bounds.X, SqDatBit_Bounds.Y, BitWidths[0], Height);
            offset += PitchBit_Bounds.Width;
            Rect InstrBit_Bounds = new Rect(SqTxtBit_Bounds.X + offset, SqDatBit_Bounds.Y, BitWidths[1], Height);
            offset += InstrBit_Bounds.Width;
            Rect VelocBit_Bounds = new Rect(SqTxtBit_Bounds.X + offset, SqDatBit_Bounds.Y, BitWidths[2], Height);
            offset += VelocBit_Bounds.Width;
            Rect SqParBit_Bounds = new Rect(SqTxtBit_Bounds.X + offset, SqDatBit_Bounds.Y, BitWidths[3], Height);
            offset += SqParBit_Bounds.Width;
            Rect SqValBit_Bounds = new Rect(SqTxtBit_Bounds.X + offset, SqDatBit_Bounds.Y, BitWidths[4], Height);
            boundList.Add(PitchBit_Bounds);
            boundList.Add(InstrBit_Bounds);
            boundList.Add(VelocBit_Bounds);
            boundList.Add(SqParBit_Bounds);
            boundList.Add(SqValBit_Bounds);
            var rc = Renderer.Open();
            //render text and selection if need
            for (int i = 0; i < SelectedBits.Length; i++)
            {
                Brush bg = SelectedBits[i] ? BR_SelBG : null;
                if(bg != null)
                {
                    rc.DrawRectangle(bg, null, boundList[i]);
                }
                
                UpdateBitText(i,hot);
                rc.DrawDrawing(TextRenderer[i]);
            }
            //draw borders
            Point tr2 = SqDatBit_Bounds.TopRight;
            Point br2 = SqDatBit_Bounds.BottomRight;

            rc.DrawLine(new Pen(BR_BdrEn, 4), tr2, br2);
            rc.DrawLine(new Pen(BR_BdrEx, 2), SqDatBit_Bounds.TopRight, SqDatBit_Bounds.BottomRight);
            if (debugsel.HasValue)
            {
                rc.DrawRectangle(Brushes.Red, null, debugsel.Value);
            }
            rc.Close();
        }
        
        private FormattedText Text(string text, Brush foreground)
        {
            return new FormattedText(text, System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, SystemComponent.MPTEditorFont, 16, foreground, 1);
        }
        
        internal bool DetectSelection(Rect Selection, out int activeBit, int active=0)
        {
            activeBit = 0;
            var lb = new bool[5];
            SelectedBits.CopyTo(lb, 0);
            for (int i = 0; i < SelectedBits.Length; i++)
            {
                SelectedBits[i] = boundList[i].IntersectsWith(Selection);
            }
            if (!Enumerable.SequenceEqual(lb, SelectedBits))
            {
                UpdateBit(Row == active);
            }
            //scroll to active corner

            if(Selection.Width < 2 && Selection.Height < 2)
            {
                activeBit = SelectedBits.ToList().IndexOf(true);
            }
            return Selection.Width > 2 || Selection.Height > 2;
        }

        internal void ClearSelection(int activeRow=0)
        {
            var lb = new bool[5];
            SelectedBits.CopyTo(lb, 0);
            SelectedBits = new bool[] { false, false, false, false, false };
            if (!Enumerable.SequenceEqual(lb, SelectedBits))
            {

                UpdateBit(Row == activeRow);
            }
        }

        internal void UpdateBitText(int i, bool hot)
        {
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
                        if (Pitch == -1)
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
                        text = "v" + Velocity.Value.ToString("X2");
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
            if (hot && fg == BR_Empty)
            {
                fg = BR_Pitch;
            }
            if (BitFormattedText[i] == null)
            {
                BitFormattedText[i] = Text(text, fg);
            }
            else
            {
                if (BitFormattedText[i].Text == text)
                {
                    BitFormattedText[i].SetForegroundBrush(fg);
                }
                else
                {
                    BitFormattedText[i] = Text(text, fg);

                }
            }
            var rc = TextRenderer[i].Open();
            rc.DrawText(BitFormattedText[i], new Point(boundList[i].TopLeft.X + 2, boundList[i].TopLeft.Y));
            rc.Close();

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
