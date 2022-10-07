using System;
using System.Collections.Generic;
using Sanford.Multimedia.Midi;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

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
                        if (item.Pitch == -2)
                        {
                            engine.MidiNote_SetControl(ControllerType.AllSoundOff, item.midiChannel, 127, output);//in theory just for the channel?
                        }
                    }
                }
            }
            return ControlList;
        }
    }
    public class SeqData
    {
        public int Column { get; set; }
        public int Row { get; set; }
        public int midiChannel;
        private int? _pitch = 0;
        private byte? _velocity = 0;
        private byte? _trackerInstrument;
        private SeqParam _seqParam;

        public int? Pitch
        {
            get => _pitch;
            set
            {
                _pitch = value;
            }
        }

        public byte? Velocity
        {
            get => _velocity;
            set
            {
                _velocity = value;
            }
        }
        public byte? Instrument
        {
            get => _trackerInstrument; 
            set
            {
                _trackerInstrument = value;
            }
        }
        public SeqParam Parameter
        {
            get => _seqParam; 
            set
            {
                _seqParam = value;
            }
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
                velocity = "v" + Velocity.Value/2;//Since OpenMPT does not support values over 64
            }
            string param = "...";
            if(Parameter != null)
            {
                param = Parameter.Mark.ToString() + Parameter.Value.ToString("X:2");
            }
            return "|"+note + instindex + velocity + param;
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
