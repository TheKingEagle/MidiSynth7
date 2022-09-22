using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanford.Multimedia.Midi;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

// =============== NOTICE: This file was ported from v6. ===============
// TODO: Re-implement a version of fluidsynth.
// TODO: Figure out any optimizations to make to this system
// TODO: Create new presets with more flexibility
// TODO: Possible tracker support?
// TODO: Get off the UI Thread!!!!!
// =============== ===================================== ===============
namespace MidiSynth7.components
{
    public class ErrorHandler
    {
        public static string ShowError(Exception error)
        {

            return error.ToString();
        }
    }

    public class MidiEngine
    {

        #region Declarations

        /// <summary>
        /// Gets whether or not the engine will use the internal synthesizer (by fluidSynth)
        /// Deprecated until further notice
        /// </summary>
        public bool InternalSF2 { get; private set; }
        
        /// <summary>
        /// This returns the copyright information that is present in the track.
        /// </summary>
        public string Copyright { get { return metaInfCr; } }

        public int SynthMainSF2 { get; set; }

        public event EventHandler<EventArgs> MidiInitalized;                                    // When the midi was started.
        public event EventHandler<EventArgs> MidiClosed;                                        // When the midi was stopped.
        public event EventHandler<NoteEventArgs> NotePlayed;                                    // When a note has played.
        public event EventHandler<NoteEventArgs> NoteStopped;                                   // when note stopped.
        public event EventHandler<EventArgs> FileLoadComplete;
        public event EventHandler<EventArgs> SequenceBuilder_Completed;                         // The record build worked and has completed.
        
        public OutputDevice device;                                                             // The output device of midi.
        public InputDevice inDevice;                                                            // The input device (external keyboards etc)
        public InputDevice inDevice2;                                                           // The input device (external keyboards etc)
        public Sequencer ModableSequencer;                                                      // For Riff center
        public Sequence riffSequence;                                                           // Loaded Riff Sequence
        public List<Sequence> presets = new List<Sequence>();
        
        private Sequencer midiSequencer;                                                        // The sequencer that plays midi files.
        private Sequence midiSequence;                                                          // Timer used for checking the loop status.
        private MidiInternalClock MiClock = new MidiInternalClock(6);                           // The internal time keeper for midi recording.
        private RecordingSession recordSession;                                                 // Recording session of the midi files.
        private BackgroundWorker buildWorker = new BackgroundWorker();
        private BackgroundWorker MetaParserWorker = new BackgroundWorker();


        private int spb = -1;
        private int spp = -1;
        private int spc = -1;
        private int sbp = -1;
        private int sbc = -1;
        private bool gotMeta = false;
        private bool isRecording = false;                                                       // used for the recording session.
        private string metaInfCr = "No Copyright information available.";                       // meta information : Copyright
        private string RSTitle;
        private string RSAuthor;
        private string RSCopyright;
        private string RSComments;
        private string PresetDirectory = App.PRESET_DIR;
       
        #endregion

        #region EventHandles
        private void OnMidiInit(EventArgs e)
        {
            MidiInitalized?.Invoke(this, e);
        }
        private void OnNotePlay(NoteEventArgs e)
        {
            NotePlayed?.Invoke(this, e);
        }
        private void OnNoteStopped(NoteEventArgs e)
        {
            NoteStopped?.Invoke(this, e);
        }
        private void OnFileLoadComplete(EventArgs e)
        {
            FileLoadComplete?.Invoke(this, e);
        }
        private void OnMidiClose(EventArgs e)
        {
            MidiClosed?.Invoke(this, e);
        }
        private void OnSequenceBuildComplete(EventArgs e)
        {
            SequenceBuilder_Completed?.Invoke(this, e);
        }
        #endregion

        /// <summary>
        /// Initalizes the Musical Instrument Digital Interface for use.
        /// </summary>
        /// <param name="DeviceIndex">The midi device to start.</param>
        public MidiEngine(int DeviceIndex)
        {
            try
            {
                Generatepresets();
                device = new OutputDevice(DeviceIndex);
                OnMidiInit(new EventArgs());
                GenerateSequencer();
                GenerateSequence();
                buildWorker.DoWork += BuildWorker_DoWork;
                buildWorker.RunWorkerCompleted += BuildWorker_RunWorkerCompleted;
                buildWorker.WorkerSupportsCancellation = false;
                MetaParserWorker.WorkerSupportsCancellation = false;
                MetaParserWorker.DoWork += MetaParserWorker_DoWork;
                MetaParserWorker.RunWorkerCompleted += MetaParserWorker_RunWorkerCompleted;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Methods

        /// <summary>
        /// returns a list of the available MIDI output devices installed on your system.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetOutputDevices()
        {
            List<string> devlist = new List<string>();
            for (int i = 0; i < OutputDevice.DeviceCount; i++)
            {
                devlist.Add(OutputDevice.GetDeviceCapabilities(i).name);
            }

            //ERROR
            if (devlist.Count() == 0)
            {
                throw new Exception("No usable MIDI devices found.");
            }
            return devlist;
        }
        
        /// <summary>
        /// Returns a list of the avilable MIDI input devices installed on your system.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetInputDevices()
        {
            List<string> devlist = new List<string>();
            for (int i = 0; i < InputDevice.DeviceCount; i++)
            {
                devlist.Add(InputDevice.GetDeviceCapabilities(i).name);
            }

            //ERROR
            if (devlist.Count() == 0)
            {
                throw new WarningException("No Input devices detected.");
            }
            return devlist;
        }

        public static (string noteLabel, int octave) GetNote(int note)
        {
            string[] noteString = new string[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

            int octave = (note / 12) - 1;
            int noteIndex = note % 12;
            string label = noteString[noteIndex];
            return (label, octave);
        }

        /// <summary>
        /// Returns the length of the sequence.
        /// </summary>
        /// <returns></returns>
        public int GetLen() => midiSequence.GetLength();

        /// <summary>
        /// Returns the length of the sequence played so far.
        /// </summary>
        /// <returns></returns>
        public int SeqPos() => midiSequencer.Position;

        /// <summary>
        /// Returns the number of tracks in the sequence.
        /// </summary>
        /// <returns></returns>
        public int GetTrackCount() => midiSequence.Count();

        /// <summary>
        /// Save sequence as midi file.
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveSequence(string fileName)
        {
            midiSequencer.Sequence[0].Insert(0, new MetaMessage(MetaType.TrackName, Encoding.Default.GetBytes(RSTitle)));
            midiSequencer.Sequence[0].Insert(1, new MetaMessage(MetaType.Text, Encoding.Default.GetBytes(RSAuthor)));
            midiSequencer.Sequence[0].Insert(2, new MetaMessage(MetaType.Copyright, Encoding.Default.GetBytes(RSCopyright)));
            midiSequencer.Sequence[0].Insert(3, new MetaMessage(MetaType.Text, Encoding.Default.GetBytes(RSComments)));
            midiSequencer.Sequence.SaveAsync(fileName);
        }

        /// <summary>
        /// Plays a midi note with specific parameters.
        /// </summary>
        /// <param name="channel">Midi channel. 0 - 15.</param>
        /// <param name="pitch"> Pitch of the note. 0 - 127</param>
        /// <param name="volume">Velocity of the note 0 - 127</param>
        public void MidiNote_Play(int channel, int pitch, int volume, bool send_event = true)
        {
            //throw(new Exception("test"));
            if (pitch > 127)
            {
                return;
            }
            if (pitch < 0)
            {
                return;
            }
            if (!InternalSF2)
            {
                device.Send(new ChannelMessage(ChannelCommand.NoteOn, channel, pitch, volume));

            }
            if (isRecording)
            {
                recordSession.Record(new ChannelMessage(ChannelCommand.Controller, channel, (int)ControllerType.Volume, volume));
                recordSession.Record(new ChannelMessage(ChannelCommand.NoteOn, channel, pitch, volume));
            }
            if(send_event) OnNotePlay(new NoteEventArgs(new ChannelMessage(ChannelCommand.NoteOn, channel, pitch, volume)));
        }

        /// <summary>
        /// Stop the specific note.
        /// </summary>
        /// <param name="channel">the channel 0 - 15</param>
        /// <param name="pitch"> the pitch to terminate. 0 - 127</param>
        public void MidiNote_Stop(int channel, int pitch, bool send_event = true)
        {
            if (pitch > 127)
            {
                return;
            }
            if (pitch < 0)
            {
                return;
            }
            if (!InternalSF2)
            {
                device.Send(new ChannelMessage(ChannelCommand.NoteOff, channel, pitch, 0));
            }

            if (isRecording)
            {
                recordSession.Record(new ChannelMessage(ChannelCommand.NoteOff, channel, pitch));
            }
            if(send_event) OnNoteStopped(new NoteEventArgs(new ChannelMessage(ChannelCommand.NoteOff, channel, pitch, 0)));
        }

        /// <summary>
        /// Play a note with specified parameters and time in milliseconds.
        /// </summary>
        /// <param name="channel">0 - 15</param>
        /// <param name="pitch">0 - 15</param>
        /// <param name="velocity">0 - 127</param>
        /// <param name="time">Time in milliseconds.</param>
        public void MidiNote_PlayTimed(int channel, int pitch, int velocity, int time)
        {

            MidiNote_Play(channel, pitch, velocity);
            new p(this, channel, pitch, velocity, time);
        }

        /// <summary>
        /// Set the instrument(Patch)
        /// </summary>
        /// <param name="bank">Bank of patch</param>
        /// <param name="patch">the instrument 0 - 127 supported.</param>
        /// <param name="ch"> 0 - 15.</param>
        public void MidiNote_SetProgram(int bank, int patch, int ch)
        {
            if (!InternalSF2)
            {
                device.Send(new ChannelMessage(ChannelCommand.Controller, ch, (int)ControllerType.BankSelect, bank));
                device.Send(new ChannelMessage(ChannelCommand.ProgramChange, ch, patch));
            }
            if (InternalSF2)
            {
                //internalDevice.BankSelect(ch, (uint)bank);
                //internalDevice.ProgChange(ch, patch);
            }
            //2012 me: I WISH THERE WAS A BETTER WAY!
            //2015 me: Holy hell.... I still haven't found a better way. Oh well... Back to coding.
            //2016 me: Welp.... im not messing with this anymore. if it works, im keeping it.
            //2018 me: What the everloving shit even is this code???!!! How the **** does this even work again????????
            //2021 me: wow real mature. The code still works though, and I cba to mess with it right now.
            if ((isRecording))
            {
                if (spb != bank && spc != ch && spp != patch)//case 1 - none are equal.
                {
                    recordSession.Record(new ChannelMessage(ChannelCommand.Controller, ch, (int)ControllerType.BankSelect, bank));
                    recordSession.Record(new ChannelMessage(ChannelCommand.ProgramChange, ch, patch));
                }
                if ((spb == bank && spc != ch && spp != patch) || (spb != bank && spc == ch && spp != patch) || (spb != bank && spc != ch && spp == patch))//case 2 - none but one are equal
                {
                    recordSession.Record(new ChannelMessage(ChannelCommand.Controller, ch, (int)ControllerType.BankSelect, bank));
                    recordSession.Record(new ChannelMessage(ChannelCommand.ProgramChange, ch, patch));
                }


                if ((spb == bank && spc == ch && spp != patch) || (spb == bank && spc != ch && spp == patch) || (spb != bank && spc == ch && spp == patch))//case 3 - any two are equal
                {
                    recordSession.Record(new ChannelMessage(ChannelCommand.Controller, ch, (int)ControllerType.BankSelect, bank));
                    recordSession.Record(new ChannelMessage(ChannelCommand.ProgramChange, ch, patch));
                }


                spb = bank;
                spp = patch;
                spc = ch;
            }
        }

        /// <summary>
        /// Set note position (left-right) (0 - 127) 64 is center.
        /// </summary>
        /// <param name="ch">0 - 15</param>
        /// <param name="pan">0 - 127</param>
        public void MidiNote_SetPan(int ch, int pan)
        {
            if (!InternalSF2)
            {
                device.Send(new ChannelMessage(ChannelCommand.Controller, ch, (int)ControllerType.Pan, pan));
            }
            if (InternalSF2)
            {
                //internalDevice.ControllerChange(ch, (int)ControllerType.Pan, pan);
            }
            if (((isRecording) && ((sbc != ch && sbp != pan)) || (((isRecording) && (sbc == ch && sbp != pan))) || (((isRecording) && (sbc != ch && sbp == pan)))))
            {
                recordSession.Record(new ChannelMessage(ChannelCommand.Controller, ch, (int)ControllerType.Pan, pan));
                sbp = pan;
                sbc = ch;
            }
        }

        /// <summary>
        /// Some devices or soundfonts may not support reverb.
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="reverb"></param>
        public void MidiNote_SetReverb(int ch, int reverb)
        {
            if (!InternalSF2)
            {
                device.Send(new ChannelMessage(ChannelCommand.Controller, ch, (int)ControllerType.EffectsLevel, reverb));
            }
            if (InternalSF2)
            {
                //internalDevice.ControllerChange(ch, (int)ControllerType.EffectsLevel, reverb);
            }
            if (isRecording)
            {
                recordSession.Record(new ChannelMessage(ChannelCommand.Controller, ch, (int)ControllerType.EffectsLevel, reverb));
            }
        }

        /// <summary>
        /// Sets the Chorus FX for the device. Note this may not do anything for specific engines(MSGS)
        /// </summary>
        /// <param name="ch">the channel to apply.</param>
        /// <param name="Chorus">The amount 0-127</param>
        public void MidiNote_SetChorus(int ch, int Chorus)
        {
            if (!InternalSF2)
            {
                device.Send(new ChannelMessage(ChannelCommand.Controller, ch, (int)ControllerType.ChorusLevel, Chorus));
            }
            if (InternalSF2)
            {
                //internalDevice.ControllerChange(ch, (int)ControllerType.ChorusLevel, Chorus);
            }
            if (isRecording)
            {
                recordSession.Record(new ChannelMessage(ChannelCommand.Controller, ch, (int)ControllerType.ChorusLevel, Chorus));
            }
        }

        /// <summary>
        /// Sustain control.
        /// </summary>
        /// <param name="ch">0 - 16</param>
        /// <param name="sustain">False = 0, true = 127(Thats the only values it will ever need...)</param>
        public void MidiNote_SetSustain(int ch, bool sustain)
        {
            if (sustain)
            {
                if (!InternalSF2)
                {
                    device.Send(new ChannelMessage(ChannelCommand.Controller, ch, (int)ControllerType.HoldPedal1, 127));
                }
                if (InternalSF2)
                {
                    //internalDevice.ControllerChange(ch, (int)ControllerType.HoldPedal1, 127);
                }
                if (isRecording)
                {
                    recordSession.Record(new ChannelMessage(ChannelCommand.Controller, ch, (int)ControllerType.HoldPedal1, 127));
                }
            }
            if (!sustain)
            {
                if (!InternalSF2)
                {
                    device.Send(new ChannelMessage(ChannelCommand.Controller, ch, (int)ControllerType.HoldPedal1, 0));
                }
                if (InternalSF2)
                {
                    //internalDevice.ControllerChange(ch, (int)ControllerType.HoldPedal1, 0);
                }
                if (isRecording)
                {
                    recordSession.Record(new ChannelMessage(ChannelCommand.Controller, ch, (int)ControllerType.HoldPedal1, 0));
                }
            }
        }

        /// <summary>
        /// Modulation Effect
        /// </summary>
        /// <param name="ch">0 - 16</param>
        /// <param name="modulation">0 - 127</param>
        public void MidiNote_SetModulation(int ch, int modulation)
        {
            if (!InternalSF2)
            {
                device.Send(new ChannelMessage(ChannelCommand.Controller, ch, (int)ControllerType.ModulationWheel, modulation));
            }
            if (InternalSF2)
            {
                //internalDevice.ControllerChange(ch, (int)ControllerType.ModulationWheel, modulation);
            }
            if (isRecording)
            {
                recordSession.Record(new ChannelMessage(ChannelCommand.Controller, ch, (int)ControllerType.ModulationWheel, modulation));
            }
        }

        /// <summary>
        /// gracefully release the midi engine and allow for reuse in some other handle.
        /// </summary>
        public void MidiEngine_Close()
        {
            if (!InternalSF2)
            {
                if (device != null)
                {
                    OnMidiClose(new EventArgs());
                    midiSequencer.ChannelMessagePlayed -= MidiSequencer_ChannelMessagePlayed;
                    midiSequencer.MetaMessagePlayed -= MidiSequencer_MetaMessagePlayed;
                    midiSequence.LoadCompleted -= MidiSequence_LoadCompleted;
                    buildWorker.DoWork -= BuildWorker_DoWork;
                    buildWorker.RunWorkerCompleted -= BuildWorker_RunWorkerCompleted;
                    device.Close();
                    //close input devices
                    if(inDevice != null)
                    {
                        if (inDevice.IsDisposed) return;

                        inDevice.StopRecording();
                        inDevice.Close();
                        inDevice = null;
                    }
                    if (inDevice2 != null)
                    {
                        if (inDevice2.IsDisposed) return;

                        inDevice2.StopRecording();
                        inDevice2.Close();
                        inDevice2 = null;
                    }
                    device = null;
                }
            }
            if (InternalSF2)
            {
                OnMidiClose(new EventArgs());
                midiSequencer.ChannelMessagePlayed -= MidiSequencer_ChannelMessagePlayed;
                midiSequencer.MetaMessagePlayed -= MidiSequencer_MetaMessagePlayed;
                midiSequence.LoadCompleted -= MidiSequence_LoadCompleted;
                buildWorker.DoWork -= BuildWorker_DoWork;
                buildWorker.RunWorkerCompleted -= BuildWorker_RunWorkerCompleted;
                //internalDevice.SFontUnload((uint)SynthMainSF2, true);
                //internalDevice = null;
            }
        }

        /// <summary>
        /// Silence the MIDI engine, sends ALL SOUNDS OFF controller.
        /// </summary>
        public void MidiEngine_Panic()
        {
            for (int i = 0; i < 16; i++)
            {

                if (InternalSF2)
                {
                    //internalDevice.ControllerChange(i, (int)ControllerType.AllNotesOff, 127);
                    //internalDevice.ControllerChange(i, (int)ControllerType.AllSoundOff, 127);
                }
                if (!InternalSF2)
                {
                    MidiEngine_SendRawChannelMessage(new ChannelMessage(ChannelCommand.Controller, i, (int)ControllerType.AllNotesOff));
                    MidiEngine_SendRawChannelMessage(new ChannelMessage(ChannelCommand.Controller, i, (int)ControllerType.AllSoundOff));
                }
            }
        }

        /// <summary>
        /// Send ALL NOTES OFF controller.
        /// </summary>
        public void MidiEngine_AllNotesOff()
        {
            for (int i = 0; i < 16; i++)
            {

                if (InternalSF2)
                {
                    //internalDevice.ControllerChange(i, (int)ControllerType.AllNotesOff, 127);
                    //internalDevice.Reset();

                }
                if (!InternalSF2)
                {
                    MidiEngine_SendRawChannelMessage(new ChannelMessage(ChannelCommand.Controller, i, (int)ControllerType.AllNotesOff));
                }
            }
        }

        /// <summary>
        /// Send a channel message to the device
        /// </summary>
        /// <param name="cm"></param>
        public void MidiEngine_SendRawChannelMessage(ChannelMessage cm)
        {
            if (InternalSF2)
            {
                if (cm.Command == ChannelCommand.Controller)
                {
                    //internalDevice.ControllerChange(cm.MidiChannel, cm.Data1, cm.Data2);
                }
                if (cm.Command == ChannelCommand.NoteOff)
                {
                    //internalDevice.NoteOff(cm.MidiChannel, (short)cm.Data1);
                }
                if (cm.Command == ChannelCommand.NoteOn)
                {
                    //internalDevice.NoteOn(cm.MidiChannel, (short)cm.Data1, (short)cm.Data2);
                }
                if (cm.Command == ChannelCommand.ProgramChange)
                {
                    //internalDevice.ProgChange(cm.MidiChannel, cm.Data1);
                }

            }
            if (device != null)
            {
                device.Send(cm);
            }
        }

        /// <summary>
        /// Extra controls not preset with midi engine functions.
        /// </summary>
        /// <param name="control">the control type</param>
        /// <param name="ch">channel (0 - 15)</param>
        /// <param name="value">0 - 127</param>
        public void MidiNote_SetControl(ControllerType control, int ch, int value)
        {
            if (!InternalSF2)
            {
                device.Send(new ChannelMessage(ChannelCommand.Controller, ch, (int)control, value));
            }
            if (InternalSF2)
            {
                //internalDevice.ControllerChange(ch, (int)control, value);
            }
            if (isRecording)
            {
                recordSession.Record(new ChannelMessage(ChannelCommand.Controller, ch, (int)control, value));
            }
        }

        /// <summary>
        /// resets the engine device.
        /// </summary>
        public void MidiEngine_Reset()
        {
            if (!InternalSF2)
            {
                if (device != null)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        MidiNote_SetPan(i, 64);
                        MidiNote_SetChorus(i, 0);
                        MidiNote_SetSustain(i, false);
                        MidiNote_SetModulation(i, 0);
                        MidiNote_SetReverb(i, 0);
                        //safe measure.
                        MidiNote_SetControl(ControllerType.AllControllersOff, i, 127);
                        MidiNote_SetControl(ControllerType.AllNotesOff, i, 127);
                        MidiNote_SetControl(ControllerType.AllSoundOff, i, 127);
                        //EXTRA safe measure. 
                    }
                    device.Reset();
                }
            }
            if (InternalSF2)
            {
                for (int i = 0; i < 16; i++)
                {
                    MidiNote_SetPan(i, 64);
                    MidiNote_SetChorus(i, 0);
                    MidiNote_SetSustain(i, false);
                    MidiNote_SetModulation(i, 0);
                    MidiNote_SetReverb(i, 0);
                    //safe measure.
                    MidiNote_SetControl(ControllerType.AllControllersOff, i, 127);
                    MidiNote_SetControl(ControllerType.AllNotesOff, i, 127);
                    MidiNote_SetControl(ControllerType.AllSoundOff, i, 127);
                    //EXTRA safe measure. 
                }
                //internalDevice.Reset();
            }

        }

        /// <summary>
        /// Loads a MIDI file into the sequencer.
        /// </summary>
        /// <param name="file">The MIDI File to add.</param>
        public void MidiFile_Add(string file)
        {
            try
            {
                if (midiSequence.IsBusy == true)
                {
                    
                    return;
                }
                GenerateSequence();
                midiSequencer.Stop();
                gotMeta = false;
                midiSequence.LoadAsync(file);

            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Record a new sequence.
        /// </summary>
        public void MidiFile_RecordBegin()
        {
            sbc = -1;
            sbp = -1;
            spp = -1;
            spb = -1;
            spc = -1;
            recordSession = new RecordingSession(MiClock);
            GenerateSequence();
            GenerateSequencer();
            MiClock.Start();
            isRecording = true;

        }

        /// <summary>
        /// Stop recording the sequence.
        /// </summary>
        public void MidiFile_RecordStop()
        {
            sbc = -1;
            sbp = -1;
            spp = -1;
            spb = -1;
            spc = -1;
            buildWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Play or loop the sequencer.
        /// </summary>
        public void MidiFile_Play()
        {
            if (midiSequence == null) return;
            try
            {
                for (int i = 0; i < 16; i++)
                {
                    MidiNote_SetControl(ControllerType.Volume, i, 127);
                }
                midiSequencer.Sequence = midiSequence;
                //FileTimer.Stop();
                midiSequencer.Start();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Stop playing a file.
        /// </summary>
        public void MidiFile_Stop() => midiSequencer.Stop();

        /// <summary>
        /// Continue playing.
        /// </summary>
        public void MidiFile_Continue() => midiSequencer.Continue();

        /// <summary>
        /// Create default meta for the new file.
        /// </summary>
        /// <param name="Auth"></param>
        /// <param name="title"></param>
        /// <param name="copy"></param>
        /// <param name="comments"></param>
        public void RecordDefaultMeta(string Auth, string title, string copy, string comments)
        {
            this.RSAuthor = Auth;
            this.RSCopyright = copy;
            this.RSTitle = title;
            this.RSComments = comments;
        }

        private void Generatepresets()
        {
            presets.Clear();
            presets.Add(new Sequence(PresetDirectory + "\\preset1.mid"));
            presets.Add(new Sequence(PresetDirectory + "\\preset2.mid"));
            presets.Add(new Sequence(PresetDirectory + "\\preset3.mid"));
            presets.Add(new Sequence(PresetDirectory + "\\preset4.mid"));
            presets.Add(new Sequence(PresetDirectory + "\\preset5.mid"));
            presets.Add(new Sequence(PresetDirectory + "\\preset6.mid"));
            presets.Add(new Sequence(PresetDirectory + "\\preset7.mid"));
            presets.Add(new Sequence(PresetDirectory + "\\preset8.mid"));
            presets.Add(new Sequence(PresetDirectory + "\\preset9.mid"));
            presets.Add(new Sequence(PresetDirectory + "\\preset10.mid"));
            presets.Add(new Sequence(PresetDirectory + "\\preset11.mid"));
            presets.Add(new Sequence(PresetDirectory + "\\preset12.mid"));
            presets.Add(new Sequence(PresetDirectory + "\\preset1.mid"));
        }

        private void GenerateSequence()
        {
            midiSequence = new Sequence();

            midiSequence.LoadCompleted += MidiSequence_LoadCompleted;
        }

        private void GenerateRiffSequence() //Used internally by the mainform?
        {
            riffSequence = new Sequence();
            riffSequence.LoadCompleted += RiffSequence_LoadCompleted;
        }
        
        private void GenerateSequencer()
        {
            midiSequencer = new Sanford.Multimedia.Midi.Sequencer();
            midiSequencer.ChannelMessagePlayed += MidiSequencer_ChannelMessagePlayed;
            midiSequencer.MetaMessagePlayed += MidiSequencer_MetaMessagePlayed;
        }
        
        private void GenerateRiffSequencer() //Used internally by the mainform?
        {
            ModableSequencer = new Sanford.Multimedia.Midi.Sequencer();
            ModableSequencer.ChannelMessagePlayed += MidiSequencer_ChannelMessagePlayed;
            midiSequencer.MetaMessagePlayed += MidiSequencer_MetaMessagePlayed;
        }

        #region Internal component events
        
        private void MidiSequencer_MetaMessagePlayed(object sender, MetaMessageEventArgs e)
        {
            if (!gotMeta)
            {
                if (e.Message.MetaType == MetaType.Copyright)
                {
                    metaInfCr = Encoding.Default.GetString(e.Message.GetBytes());
                    gotMeta = true;
                }
                else
                { metaInfCr = "No Copyright information available."; }
            }
        }

        private void MidiSequence_LoadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ErrorHandler.ShowError(new Exception("Failed to load the midi file.\n" + e.Error.Message, e.Error));
            }
            MetaParserWorker.RunWorkerAsync();

        }

        private void MidiSequencer_ChannelMessagePlayed(object sender, ChannelMessageEventArgs e)
        {
            try
            {

                device.Send(e.Message);
                OnNotePlay(new NoteEventArgs(e.Message));
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void MetaParserWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ErrorHandler.ShowError(e.Error);
            }
            OnFileLoadComplete(new EventArgs());
        }

        private void MetaParserWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (Track item in midiSequence)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    if (item.GetMidiEvent(i).MidiMessage.MessageType == MessageType.Meta)
                    {
                        MetaMessage meta = (MetaMessage)item.GetMidiEvent(i).MidiMessage;
                        if (!gotMeta)
                        {
                            if (meta.MetaType == MetaType.Copyright)
                            {
                                metaInfCr = Encoding.Default.GetString(meta.GetBytes());
                                gotMeta = true;
                            }
                            else
                            { metaInfCr = "No Copyright information available."; }
                        }
                    }
                }
            }
        }

        private void BuildWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ErrorHandler.ShowError(e.Error);
            }
            OnSequenceBuildComplete(new EventArgs());
        }

        private void BuildWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            MiClock.Stop();
            recordSession.Build();
            midiSequence.Clear();

            midiSequence.Add(recordSession.Result);
            midiSequencer.Sequence = midiSequence;
            isRecording = false;
        }

        private void RiffSequence_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // TODO: Why is this here?
        }

        #endregion

        #endregion
    }

    //Extra cryptic... isn't it?
    class p
    {
        int tt = 0;
        int cc = 0;
        int pss = 0;
        int vv = 0;
        MidiEngine oo;
        DispatcherTimer ta;
        public p(MidiEngine o, int c, int ps, int v, int t)
        {

            oo = o;
            cc = c;
            pss = ps;
            vv = v;
            tt = t;
            ta = new DispatcherTimer();
            ta.Interval = TimeSpan.FromMilliseconds(t);
            ta.Tick += ta_Tick;
            ta.Start();
        }

        void ta_Tick(object sender, EventArgs e)
        {
            oo.MidiNote_Stop(cc, pss);
            ta.Stop();
        }

    }

    #region Custom event args
    public class NoteEventArgs : EventArgs
    {
        ChannelMessage cm;
        bool sq = false;
        public NoteEventArgs(ChannelMessage chMsg)
        {
            cm = chMsg;
        }
        public NoteEventArgs(ChannelMessage chMsg, bool isSequence)
        {
            cm = chMsg;
            sq = isSequence;
        }
        public ChannelMessage ChannelMssge
        {
            get
            {
                return cm;
            }
        }
        public bool Sequence
        {
            get
            {
                return sq;
            }
        }
    }

    #endregion
}
